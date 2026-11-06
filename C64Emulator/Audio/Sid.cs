using System;
using System.Threading;

namespace C64Emulator.Audio;

/// <summary>
/// The two SID revisions the C64 shipped with. The 6581 is the breadbin
/// chip: a darker, saturating filter and quieter combined waveforms. The
/// 8580 in later C64C boards is cleaner and its cutoff reaches higher.
/// </summary>
internal enum SidModel : byte
{
    Mos6581 = 0,
    Mos8580 = 1
}

/// <summary>
/// MOS 6581/8580 SID model. Three 24-bit phase-accumulator oscillators feed
/// ADSR envelopes, optional ring modulation/sync, and a shared resonant
/// filter. The CPU clock advances the model one cycle at a time; 16-bit mono
/// samples are queued for the SDL host at <see cref="SampleRate"/>.
/// </summary>
internal sealed class Sid
{
    public const int RegisterCount = 29;
    public const int SampleRate = 48000;

    private const int DefaultClockHz = 985_248;
    private const int PhaseMask = 0xFFFFFF;
    private const int PhaseHalf = 0x800000;
    private const int QueueCapacity = 16384;
    private const int ScratchCapacity = 64;

    // Leave enough headroom for three simultaneous voices. At 80% host
    // volume a higher gain could hard-clip loader music.
    private const double MasterGain = 0.20;

    /// <summary>
    /// reSID's envelope rate table: CPU cycles in one rate-counter period for
    /// each 4-bit attack/decay/release value. A full attack is 256 periods,
    /// so rate 0 takes about 2 ms and rate 15 about 8 s at 1 MHz.
    /// </summary>
    private static readonly int[] RateCounterPeriod =
    [
        9, 32, 63, 95, 149, 220, 267, 313,
        392, 977, 1954, 3126, 3907, 11720, 19532, 31251
    ];

    private static readonly int[] SustainLevel =
    [
        0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
        0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
    ];

    private readonly byte[] registers = new byte[RegisterCount];
    private readonly Voice[] voices = [new Voice(), new Voice(), new Voice()];
    private readonly Filter filter = new Filter();
    private readonly short[] scratch = new short[ScratchCapacity];
    private readonly short[] queue = new short[QueueCapacity];
    private readonly Lock queueLock = new();

    private int queueHead;
    private int queueTail;
    private int queueCount;
    private double sampleAccumulator;
    private SidModel model = SidModel.Mos6581;

    [System.NonSerialized] private int clockFrequency = DefaultClockHz;
    [System.NonSerialized] private double cyclesPerSample =
        (double)DefaultClockHz / SampleRate;
    private volatile int outputVolume = 100;
    private volatile bool muted;

    public Sid()
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
    /// Chip revision. Switching models reconfigures the filter immediately,
    /// like reSID, without resetting the oscillator or envelope state.
    /// </summary>
    public SidModel Model
    {
        get => model;
        set
        {
            if (model == value)
                return;

            model = value;
            filter.SetModel(value);
            UpdateFilter();
        }
    }

    /// <summary>
    /// CPU clock driving the phase accumulators. An NTSC SID runs at
    /// 1.0227 MHz, which raises every voice by 3.8% if it is not applied.
    /// </summary>
    public int ClockFrequency
    {
        get => clockFrequency;
        set
        {
            if (value <= 0 || value == clockFrequency)
                return;

            clockFrequency = value;
            cyclesPerSample = (double)value / SampleRate;
        }
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
            0x1B => (byte)(voices[2].Phase >> 16),
            0x1C => (byte)Math.Clamp(voices[2].EnvelopeCounter, 0, 255),
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
    /// Advances the model by the number of CPU cycles just executed. The
    /// envelopes and oscillators run at the CPU clock; samples are taken at
    /// the fixed host rate.
    /// </summary>
    public void Clock(int cycles)
    {
        if (cycles <= 0)
            return;

        var generated = 0;

        for (var cycle = 0; cycle < cycles; cycle++)
        {
            ClockCycle();

            sampleAccumulator += 1.0;

            if (sampleAccumulator < cyclesPerSample)
                continue;

            sampleAccumulator -= cyclesPerSample;
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
            var count = Math.Min(destination.Length, queueCount);

            for (var i = 0; i < count; i++)
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

    private void ClockCycle()
    {
        // Envelopes and oscillators are clocked every cycle, as on the chip;
        // the output is only sampled at the host rate. This is what makes
        // envelope timing (and the ADSR delay bug) CPU-cycle accurate.
        foreach (Voice voice in voices)
            ClockEnvelope(voice);

        foreach (Voice voice in voices)
            ClockWaveform(voice);

        // Sync sources: voice 3 -> voice 1, voice 1 -> voice 2, and
        // voice 2 -> voice 3. The source's MSB edge resets the phase.
        for (var i = 0; i < voices.Length; i++)
        {
            Voice voice = voices[i];

            if ((voice.Control & 0x02) != 0 && voices[(i + 2) % 3].MsbRising)
                voice.Phase = 0;
        }
    }

    private static void ClockWaveform(Voice voice)
    {
        if ((voice.Control & 0x08) != 0)
        {
            // TEST holds and clears the accumulator.
            voice.Phase = 0;
            voice.MsbRising = false;
            return;
        }

        var previous = voice.Phase;

        voice.Phase = (voice.Phase + voice.Frequency) & PhaseMask;
        voice.MsbRising =
            previous < PhaseHalf && voice.Phase >= PhaseHalf;

        // The noise shift register is clocked by bit 19 rising, not by the
        // sampled output rate.
        if ((previous & 0x080000) == 0 && (voice.Phase & 0x080000) != 0)
        {
            var feedback =
                ((voice.NoiseRegister >> 22) ^ (voice.NoiseRegister >> 17)) & 1;

            voice.NoiseRegister =
                ((voice.NoiseRegister << 1) | feedback) & 0x7FFFFF;
        }
    }

    /// <summary>
    /// One cycle of the ADSR state machine: a 15-bit rate counter, an
    /// exponential divider (longer steps as the envelope falls) and the
    /// 8-bit envelope counter. Modelled on reSID's envelope generator.
    /// </summary>
    private static void ClockEnvelope(Voice voice)
    {
        voice.RateCounter++;

        // The prescaler skips zero on wrap, which is what produces the
        // long ADSR stall when a period is shortened below a running count.
        if (voice.RateCounter >= 0x8000)
            voice.RateCounter = (voice.RateCounter + 1) & 0x7FFF;

        if (voice.RateCounter != voice.RatePeriod)
            return;

        voice.RateCounter = 0;

        if (voice.State != EnvelopeState.Attack)
        {
            voice.ExponentialCounter++;

            if (voice.ExponentialCounter != voice.ExponentialCounterPeriod)
                return;
        }

        voice.ExponentialCounter = 0;

        if (voice.HoldZero)
            return;

        switch (voice.State)
        {
            case EnvelopeState.Attack:
                voice.EnvelopeCounter++;

                if (voice.EnvelopeCounter >= 0xFF)
                {
                    voice.EnvelopeCounter = 0xFF;
                    voice.State = EnvelopeState.DecaySustain;
                    voice.RatePeriod = RateCounterPeriod[voice.Decay];
                }

                break;

            case EnvelopeState.DecaySustain:
                if (voice.EnvelopeCounter > SustainLevel[voice.Sustain])
                    voice.EnvelopeCounter--;

                break;

            case EnvelopeState.Release:
                if (voice.EnvelopeCounter > 0)
                    voice.EnvelopeCounter--;

                break;
        }

        UpdateExponentialPeriod(voice);
    }

    /// <summary>
    /// The exponential divider is reloaded only when the envelope counter
    /// lands exactly on one of the chip's threshold values. The resulting
    /// curve matches the datasheet decay/release times (24 s at rate 15).
    /// </summary>
    private static void UpdateExponentialPeriod(Voice voice)
    {
        switch (voice.EnvelopeCounter)
        {
            case 0xFF: voice.ExponentialCounterPeriod = 1; break;
            case 0x5D: voice.ExponentialCounterPeriod = 2; break;
            case 0x36: voice.ExponentialCounterPeriod = 4; break;
            case 0x1A: voice.ExponentialCounterPeriod = 8; break;
            case 0x0E: voice.ExponentialCounterPeriod = 16; break;
            case 0x06: voice.ExponentialCounterPeriod = 30; break;
            case 0x00:
                voice.ExponentialCounterPeriod = 1;
                voice.HoldZero = true;
                break;
        }
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
                voice.PulseWidth =
                    (voice.PulseWidth & 0x00FF) | ((value & 0x0F) << 8);
                break;

            case 4:
                var gate = (value & 0x01) != 0;

                if (gate && !voice.Gate)
                {
                    voice.State = EnvelopeState.Attack;
                    voice.RatePeriod = RateCounterPeriod[voice.Attack];
                    voice.HoldZero = false;
                }
                else if (!gate && voice.Gate)
                {
                    voice.State = EnvelopeState.Release;
                    voice.RatePeriod = RateCounterPeriod[voice.Release];
                }

                voice.Gate = gate;
                voice.Control = value;

                if ((value & 0x08) != 0)
                    voice.Phase = 0;

                break;

            case 5:
                voice.Attack = (value >> 4) & 0x0F;
                voice.Decay = value & 0x0F;

                if (voice.State == EnvelopeState.Attack)
                    voice.RatePeriod = RateCounterPeriod[voice.Attack];
                else if (voice.State == EnvelopeState.DecaySustain)
                    voice.RatePeriod = RateCounterPeriod[voice.Decay];

                break;

            case 6:
                voice.Sustain = (value >> 4) & 0x0F;
                voice.Release = value & 0x0F;

                if (voice.State == EnvelopeState.Release)
                    voice.RatePeriod = RateCounterPeriod[voice.Release];

                break;
        }
    }

    private void UpdateFilter()
    {
        var cutoff = ((registers[0x15] & 0x07) << 8) | registers[0x16];
        var resonance = (registers[0x17] >> 4) & 0x0F;

        filter.Configure(cutoff, resonance);
    }

    private short GenerateSample()
    {
        var routing = registers[0x17] & 0x07;
        int mode = registers[0x18];
        var filterMode = mode & 0x70;
        double direct = 0;
        double filteredInput = 0;

        for (var i = 0; i < voices.Length; i++)
        {
            Voice voice = voices[i];

            // Register $D418 bit 7 disconnects voice 3 from the output.
            if (i == 2 && (mode & 0x80) != 0)
                continue;

            if ((voice.Control & 0x08) != 0)
                continue;

            var phase = voice.Phase;

            if ((voice.Control & 0x04) != 0)
                phase ^= voices[(i + 2) % 3].Phase & PhaseHalf;

            var waveform = Waveform(voice, phase);
            var level =
                ((waveform - 2048) / 2048.0) *
                (voice.EnvelopeCounter / 255.0);

            // Combined waveforms are an AND of the digital outputs here.
            // Real chips use measured tables; the 6581's combinations are
            // markedly quieter and more eroded than the 8580's, which this
            // per-model attenuation approximates.
            if (SelectedWaveformCount(voice.Control) > 1)
                level *= model == SidModel.Mos6581 ? 0.55 : 0.85;

            if ((routing & (1 << i)) != 0)
                filteredInput += level;
            else
                direct += level;
        }

        if (filterMode != 0)
            direct += filter.Process(filteredInput, filterMode);
        else
            direct += filteredInput;

        var volume = (mode & 0x0F) / 15.0;
        var hostVolume = muted ? 0.0 : HostGain(outputVolume);
        var output = direct * volume * MasterGain * hostVolume;
        var scaled = SoftLimit(output) * short.MaxValue;

        return (short)Math.Round(scaled);
    }

    private static int SelectedWaveformCount(int control)
    {
        var count = 0;

        if ((control & 0x10) != 0) count++;
        if ((control & 0x20) != 0) count++;
        if ((control & 0x40) != 0) count++;
        if ((control & 0x80) != 0) count++;

        return count;
    }

    /// <summary>
    /// Maps the 0-100 host volume onto an amplitude gain with a perceptual
    /// taper. A linear mapping leaves 5% at only -26 dB, which still sounds
    /// loud; loudness is roughly logarithmic, so squaring the fraction
    /// keeps 100% at full scale and makes the low end genuinely quiet.
    /// </summary>
    private static double HostGain(int percent)
    {
        var fraction = Math.Clamp(percent, 0, 100) / 100.0;

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
        var magnitude = Math.Abs(value);

        if (magnitude <= knee)
            return value;

        var limited =
            knee + (headroom * Math.Tanh((magnitude - knee) / headroom));

        return value < 0 ? -limited : limited;
    }

    private static int Waveform(Voice voice, int phase)
    {
        var control = voice.Control;
        var waveform = 0x0FFF;
        var selected = false;

        if ((control & 0x10) != 0)
        {
            var triangle = (phase >> 11) & 0x0FFF;

            if ((phase & PhaseHalf) != 0)
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
            waveform &= (voice.NoiseRegister >> 11) & 0x0FFF;
            selected = true;
        }

        return selected ? waveform : 0;
    }

    private void Enqueue(short[] samples, int count)
    {
        lock (queueLock)
        {
            for (var i = 0; i < count; i++)
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
        Attack,
        DecaySustain,
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

        public int Phase;
        public int NoiseRegister;

        public int EnvelopeCounter;
        public int RateCounter;
        public int RatePeriod;
        public int ExponentialCounter;
        public int ExponentialCounterPeriod;
        public bool HoldZero;
        public bool Gate;
        public bool MsbRising;
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
            NoiseRegister = 0x7FFFF8;
            EnvelopeCounter = 0;
            RateCounter = 0;
            RatePeriod = RateCounterPeriod[0];
            ExponentialCounter = 0;
            ExponentialCounterPeriod = 1;
            HoldZero = true;
            Gate = false;
            MsbRising = false;
            State = EnvelopeState.Release;
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
        private SidModel model = SidModel.Mos6581;

        public void SetModel(SidModel value)
        {
            if (model == value)
                return;

            model = value;
            lastCutoff = -1;
            lastResonance = -1;
        }

        public void Configure(int cutoff, int resonance)
        {
            if (cutoff == lastCutoff && resonance == lastResonance)
                return;

            lastCutoff = cutoff;
            lastResonance = resonance;

            var cutoffHz = CutoffHz(cutoff);

            var g = Math.Tan(Math.PI * cutoffHz / SampleRate);
            k = 2.0 - (1.9 * (resonance / 15.0));

            a1 = 1.0 / (1.0 + (g * (g + k)));
            a2 = g * a1;
            a3 = g * a2;
        }

        /// <summary>
        /// The 6581 cutoff covers roughly 220 Hz to 6 kHz with a tanh-shaped
        /// register curve; the 8580 is closer to linear and reaches 12 kHz.
        /// Both are approximations of measured chips, not a full ladder
        /// model, but they give each revision its own voice.
        /// </summary>
        private double CutoffHz(int cutoff)
        {
            var normalized = Math.Clamp(cutoff / 2047.0, 0.0, 1.0);
            double cutoffHz;

            if (model == SidModel.Mos6581)
            {
                var shaped = Math.Tanh(((normalized * 2.0) - 1.0) * Math.PI);

                cutoffHz =
                    220.0 +
                    (((shaped + 1.0) * 0.5) * (6000.0 - 220.0));
            }
            else
            {
                cutoffHz = 220.0 + (normalized * (12000.0 - 220.0));
            }

            return Math.Clamp(cutoffHz, 20.0, SampleRate * 0.45);
        }

        public double Process(double input, int mode)
        {
            // The 6581's input stage saturates before the ladder; the 8580
            // is linear. Unity slope at zero keeps quiet passages clean.
            if (model == SidModel.Mos6581)
                input = Math.Tanh(input * 1.5) / 1.5;

            var v3 = input - ic2eq;
            var v1 = (a1 * ic1eq) + (a2 * v3);
            var v2 = ic2eq + (a2 * ic1eq) + (a3 * v3);

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
