namespace C64Emulator
{
    /// <summary>
    /// Matches CBM filenames using the '*' and '?' patterns accepted by LOAD.
    /// </summary>
    internal static class Wildcard
    {
        public static bool Matches(string text, string pattern)
        {
            int textIndex = 0;
            int patternIndex = 0;
            int starIndex = -1;
            int matchIndex = 0;

            while (textIndex < text.Length)
            {
                if (
                    patternIndex < pattern.Length &&
                    (pattern[patternIndex] == '?' ||
                     pattern[patternIndex] == text[textIndex]))
                {
                    textIndex++;
                    patternIndex++;
                }
                else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    starIndex = patternIndex++;
                    matchIndex = textIndex;
                }
                else if (starIndex != -1)
                {
                    patternIndex = starIndex + 1;
                    textIndex = ++matchIndex;
                }
                else
                {
                    return false;
                }
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                patternIndex++;

            return patternIndex == pattern.Length;
        }
    }
}
