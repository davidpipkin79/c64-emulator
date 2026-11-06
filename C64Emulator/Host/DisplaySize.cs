using C64Emulator.Video;

namespace C64Emulator.Host;

internal readonly record struct DisplaySize(int Width, int Height, int RefreshRate)
{
    public bool IsFiftyHertz => RefreshRate is >= 49 and <= 51;

    public bool IsSixtyHertz => RefreshRate is >= 59 and <= 61;

    /// <summary>
    /// Whether this mode matches the frame rate of the active video
    /// standard; the display menu lists those first.
    /// </summary>
    public bool IsPreferred(VideoStandard standard)
    {
        return standard == VideoStandard.Ntsc ? IsSixtyHertz : IsFiftyHertz;
    }
}
