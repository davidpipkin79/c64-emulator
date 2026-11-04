using System;

namespace C64Emulator
{
    /// <summary>
    /// MOS 6581/8580 SID model. Three 24-bit phase-accumulator oscillators feed
    /// ADSR envelopes, optional ring modulation/sync, and a shared resonant
    /// filter. The CPU clock advances the model; 16-bit mono samples are queued
    /// for the SDL host at <see cref="SampleRate"/>.
    /// </summary>
    internal sealed class SID
    {
        public const int RegisterCount = 29;
        public const int SampleRate = 48000;

        private const int ClockHz = 985248;
        private const double CyclesPerSample = (double)ClockHz / SampleRate;
        private const double PhasePeriod = 1 << 24;
        private const int QueueCapacity = 16384;
        private const int ScratchCapacity = 64;
        // Leave enough headroom for three simultaneous voices. At 80% host
        // volume the old 0.28 gain could hard-clip the loader music, which
        // produced crackle even though the sample rate and queue were fine.
        private const double MasterGain = 0.20;

        // The SID's envelope rates are commonly quoted as times. Attack is
        // linear; decay and release use an exponential curve.
        private static readonly double[] AttackSeconds =
        {
            0.002, 0.008, 0.016, 0.024, 0.038, 0.056, 0.068, 0.080,
            0.100, 0.250, 0.500, 0.800, 1.000, 3.000, 5.000, 8.000
        };

        private static readonly double[] DecayReleaseSeconds =
        {
            0.006, 0.024, 0.048, 0.072, 0.114, 0.168, 0.204, 0.240,
            0.300, 0.750, 1.500, 2.400, 3.000, 9.000, 15.000, 24.000
        };

        private readonly byte[] registers = new byte[RegisterCount];
        private readonly Voice[] voices = { new Voice(), new Voice(), new Voice() };
        private readonly Filter filter = new Filter();
        private readonly short[] scratch = new short[ScratchCapacity];
        private readonly short[] queue = new short[QueueCapacity];
        private readonly object queueLock = new object();

        private int queueHead;
        private int queueTail;
        private int queueCount;
        private double sampleAccumulator;
        private volatile int outputVolume = 100;
        private volatile bool muted;

        public SID()
        {
            Reset();
        }

        /// <summary>
        /// Host-side master volume, independent of the SID's $D418 register.
        /// Values are clamped to 0-100 and mapped with a perceptual taper.
        /// </summary>
        public int OutputVolume
        {
            get => outputVolume;
            set => outputVolume = Math.Clamp(value, 0, 100);
        }

        public bool Muted
        {
            get => muted;
            set => muted = value;
        }

        /// <summary>
        /// Optional diagnostic hook invoked for every CPU write to a SID
        /// register. Used by the sound trace to record the writer's PC and the
        /// processor-port banking at the time of the write.
        /// </summary>
        public Action<int, byte>? RegisterWriteHook { get; set; }

        public byte GetRegister(int register)
        {
            if ((uint)register >= RegisterCount)
                return 0xFF;

            return register switch
            {
                // No paddles are connected; the pulled-up POT inputs read high.
                0x19 or 0x1A => 0xFF,
                0x1B => (byte)((int)voices[2].Phase >> 16),
                0x1C => (byte)Math.Clamp((int)Math.Round(voices[2].Envelope), 0, 255),
                _ => registers[register]
            };
        }

        public void SetRegister(int register, byte value)
        {
            if ((uint)register >= RegisterCount)
                return;

            RegisterWriteHook?.Invoke(register, value);
            registers[register] = value;

            if (register <= 20)
            {
                UpdateVoice(register / 7, register % 7, value);
                return;
            }

            switch (register)
            {
                case 0x15:
                case 0x16:
                case 0x17:
                    UpdateFilter();
                    break;
            }
        }

        /// <summary>
        /// Advances the oscillators by the number of CPU cycles just executed.
        /// Samples are generated at the fixed host rate.
        /// </summary>
        public void Clock(int cycles)
        {
            if (cycles <= 0)
                return;

            sampleAccumulator += cycles;

            int generated = 0;

            while (sampleAccumulator >= CyclesPerSample)
            {
                sampleAccumulator -= CyclesPerSample;
                scratch[generated++] = GenerateSample();

                if (generated == scratch.Length)
                {
                    Enqueue(scratch, generated);
                    generated = 0;
                }
            }

            if (generated > 0)
                Enqueue(scratch, generated);
        }

        /// <summary>
        /// Copies queued mono samples for the audio device. Returns the number
        /// of samples written to <paramref name="destination"/>.
        /// </summary>
        public int ReadSamples(Span<short> destination)
        {
            if (destination.IsEmpty)
                return 0;

            lock (queueLock)
            {
                int count = Math.Min(destination.Length, queueCount);

                for (int i = 0; i < count; i++)
                {
                    destination[i] = queue[queueHead];
                    queueHead = (queueHead + 1) % QueueCapacity;
                }

                queueCount -= count;

                if (queueCount == 0)
                    queueHead = queueTail = 0;

                return count;
            }
        }

        /// <summary>
        /// Drops queued audio. Used when warp ends or when the local machine is
        /// not the audio source.
        /// </summary>
        public void DiscardSamples()
        {
            lock (queueLock)
            {
                queueHead = 0;
                queueTail = 0;
                queueCount = 0;
            }
        }

        public void Reset()
        {
            Array.Clear(registers);

            foreach (Voice voice in voices)
                voice.Reset();

            filter.Reset();
            filter.Configure(0, 0);

            lock (queueLock)
            {
                queueHead = 0;
                queueTail = 0;
                queueCount = 0;
            }

            sampleAccumulator = 0;
        }

        private void UpdateVoice(int index, int offset, byte value)
        {
            Voice voice = voices[index];

            switch (offset)
            {
                case 0:
                    voice.Frequency = (voice.Frequency & 0xFF00) | value;
                    break;

                case 1:
                    voice.Frequency = (voice.Frequency & 0x00FF) | (value << 8);
                    break;

                case 2:
                    voice.PulseWidth = (voice.PulseWidth & 0x0F00) | value;
                    break;

                case 3:
                    voice.PulseWidth = (voice.PulseWidth & 0x00FF) | ((value & 0x0F) << 8);
                    break;

                case 4:
                    bool gate = (value & 0x01) != 0;

                    if (gate && !voice.Gate)
                        voice.State = EnvelopeState.Attack;
                    else if (!gate && voice.Gate)
                        voice.State = EnvelopeState.Release;

                    voice.Gate = gate;
                    voice.Control = value;

                    if ((value & 0x08) != 0)
                    {
                        voice.Phase = 0;
                        voice.Wrapped = false;
                    }

                    break;

                case 5:
                    voice.Attack = (value >> 4) & 0x0F;
                    voice.Decay = value & 0x0F;
                    UpdateEnvelopeRates(voice);
                    break;

                case 6:
                    voice.Sustain = (value >> 4) & 0x0F;
                    voice.Release = value & 0x0F;
                    UpdateEnvelopeRates(voice);

                    if (voice.State == EnvelopeState.Sustain)
                        voice.Envelope = SustainLevel(voice.Sustain);

                    break;
            }
        }

        private void UpdateFilter()
        {
            int cutoff = ((registers[0x15] & 0x07) << 8) | registers[0x16];
            int resonance = (registers[0x17] >> 4) & 0x0F;
            filter.Configure(cutoff, resonance);
        }

        private static void UpdateEnvelopeRates(Voice voice)
        {
            double attack = AttackSeconds[voice.Attack];
            voice.AttackIncrement = 255.0 / Math.Max(1.0, attack * SampleRate);

            double decayTau = DecayReleaseSeconds[voice.Decay] / 4.6;
            voice.DecayCoefficient = Math.Exp(-1.0 / Math.Max(1.0, decayTau * SampleRate));

            double releaseTau = DecayReleaseSeconds[voice.Release] / 4.6;
            voice.ReleaseCoefficient = Math.Exp(-1.0 / Math.Max(1.0, releaseTau * SampleRate));
        }

        private static double SustainLevel(int sustain)
        {
            return (sustain << 4) | sustain;
        }

        private static void AdvanceEnvelope(Voice voice)
        {
            switch (voice.State)
            {
                case EnvelopeState.Attack:
                    voice.Envelope += voice.AttackIncrement;

                    if (voice.Envelope >= 255.0)
                    {
                        voice.Envelope = 255.0;
                        voice.State = EnvelopeState.Decay;
                    }

                    break;

                case EnvelopeState.Decay:
                    double sustain = SustainLevel(voice.Sustain);
                    voice.Envelope = sustain + ((voice.Envelope - sustain) * voice.DecayCoefficient);

                    if (voice.Envelope - sustain < 0.5)
                    {
                        voice.Envelope = sustain;
                        voice.State = EnvelopeState.Sustain;
                    }

                    break;

                case EnvelopeState.Release:
                    voice.Envelope *= voice.ReleaseCoefficient;

                    if (voice.Envelope < 0.5)
                    {
                        voice.Envelope = 0;
                        voice.State = EnvelopeState.Idle;
                    }

                    break;
            }
        }

        private short GenerateSample()
        {
            foreach (Voice voice in voices)
                AdvanceEnvelope(voice);

            for (int i = 0; i < voices.Length; i++)
            {
                Voice voice = voices[i];

                if ((voice.Control & 0x08) != 0)
                {
                    voice.Phase = 0;
                    voice.Wrapped = false;
                    continue;
                }

                voice.Phase += voice.Frequency * CyclesPerSample;
                voice.Wrapped = voice.Phase >= PhasePeriod;

                if (voice.Wrapped)
                    voice.Phase -= PhasePeriod;

                UpdateNoise(voice);
            }

            // Sync sources are voice 3 -> voice 1, voice 1 -> voice 2 and
            // voice 2 -> voice 3.
            for (int i = 0; i < voices.Length; i++)
            {
                Voice voice = voices[i];

                if ((voice.Control & 0x02) != 0 && voices[(i + 2) % 3].Wrapped)
                {
                    voice.Phase = 0;
                    voice.Wrapped = false;
                }
            }

            int routing = registers[0x17] & 0x07;
            int mode = registers[0x18];
            int filterMode = mode & 0x70;
            double direct = 0;
            double filteredInput = 0;

            for (int i = 0; i < voices.Length; i++)
            {
                Voice voice = voices[i];

                // Register $D418 bit 7 disconnects voice 3 from the output.
                if (i == 2 && (mode & 0x80) != 0)
                    continue;

                if ((voice.Control & 0x08) != 0)
                    continue;

                int phase = (int)voice.Phase;

                if ((voice.Control & 0x04) != 0)
                    phase ^= ((int)voices[(i + 2) % 3].Phase) & 0x800000;

                int waveform = Waveform(voice, phase);
                double level =
                    ((waveform - 2048) / 2048.0) *
                    (voice.Envelope / 255.0);

                if ((routing & (1 << i)) != 0)
                    filteredInput += level;
                else
                    direct += level;
            }

            if (filterMode != 0)
                direct += filter.Process(filteredInput, filterMode);
            else
                direct += filteredInput;

            double volume = (mode & 0x0F) / 15.0;
            double hostVolume = muted ? 0.0 : HostGain(outputVolume);
            double output = direct * volume * MasterGain * hostVolume;
            double scaled = SoftLimit(output) * short.MaxValue;

            return (short)Math.Round(scaled);
        }

        /// <summary>
        /// Maps the 0-100 host volume onto an amplitude gain with a perceptual
        /// taper. A linear mapping leaves 5% at only -26 dB, which still sounds
        /// loud; loudness is roughly logarithmic, so squaring the fraction
        /// keeps 100% at full scale and makes the low end genuinely quiet
        /// (50% is a quarter of the amplitude, 5% is 1/400).
        /// </summary>
        private static double HostGain(int percent)
        {
            double fraction = Math.Clamp(percent, 0, 100) / 100.0;

            return fraction * fraction;
        }

        /// <summary>
        /// The resonant filter can boost a peak well past full scale. A hard
        /// clamp folds those harmonics back as crackle; keep the signal
        /// linear below the knee and roll the peaks off smoothly instead.
        /// </summary>
        private static double SoftLimit(double value)
        {
            const double knee = 0.70;
            const double headroom = 1.0 - knee;
            double magnitude = Math.Abs(value);

            if (magnitude <= knee)
                return value;

            double limited =
                knee + (headroom * Math.Tanh((magnitude - knee) / headroom));

            return value < 0 ? -limited : limited;
        }

        private static int Waveform(Voice voice, int phase)
        {
            int control = voice.Control;
            int waveform = 0x0FFF;
            bool selected = false;

            if ((control & 0x10) != 0)
            {
                int triangle = (phase >> 11) & 0x0FFF;

                if ((phase & 0x800000) != 0)
                    triangle ^= 0x0FFF;

                waveform &= triangle;
                selected = true;
            }

            if ((control & 0x20) != 0)
            {
                waveform &= (phase >> 12) & 0x0FFF;
                selected = true;
            }

            if ((control & 0x40) != 0)
            {
                waveform &= ((phase >> 12) & 0x0FFF) < voice.PulseWidth
                    ? 0x0FFF
                    : 0;
                selected = true;
            }

            if ((control & 0x80) != 0)
            {
                waveform &= (int)(voice.Noise >> 11) & 0x0FFF;
                selected = true;
            }

            return selected ? waveform : 0;
        }

        private static void UpdateNoise(Voice voice)
        {
            int phase = (int)voice.Phase;
            bool bit = (phase & 0x80000) != 0;

            if (bit == voice.NoiseBit)
                return;

            voice.NoiseBit = bit;

            uint feedback = ((voice.Noise >> 22) ^ (voice.Noise >> 17)) & 1;
            voice.Noise = ((voice.Noise << 1) | feedback) & 0x7FFFFF;
        }

        private void Enqueue(short[] samples, int count)
        {
            lock (queueLock)
            {
                for (int i = 0; i < count; i++)
                {
                    if (queueCount == QueueCapacity)
                    {
                        queueHead = (queueHead + 1) % QueueCapacity;
                        queueCount--;
                    }

                    queue[queueTail] = samples[i];
                    queueTail = (queueTail + 1) % QueueCapacity;
                    queueCount++;
                }
            }
        }

        private enum EnvelopeState
        {
            Idle,
            Attack,
            Decay,
            Sustain,
            Release
        }

        private sealed class Voice
        {
            public int Frequency;
            public int PulseWidth;
            public int Control;
            public int Attack;
            public int Decay;
            public int Sustain;
            public int Release;

            public double Phase;
            public double Envelope;
            public double AttackIncrement;
            public double DecayCoefficient;
            public double ReleaseCoefficient;

            public uint Noise;
            public bool NoiseBit;
            public bool Wrapped;
            public bool Gate;
            public EnvelopeState State;

            public void Reset()
            {
                Frequency = 0;
                PulseWidth = 0;
                Control = 0;
                Attack = 0;
                Decay = 0;
                Sustain = 0;
                Release = 0;
                Phase = 0;
                Envelope = 0;
                Noise = 0x7FFFF8;
                NoiseBit = false;
                Wrapped = false;
                Gate = false;
                State = EnvelopeState.Idle;
                UpdateEnvelopeRates(this);
            }
        }

        private sealed class Filter
        {
            private double ic1eq;
            private double ic2eq;
            private double a1;
            private double a2;
            private double a3;
            private double k;
            private int lastCutoff = -1;
            private int lastResonance = -1;

            public void Configure(int cutoff, int resonance)
            {
                if (cutoff == lastCutoff && resonance == lastResonance)
                    return;

                lastCutoff = cutoff;
                lastResonance = resonance;

                double normalized = Math.Clamp(cutoff / 2047.0, 0.0, 1.0);
                double cutoffHz = 30.0 + (normalized * normalized * 12000.0);
                cutoffHz = Math.Clamp(cutoffHz, 20.0, SampleRate * 0.45);

                double g = Math.Tan(Math.PI * cutoffHz / SampleRate);
                k = 2.0 - (1.9 * (resonance / 15.0));

                a1 = 1.0 / (1.0 + (g * (g + k)));
                a2 = g * a1;
                a3 = g * a2;
            }

            public double Process(double input, int mode)
            {
                double v3 = input - ic2eq;
                double v1 = (a1 * ic1eq) + (a2 * v3);
                double v2 = ic2eq + (a2 * ic1eq) + (a3 * v3);

                ic1eq = (2.0 * v1) - ic1eq;
                ic2eq = (2.0 * v2) - ic2eq;

                double output = 0;

                if ((mode & 0x10) != 0)
                    output += v2;

                if ((mode & 0x20) != 0)
                    output += v1;

                if ((mode & 0x40) != 0)
                    output += input - (k * v1) - v2;

                return output;
            }

            public void Reset()
            {
                ic1eq = 0;
                ic2eq = 0;
                lastCutoff = -1;
                lastResonance = -1;
            }
        }
    }
}
