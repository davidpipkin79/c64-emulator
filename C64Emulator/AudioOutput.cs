using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SDL3;

namespace C64Emulator
{
    internal readonly record struct AudioDeviceInfo(uint Id, string Name)
    {
        public bool IsDefault => Id == SDL.AudioDeviceDefaultPlayback;
    }

    /// <summary>
    /// Owns the SDL audio stream and feeds it from the SID's bounded queue.
    /// Must be used from the SDL host thread only.
    /// </summary>
    internal sealed class AudioOutput : IDisposable
    {
        private const string SystemDefaultName = "System Default";
        private static readonly int MaxQueuedSamples = SID.SampleRate * 100 / 1000;
        private static readonly short[] Prebuffer =
            new short[SID.SampleRate * 60 / 1000];

        private readonly short[] buffer = new short[2048];
        private readonly short[] resampledBuffer = new short[2048];
        private long lastPumpTimestamp;
        private double sampleCredit;
        private IntPtr stream;

        public AudioOutput(string? preferredDevice = null)
        {
            uint deviceId = ResolveDevice(preferredDevice);
            Open(deviceId);

            if (!Available && deviceId != SDL.AudioDeviceDefaultPlayback)
                Open(SDL.AudioDeviceDefaultPlayback);
        }

        public string? Error { get; private set; }

        public bool Available => stream != IntPtr.Zero;

        public uint CurrentDeviceId { get; private set; }

        public string CurrentDeviceName { get; private set; } = SystemDefaultName;

        public static AudioDeviceInfo[] GetPlaybackDevices()
        {
            List<AudioDeviceInfo> devices = new List<AudioDeviceInfo>
            {
                new AudioDeviceInfo(SDL.AudioDeviceDefaultPlayback, SystemDefaultName)
            };

            uint[]? deviceIds = SDL.GetAudioPlaybackDevices(out _);

            if (deviceIds is null)
                return devices.ToArray();

            foreach (uint deviceId in deviceIds)
            {
                string? name = SDL.GetAudioDeviceName(deviceId);

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                devices.Add(new AudioDeviceInfo(deviceId, name));
            }

            return devices.ToArray();
        }

        /// <summary>
        /// Replaces the current stream with one opened on <paramref name="deviceId"/>.
        /// If that device fails, the default device is tried. Returns false when
        /// the requested device could not be selected.
        /// </summary>
        public bool SelectDevice(uint deviceId)
        {
            if (deviceId == CurrentDeviceId && Available)
                return true;

            if (Open(deviceId))
                return true;

            string failure = Error ?? "unknown audio error";

            if (deviceId != SDL.AudioDeviceDefaultPlayback)
                Open(SDL.AudioDeviceDefaultPlayback);

            Error = failure;
            return false;
        }

        public void Pump(SID? sid, bool muted, bool warping = false)
        {
            if (stream == IntPtr.Zero || sid is null)
                return;

            if (muted)
            {
                sid.DiscardSamples();
                lastPumpTimestamp = 0;
                sampleCredit = 0;
                return;
            }

            int queuedSamples = SDL.GetAudioStreamQueued(stream) / sizeof(short);

            // If the device has fallen behind (window drag, breakpoint, etc.),
            // do not add more latency; keep the current queue playing. Warp
            // deliberately bypasses this limit so SID audio keeps flowing while
            // the CPU is running flat out.
            if (!warping && queuedSamples > MaxQueuedSamples)
            {
                sid.DiscardSamples();
                lastPumpTimestamp = 0;
                sampleCredit = 0;
                return;
            }

            bool resampled = false;
            int count;
            int writeCount;

            if (warping)
            {
                count = sid.ReadSamples(buffer);
                writeCount = count;
                lastPumpTimestamp = 0;
                sampleCredit = 0;
            }
            else
            {
                long now = Stopwatch.GetTimestamp();
                long elapsed = lastPumpTimestamp == 0 ? 0 : now - lastPumpTimestamp;
                bool resumed = elapsed > Stopwatch.Frequency / 10;
                lastPumpTimestamp = now;

                if (resumed)
                {
                    // Emulation was paused (menu, debugger); the device kept
                    // playing while no frames were produced. Drop audio from
                    // before the stall and restore the cushion before feeding
                    // new samples.
                    sid.DiscardSamples();
                    sampleCredit = 0;

                    if (queuedSamples < Prebuffer.Length)
                        PrimeOutput();

                    elapsed = 0;
                }

                if (elapsed <= 0)
                {
                    count = sid.ReadSamples(buffer);
                    writeCount = count;
                }
                else
                {
                    // Feed the device by wall-clock time, not by emulated
                    // frames. Frame deadlines that land a little late would
                    // otherwise drain the queue and cause periodic underrun
                    // crackle.
                    sampleCredit += elapsed * (double)SID.SampleRate / Stopwatch.Frequency;
                    sampleCredit = Math.Min(sampleCredit, buffer.Length);

                    int desired = (int)sampleCredit;
                    sampleCredit -= desired;

                    count = sid.ReadSamples(buffer.AsSpan(0, desired));

                    if (
                        count > 0 &&
                        desired > count &&
                        desired <= buffer.Length &&
                        desired <= count * 5 / 4)
                    {
                        // Slightly fewer samples were produced than the
                        // device consumed. Stretch them by interpolation so
                        // the waveform stays continuous; the pitch change is
                        // a fraction of a percent.
                        Resample(buffer, count, resampledBuffer, desired);
                        writeCount = desired;
                        resampled = true;
                    }
                    else
                    {
                        writeCount = count;

                        // A large shortfall is a real stall, not drift; do
                        // not carry it forward as a catch-up burst.
                        if (desired > count)
                            sampleCredit = 0;
                    }
                }
            }

            if (writeCount == 0)
                return;

            ReadOnlySpan<short> samples = resampled
                ? resampledBuffer.AsSpan(0, writeCount)
                : buffer.AsSpan(0, writeCount);
            ReadOnlySpan<byte> data = MemoryMarshal.AsBytes(samples);

            if (!SDL.PutAudioStreamData(stream, data, data.Length))
                Error = "Unable to write audio: " + SDL.GetError();
        }

        private static void Resample(
            short[] source,
            int sourceCount,
            short[] destination,
            int destinationCount)
        {
            if (sourceCount == 1)
            {
                Array.Fill(destination, source[0], 0, destinationCount);
                return;
            }

            double step = (double)(sourceCount - 1) / (destinationCount - 1);

            for (int i = 0; i < destinationCount; i++)
            {
                double position = i * step;
                int index = (int)position;
                int next = Math.Min(index + 1, sourceCount - 1);
                double fraction = position - index;

                destination[i] = (short)Math.Round(
                    source[index] + ((source[next] - source[index]) * fraction));
            }
        }

        public void Flush()
        {
            if (stream == IntPtr.Zero)
                return;

            SDL.ClearAudioStream(stream);
            lastPumpTimestamp = 0;
            sampleCredit = 0;
            PrimeOutput();
        }

        public void Dispose()
        {
            CloseStream();
        }

        private bool Open(uint deviceId)
        {
            CloseStream();

            SDL.AudioSpec spec = new SDL.AudioSpec
            {
                Format = SDL.AudioFormat.AudioS16LE,
                Channels = 1,
                Freq = SID.SampleRate
            };

            IntPtr opened = SDL.OpenAudioDeviceStream(
                deviceId,
                in spec,
                null!,
                IntPtr.Zero);

            if (opened == IntPtr.Zero)
            {
                Error = "Unable to open audio device: " + SDL.GetError();
                CurrentDeviceId = 0;
                CurrentDeviceName = SystemDefaultName;
                return false;
            }

            if (!SDL.ResumeAudioStreamDevice(opened))
            {
                Error = "Unable to start audio device: " + SDL.GetError();
                SDL.DestroyAudioStream(opened);
                CurrentDeviceId = 0;
                CurrentDeviceName = SystemDefaultName;
                return false;
            }

            stream = opened;
            CurrentDeviceId = deviceId;
            CurrentDeviceName = deviceId == SDL.AudioDeviceDefaultPlayback
                ? SystemDefaultName
                : (SDL.GetAudioDeviceName(deviceId) ?? SystemDefaultName);
            Error = null;
            lastPumpTimestamp = 0;
            sampleCredit = 0;
            PrimeOutput();

            return true;
        }

        private void PrimeOutput()
        {
            if (stream == IntPtr.Zero || Prebuffer.Length == 0)
                return;

            ReadOnlySpan<byte> silence = MemoryMarshal.AsBytes(Prebuffer.AsSpan());

            if (!SDL.PutAudioStreamData(stream, silence, silence.Length))
                Error = "Unable to prime audio: " + SDL.GetError();
        }

        private void CloseStream()
        {
            if (stream == IntPtr.Zero)
                return;

            SDL.DestroyAudioStream(stream);
            stream = IntPtr.Zero;
        }

        private static uint ResolveDevice(string? preferredDevice)
        {
            if (string.IsNullOrWhiteSpace(preferredDevice))
                return SDL.AudioDeviceDefaultPlayback;

            foreach (AudioDeviceInfo device in GetPlaybackDevices())
            {
                if (string.Equals(device.Name, preferredDevice, StringComparison.OrdinalIgnoreCase))
                    return device.Id;
            }

            return SDL.AudioDeviceDefaultPlayback;
        }
    }
}
