using System;
using System.Collections.Generic;
using System.IO;
using C64Emulator.Core;
using C64Emulator.Media;

namespace C64Emulator.Devices;

/// <summary>
/// The disk-device side of a D64 image. When a 1541 DOS ROM is available
/// the image is also attached to a real <see cref="Drive1541"/> that
/// speaks the IEC protocol; otherwise the KERNAL-level virtual drive in
/// <see cref="C64Machine"/> remains available as a fallback.
/// </summary>
internal sealed class DiskDrive
{
    private D64Image? image;

    public DiskDrive(IecBus? iec = null, byte[]? rom = null)
    {
        if (iec is not null && rom is not null)
        {
            try
            {
                RealDrive = new Drive1541(rom, iec);
            }
            catch (Exception ex)
            {
                RealDriveError = ex.Message;
            }
        }
    }

    public bool IsMounted => image is not null;

    public string? ImagePath { get; private set; }

    public string DiskName => image?.DiskName ?? string.Empty;

    public string DiskId => image?.DiskId ?? string.Empty;

    public IReadOnlyList<D64FileInfo> Directory =>
        image?.Directory ?? Array.Empty<D64FileInfo>();

    /// <summary>
    /// The hardware 1541, or null when no DOS ROM was supplied.
    /// </summary>
    public Drive1541? RealDrive { get; }

    public bool RealDriveActive => RealDrive is not null;

    /// <summary>
    /// When true the KERNAL talks to the hardware 1541 over the IEC bus.
    /// When false the virtual KERNAL disk path is used and the hardware
    /// drive stays idle. The virtual path is the default because it loads
    /// D64 programs instantly.
    /// </summary>
    public bool TrueDriveEmulation { get; set; }

    public bool HardwareDriveActive => TrueDriveEmulation && RealDrive is not null;

    public string? RealDriveError { get; }

    public void Mount(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        D64Image loaded = D64Image.Load(path);
        Mount(loaded, path);
    }

    public void Mount(D64Image d64, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(d64);

        RealDrive?.Flush();
        image = d64;
        ImagePath = path;
        RealDrive?.Mount(d64, path);
    }

    public void Eject()
    {
        RealDrive?.Eject();
        image = null;
        ImagePath = null;
    }

    public void Reset()
    {
        RealDrive?.Reset();
    }

    public void Tick(int cycles)
    {
        RealDrive?.Tick(cycles);
    }

    public void Flush()
    {
        RealDrive?.Flush();
    }

    public D64FileInfo? FindFile(string name)
    {
        return image?.FindFile(name);
    }

    public D64FileInfo? FirstProgram()
    {
        return image?.FirstProgram();
    }

    public byte[] ReadFile(D64FileInfo file)
    {
        D64Image? mounted = image;

        if (mounted is null)
            throw new InvalidOperationException("No disk is mounted.");

        return mounted.ReadFile(file);
    }

    public static byte[]? TryLoadRom(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        string[] names =
        [
            "dos1541.rom",
            "dos1541.bin",
            "1541.rom"
        ];

        foreach (var name in names)
        {
            var path = Path.Combine(directory, name);

            if (File.Exists(path))
                return File.ReadAllBytes(path);
        }

        var romDirectory = Path.Combine(directory, "Roms");

        foreach (var name in names)
        {
            var path = Path.Combine(romDirectory, name);

            if (File.Exists(path))
                return File.ReadAllBytes(path);
        }

        return null;
    }

    public static bool TryGetLoadAddress(byte[] prg, out ushort address)
    {
        if (prg is null || prg.Length < 2)
        {
            address = 0;
            return false;
        }

        address = (ushort)(prg[0] | (prg[1] << 8));
        return true;
    }
}
