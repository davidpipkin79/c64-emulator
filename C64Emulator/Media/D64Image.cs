using System;
using System.Collections.Generic;
using System.IO;
using C64Emulator.Core;

namespace C64Emulator.Media;

internal sealed class D64FileInfo(
    string name,
    CbmFileType type,
    int blocks,
    int startTrack,
    int startSector,
    bool closed,
    bool locked)
{
    public string Name { get; } = name;

    public CbmFileType Type { get; } = type;

    public int Blocks { get; } = blocks;

    public int StartTrack { get; } = startTrack;

    public int StartSector { get; } = startSector;

    public bool Closed { get; } = closed;

    public bool Locked { get; } = locked;
}

/// <summary>
/// A sector-level D64 image. It understands the standard 1541 directory
/// chain and file-sector chains; it intentionally does not model GCR,
/// protection tracks or a physical drive.
/// </summary>
internal sealed class D64Image
{
    public const int SectorSize = 256;
    private const int StandardTracks = 35;
    private const int ExtendedTracks = 40;

    private readonly byte[] data;
    private readonly int[] trackOffsets;

    private D64Image(byte[] data, int trackCount)
    {
        if (trackCount != StandardTracks && trackCount != ExtendedTracks)
            throw new ArgumentOutOfRangeException(nameof(trackCount));

        this.data = data;
        TrackCount = trackCount;
        trackOffsets = new int[trackCount + 1];

        var offset = 0;

        for (var track = 1; track <= trackCount; track++)
        {
            trackOffsets[track] = offset;
            offset += SectorsPerTrack(track) * SectorSize;
        }

        SectorCount = offset / SectorSize;

        D64FileInfo[] directory = ReadDirectory();
        Directory = directory;

        ReadOnlySpan<byte> bam = GetSector(18, 0);
        DiskName = ReadPetscii(bam.Slice(0x90, 16)).Trim();
        DiskIdBytes = bam.Slice(0xA2, 2).ToArray();
        DiskId = ReadPetscii(DiskIdBytes).Trim();
    }

    public int TrackCount { get; }

    public int SectorCount { get; }

    public string DiskName { get; }

    public string DiskId { get; }

    /// <summary>
    /// The two raw ID bytes from the BAM. These are what the 1541 encodes
    /// into GCR headers; the display string is not a safe substitute
    /// because an empty ID decodes as "??".
    /// </summary>
    public byte[] DiskIdBytes { get; }

    public IReadOnlyList<D64FileInfo> Directory { get; }

    public static D64Image Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var bytes = File.ReadAllBytes(path);
        int tracks;

        if (bytes.Length >= 196608)
            tracks = ExtendedTracks;
        else if (bytes.Length >= 174848)
            tracks = StandardTracks;
        else
            throw new InvalidDataException(
                "The file is too small to be a D64 image.");

        var required = SectorCountForTracks(tracks) * SectorSize;

        if (bytes.Length < required)
            throw new InvalidDataException(
                "The D64 image is truncated; expected at least " +
                required +
                " bytes but found " +
                bytes.Length +
                ".");

        return new D64Image(bytes, tracks);
    }

    public static D64Image FromBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var tracks = bytes.Length >= 196608
            ? ExtendedTracks
            : bytes.Length >= 174848
                ? StandardTracks
                : 0;

        if (tracks == 0)
            throw new InvalidDataException(
                "The file is too small to be a D64 image.");

        var required = SectorCountForTracks(tracks) * SectorSize;

        if (bytes.Length < required)
            throw new InvalidDataException(
                "The D64 image is truncated; expected at least " +
                required +
                " bytes but found " +
                bytes.Length +
                ".");

        return new D64Image(bytes, tracks);
    }

    public bool IsValidSector(int track, int sector)
    {
        return track >= 1 &&
            track <= TrackCount &&
            sector >= 0 &&
            sector < SectorsPerTrack(track);
    }

    public ReadOnlySpan<byte> GetSector(int track, int sector)
    {
        if (!IsValidSector(track, sector))
            throw new ArgumentOutOfRangeException(
                nameof(sector),
                "Invalid D64 track/sector " + track + "/" + sector + ".");

        var offset = trackOffsets[track] + (sector * SectorSize);
        return data.AsSpan(offset, SectorSize);
    }

    public bool IsDirty { get; private set; }

    public void SetSector(int track, int sector, ReadOnlySpan<byte> source)
    {
        if (source.Length != SectorSize)
            throw new ArgumentException(
                "A D64 sector must be exactly 256 bytes.",
                nameof(source));

        if (!IsValidSector(track, sector))
            throw new ArgumentOutOfRangeException(
                nameof(sector),
                "Invalid D64 track/sector " + track + "/" + sector + ".");

        var offset = trackOffsets[track] + (sector * SectorSize);
        source.CopyTo(data.AsSpan(offset, SectorSize));
        IsDirty = true;
    }

    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        File.WriteAllBytes(path, data);
        IsDirty = false;
    }

    public byte[] ToArray()
    {
        return (byte[])data.Clone();
    }

    public byte[] ReadFile(D64FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!file.Closed)
            throw new InvalidDataException(
                "The file is not closed and may have been left open by a game.");

        if (!IsValidSector(file.StartTrack, file.StartSector))
            throw new InvalidDataException("The file has an invalid starting sector.");

        List<byte> bytes = [];
        var track = file.StartTrack;
        var sector = file.StartSector;
        HashSet<int> visited = new HashSet<int>();

        while (true)
        {
            if (!IsValidSector(track, sector))
                throw new InvalidDataException("The file chain points outside the image.");

            var key = (track * 256) + sector;

            if (!visited.Add(key))
                throw new InvalidDataException("The file chain contains a loop.");

            ReadOnlySpan<byte> block = GetSector(track, sector);
            int nextTrack = block[0];
            int nextSector = block[1];
            // The last sector stores its used byte count in the sector
            // link byte. The count includes the track link byte, so the
            // payload is one byte shorter; zero means a full sector.
            var used = nextTrack == 0
                ? nextSector == 0
                    ? SectorSize - 2
                    : Math.Clamp(nextSector - 1, 0, SectorSize - 2)
                : SectorSize - 2;

            for (var i = 0; i < used; i++)
                bytes.Add(block[2 + i]);

            if (nextTrack == 0)
                break;

            track = nextTrack;
            sector = nextSector;
        }

        return [.. bytes];
    }

    public D64FileInfo? FindFile(string name)
    {
        var pattern = (name ?? string.Empty).Trim();

        if (pattern.Length == 0 || pattern == "*")
            return FirstProgram();

        if (pattern == "$")
            return null;

        var normalized = pattern.ToUpperInvariant();

        foreach (D64FileInfo file in Directory)
        {
            if (WildcardMatcher.Matches(file.Name.ToUpperInvariant(), normalized))
                return file;
        }

        return null;
    }

    public D64FileInfo? FirstProgram()
    {
        foreach (D64FileInfo file in Directory)
        {
            if (file.Type == CbmFileType.Program)
                return file;
        }

        return null;
    }

    public static int SectorsPerTrack(int track)
    {
        if (track is >= 1 and <= 17)
            return 21;

        if (track is >= 18 and <= 24)
            return 19;

        if (track is >= 25 and <= 30)
            return 18;

        if (track is >= 31 and <= 40)
            return 17;

        throw new ArgumentOutOfRangeException(nameof(track));
    }

    private static int SectorCountForTracks(int tracks)
    {
        var count = 0;

        for (var track = 1; track <= tracks; track++)
            count += SectorsPerTrack(track);

        return count;
    }

    private D64FileInfo[] ReadDirectory()
    {
        List<D64FileInfo> files = [];
        HashSet<int> visited = new HashSet<int>();
        var track = 18;
        var sector = 1;

        while (track != 0 && IsValidSector(track, sector))
        {
            var key = (track * 256) + sector;

            if (!visited.Add(key))
                break;

            ReadOnlySpan<byte> block = GetSector(track, sector);

            for (var entry = 0; entry < 8; entry++)
            {
                var offset = entry * 32;
                var typeByte = block[offset + 2];
                CbmFileType type = (CbmFileType)(typeByte & 0x0F);

                if (type is < CbmFileType.Sequential or > CbmFileType.Relative)
                    continue;

                int startTrack = block[offset + 3];
                int startSector = block[offset + 4];

                if (!IsValidSector(startTrack, startSector))
                    continue;

                var name = ReadPetscii(block.Slice(offset + 5, 16)).Trim();
                var blocks = block[offset + 30] | (block[offset + 31] << 8);

                files.Add(
                    new D64FileInfo(
                        name,
                        type,
                        blocks,
                        startTrack,
                        startSector,
                        (typeByte & 0x80) != 0,
                        (typeByte & 0x40) != 0));
            }

            int nextTrack = block[0];
            int nextSector = block[1];

            if (nextTrack == 0)
                break;

            track = nextTrack;
            sector = nextSector;
        }

        return [.. files];
    }

    private static string ReadPetscii(ReadOnlySpan<byte> bytes)
    {
        return Petscii.ToAscii(bytes);
    }

}
