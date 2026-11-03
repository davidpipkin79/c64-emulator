using System;

namespace C64Emulator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            new EmuWin().Start();
        }
    }
}
