namespace C64Emulator.Host;

internal readonly record struct DisplaySize(int Width, int Height, int RefreshRate)
{
    public bool IsFiftyHertz => RefreshRate is >= 49 and <= 51;
}
