using System;
using System.Collections.Generic;
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
            new short[SID.SampleRate * 40 / 1000];

        private readonly short[] buffer = new short[2048];
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
                return;
            }

            if (!warping)
            {
                int queuedSamples = SDL.GetAudioStreamQueued(stream) / sizeof(short);

                // If the device has fallen behind (window drag, breakpoint,
                // etc.), do not add more latency; keep the current queue
                // playing. Warp deliberately bypasses this limit so SID audio
                // keeps flowing while the CPU is running flat out.
                if (queuedSamples > MaxQueuedSamples)
                {
                    sid.DiscardSamples();
                    return;
                }
            }

            int count = sid.ReadSamples(buffer);

            if (count == 0)
                return;

            ReadOnlySpan<byte> data = MemoryMarshal.AsBytes(buffer.AsSpan(0, count));

            if (!SDL.PutAudioStreamData(stream, data, data.Length))
                Error = "Unable to write audio: " + SDL.GetError();
        }

        public void Flush()
        {
            if (stream == IntPtr.Zero)
                return;

            SDL.ClearAudioStream(stream);
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
