using System;
using System.Collections.Generic;
using System.IO;
using C64Emulator.Media;

namespace C64Emulator.Devices;

/// <summary>
/// The device side of a tape image. T64 containers and standard CBM blocks
/// decoded from TAP share the virtual KERNAL load path maintained by
/// <see cref="Core.C64Machine"/>.
/// </summary>
internal sealed class TapeDrive
{
    private const int ReadLineLowCycles = 8;

    private T64Image? t64Image;
    private TapImage? tapImage;
    private Action<bool>? readLineChanged;
    private int pulseIndex;
    private int cyclesUntilPulse;
    private int readLineLowCycles;
    private bool readLineHigh = true;
    private bool motorOn;

    public bool IsMounted => t64Image is not null || tapImage is not null;

    public string? ImagePath { get; private set; }

    /// <summary>
    /// True when the mounted image contains datasette pulses and should be
    /// serviced by the CIA FLAG path rather than the virtual KERNAL loader.
    /// </summary>
    public bool UsesPulsePlayback => tapImage is not null;

    public bool MotorOn => motorOn;

    public int PulseIndex => pulseIndex;

    public int PulseCount => tapImage?.Pulses.Count ?? 0;

    public string TapeName =>
        t64Image?.TapeName ?? tapImage?.TapeName ?? string.Empty;

    public IReadOnlyList<ITapeFileInfo> Directory
    {
        get
        {
            if (t64Image is not null)
                return t64Image.Directory;

            if (tapImage is not null)
                return tapImage.Directory;

            return Array.Empty<ITapeFileInfo>();
        }
    }

    public void Mount(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (
            Path.GetExtension(path).Equals(
                ".tap",
                StringComparison.OrdinalIgnoreCase))
        {
            Mount(TapImage.Load(path), path);
            return;
        }

        Mount(T64Image.Load(path), path);
    }

    public void Mount(T64Image t64, string? path = null)
    {
        t64Image = t64 ?? throw new ArgumentNullException(nameof(t64));
        tapImage = null;
        ImagePath = path;
        ResetPlayback();
    }

    public void Mount(TapImage tap, string? path = null)
    {
        tapImage = tap ?? throw new ArgumentNullException(nameof(tap));
        t64Image = null;
        ImagePath = path;
        ResetPlayback();
    }

    public void Eject()
    {
        t64Image = null;
        tapImage = null;
        ImagePath = null;
        ResetPlayback();
    }

    /// <summary>
    /// Connects the datasette READ line to the machine's CIA FLAG input.
    /// </summary>
    public void AttachReadLine(Action<bool> changed)
    {
        readLineChanged = changed ?? throw new ArgumentNullException(nameof(changed));
        readLineHigh = true;
        readLineChanged(true);
    }

    /// <summary>
    /// The 6510 drives the cassette motor through port bit 5, active low.
    /// </summary>
    public void SetMotor(bool on)
    {
        if (motorOn == on)
            return;

        motorOn = on;

        if (!on)
        {
            readLineLowCycles = 0;
            SetReadLine(true);
        }
    }

    public void ResetPlayback()
    {
        pulseIndex = 0;
        cyclesUntilPulse = tapImage is not null && tapImage.Pulses.Count > 0
            ? tapImage.Pulses[0]
            : 0;
        readLineLowCycles = 0;
        motorOn = false;
        SetReadLine(true);
    }

    internal void WritePlayback(BinaryWriter writer)
    {
        writer.Write(pulseIndex);
        writer.Write(cyclesUntilPulse);
        writer.Write(readLineLowCycles);
        writer.Write(readLineHigh);
        writer.Write(motorOn);
    }

    internal void ReadPlayback(BinaryReader reader)
    {
        pulseIndex = Math.Max(0, reader.ReadInt32());
        cyclesUntilPulse = Math.Max(0, reader.ReadInt32());
        readLineLowCycles = Math.Max(0, reader.ReadInt32());
        readLineHigh = reader.ReadBoolean();
        motorOn = reader.ReadBoolean();

        if (tapImage is null)
        {
            pulseIndex = 0;
            cyclesUntilPulse = 0;
            readLineLowCycles = 0;
            readLineHigh = true;
            motorOn = false;
            return;
        }

        pulseIndex = Math.Min(pulseIndex, tapImage.Pulses.Count);
    }

    /// <summary>
    /// Advances the datasette one or more CPU cycles. TAP pulse durations are
    /// measured between falling edges, so each pulse drives READ low briefly
    /// and then releases it high again.
    /// </summary>
    public void Tick(int cycles)
    {
        if (tapImage is null || !motorOn || cycles <= 0)
            return;

        while (cycles > 0)
        {
            if (pulseIndex >= tapImage.Pulses.Count)
            {
                SetReadLine(true);
                break;
            }

            // The low part of a pulse and the interval to the next falling
            // edge run in parallel. Stop at whichever event comes first.
            var step = Math.Min(cycles, cyclesUntilPulse);

            if (readLineLowCycles > 0)
                step = Math.Min(step, readLineLowCycles);

            if (step <= 0)
                step = 1;

            cycles -= step;

            if (readLineLowCycles > 0)
            {
                readLineLowCycles -= step;

                if (readLineLowCycles == 0)
                    SetReadLine(true);
            }

            cyclesUntilPulse -= step;

            if (cyclesUntilPulse <= 0)
            {
                SetReadLine(false);
                pulseIndex++;
                readLineLowCycles = ReadLineLowCycles;
                cyclesUntilPulse = pulseIndex < tapImage.Pulses.Count
                    ? tapImage.Pulses[pulseIndex]
                    : int.MaxValue;
            }
        }
    }

    private void SetReadLine(bool high)
    {
        if (readLineHigh == high)
            return;

        readLineHigh = high;
        readLineChanged?.Invoke(high);
    }

    public ITapeFileInfo? FindFile(string name)
    {
        if (t64Image is not null)
            return t64Image.FindFile(name);

        if (tapImage is not null)
            return tapImage.FindFile(name);

        return null;
    }

    public ITapeFileInfo? FirstProgram()
    {
        if (t64Image is not null)
            return t64Image.FirstProgram();

        if (tapImage is not null)
            return tapImage.FirstProgram();

        return null;
    }

    public byte[] ReadFile(ITapeFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file is T64FileInfo t64)
        {
            T64Image? mounted = t64Image;

            if (mounted is null)
                throw new InvalidOperationException("No tape is mounted.");

            return mounted.ReadFile(t64);
        }

        if (file is TapFileInfo tap)
        {
            TapImage? mounted = tapImage;

            if (mounted is null)
                throw new InvalidOperationException("No tape is mounted.");

            return mounted.ReadFile(tap);
        }

        throw new ArgumentException("Unknown tape file entry.", nameof(file));
    }

    public byte[] CreatePrg(ITapeFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file is T64FileInfo t64)
        {
            T64Image? mounted = t64Image;

            if (mounted is null)
                throw new InvalidOperationException("No tape is mounted.");

            return mounted.ToPrg(t64);
        }

        if (file is TapFileInfo tap)
        {
            TapImage? mounted = tapImage;

            if (mounted is null)
                throw new InvalidOperationException("No tape is mounted.");

            return mounted.ToPrg(tap);
        }

        throw new ArgumentException("Unknown tape file entry.", nameof(file));
    }
}
