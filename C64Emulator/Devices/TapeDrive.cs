using System;
using System.Collections.Generic;
using C64Emulator.Core;
using C64Emulator.Media;

namespace C64Emulator.Devices;

/// <summary>
/// The device side of a T64 tape. The KERNAL load path is serviced by
/// <see cref="C64Machine"/>; this type keeps the mounted image and its
/// program lookup independent of the host menu.
/// </summary>
internal sealed class TapeDrive
{
    private T64Image? image;

    public bool IsMounted => image is not null;

    public string? ImagePath { get; private set; }

    public string TapeName => image?.TapeName ?? string.Empty;

    public IReadOnlyList<T64FileInfo> Directory =>
        image?.Directory ?? Array.Empty<T64FileInfo>();

    public void Mount(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        T64Image loaded = T64Image.Load(path);
        image = loaded;
        ImagePath = path;
    }

    public void Mount(T64Image t64, string? path = null)
    {
        image = t64 ?? throw new ArgumentNullException(nameof(t64));
        ImagePath = path;
    }

    public void Eject()
    {
        image = null;
        ImagePath = null;
    }

    public T64FileInfo? FindFile(string name)
    {
        return image?.FindFile(name);
    }

    public T64FileInfo? FirstProgram()
    {
        return image?.FirstProgram();
    }

    public byte[] ReadFile(T64FileInfo file)
    {
        T64Image? mounted = image;

        if (mounted is null)
            throw new InvalidOperationException("No tape is mounted.");

        return mounted.ReadFile(file);
    }

    public byte[] CreatePrg(T64FileInfo file)
    {
        T64Image? mounted = image;

        if (mounted is null)
            throw new InvalidOperationException("No tape is mounted.");

        return mounted.ToPrg(file);
    }
}
