using System;

namespace C64Emulator
{
    /// <summary>
    /// The C64 serial (IEC) bus. The three signal wires are open collector:
    /// every participant can pull a line low, and the line is high only when
    /// nobody is pulling it. The C64 drives ATN, CLK and DATA through CIA2
    /// port A; the drive drives CLK and DATA through VIA1 port B and can
    /// acknowledge ATN automatically by pulling DATA low (ATNA).
    /// </summary>
    internal sealed class IecBus
    {
        private bool c64AtnLow;
        private bool c64ClockLow;
        private bool c64DataLow;

        private bool driveClockLow;
        private bool driveDataLow;
        private bool driveAtna;

        private bool atnLow;
        private bool clockLow;
        private bool dataLow;

        /// <summary>
        /// Raised whenever the ATN wire changes. The argument is true when ATN
        /// is high (released), which is the edge the 1541 uses to detect that
        /// a command has finished arriving.
        /// </summary>
        public event Action<bool>? AtnChanged;

        /// <summary>
        /// Raised after any of the three wires changes state. Useful for
        /// diagnostics and for tests that decode the serial protocol.
        /// </summary>
        public event Action? LinesChanged;

        public bool AtnLow => atnLow;

        public bool ClockLow => clockLow;

        public bool DataLow => dataLow;
        

        public bool AtnHigh => !atnLow;

        public bool ClockHigh => !clockLow;

        public bool DataHigh => !dataLow;

        /// <summary>
        /// Updates the C64 side. A set output bit with the matching direction
        /// bit pulls the line low; the C64's inverting open-collector drivers
        /// make a high CIA2 pin mean a low bus line.
        /// </summary>
        public void SetC64(byte output, byte direction)
        {
            c64AtnLow = (direction & 0x08) != 0 && (output & 0x08) != 0;
            c64ClockLow = (direction & 0x10) != 0 && (output & 0x10) != 0;
            c64DataLow = (direction & 0x20) != 0 && (output & 0x20) != 0;
            Update();
        }

        /// <summary>
        /// Updates the drive side. ATNA is the automatic ATN acknowledge: while
        /// it is enabled and ATN is asserted, the drive holds DATA low.
        /// </summary>
        public void SetDrive(bool clockLow, bool dataLow, bool atna)
        {
            driveClockLow = clockLow;
            driveDataLow = dataLow;
            driveAtna = atna;
            Update();
        }

        public void Reset()
        {
            c64AtnLow = false;
            c64ClockLow = false;
            c64DataLow = false;
            driveClockLow = false;
            driveDataLow = false;
            driveAtna = false;
            atnLow = false;
            clockLow = false;
            dataLow = false;
        }

        private void Update()
        {
            bool newAtnLow = c64AtnLow;
            bool newClockLow = c64ClockLow || driveClockLow;
            bool newDataLow = c64DataLow || driveDataLow || (driveAtna && c64AtnLow);

            bool atnChanged = newAtnLow != atnLow;
            bool linesChanged =
                atnChanged ||
                newClockLow != clockLow ||
                newDataLow != dataLow;

            atnLow = newAtnLow;
            clockLow = newClockLow;
            dataLow = newDataLow;

            if (atnChanged)
                AtnChanged?.Invoke(!atnLow);

            if (linesChanged)
                LinesChanged?.Invoke();
        }
    }
}
