using System;

namespace C64Emulator
{
    /// <summary>
    /// A 6522 versatile interface adapter, as used by the 1541 for the IEC
    /// port (VIA1) and the disk controller (VIA2). It implements the two
    /// interval timers, the interrupt flags/mask, and the two bidirectional
    /// ports with external input callbacks. The shift register is not used by
    /// the 1541 DOS and is kept as a plain latch.
    /// </summary>
    internal sealed class Via6522
    {
        private readonly byte[] registers = new byte[16];
        private readonly Action<bool> setIrq;

        private int timer1Counter;
        private int timer1Latch;
        private bool timer1Running;

        private int timer2Counter;
        private int timer2Latch;
        private bool timer2Running;

        private bool ca1;
        private bool cb1;

        public Via6522(Action<bool> setIrq)
        {
            this.setIrq = setIrq ?? throw new ArgumentNullException(nameof(setIrq));
        }

        public Func<byte>? PortAInput { get; set; }

        public Func<byte>? PortBInput { get; set; }

        /// <summary>Raised after the port A output latch or direction changes.</summary>
        public Action? PortAChanged { get; set; }

        /// <summary>Raised after the port B output latch or direction changes.</summary>
        public Action? PortBChanged { get; set; }

        /// <summary>Raised when the CPU writes the port A output latch.</summary>
        public Action<byte>? PortAWritten { get; set; }

        /// <summary>Raised when the CPU writes the port B output latch.</summary>
        public Action<byte>? PortBWritten { get; set; }

        public Action<byte>? AcrChanged { get; set; }

        /// <summary>Raised when the CPU reads port A's external pins.</summary>
        public event Action<byte>? PortARead;

        public byte PortAOutput => registers[0x01];

        public byte PortBOutput => registers[0x00];

        public byte DdrA => registers[0x03];

        public byte DdrB => registers[0x02];

        public byte Acr => registers[0x0B];

        public byte Pcr => registers[0x0C];

        public byte Ier => registers[0x0E];

        public bool IrqAsserted => (registers[0x0D] & registers[0x0E] & 0x7F) != 0;

        public long PortBReadsLow { get; private set; }

        public void Reset()
        {
            Array.Clear(registers);
            timer1Counter = 0;
            timer1Latch = 0;
            timer1Running = false;
            timer2Counter = 0;
            timer2Latch = 0;
            timer2Running = false;
            ca1 = false;
            cb1 = false;
            setIrq(false);
        }

        public byte Read(int offset)
        {
            offset &= 0x0F;

            switch (offset)
            {
                case 0x00:
                    ClearInterrupt(0x18); // CB1/CB2
                    return ReadPortB();

                case 0x01:
                    ClearInterrupt(0x03); // CA1/CA2
                    return ReadPortA();

                case 0x04:
                {
                    byte value = (byte)(timer1Counter & 0xFF);
                    ClearInterrupt(0x40);
                    return value;
                }

                case 0x05:
                    return (byte)((timer1Counter >> 8) & 0xFF);

                case 0x06:
                    return (byte)(timer1Latch & 0xFF);

                case 0x07:
                    return (byte)((timer1Latch >> 8) & 0xFF);

                case 0x08:
                {
                    byte value = (byte)(timer2Counter & 0xFF);
                    ClearInterrupt(0x20);
                    return value;
                }

                case 0x09:
                    return (byte)((timer2Counter >> 8) & 0xFF);

                case 0x0D:
                {
                    byte value = (byte)(registers[0x0D] & 0x7F);

                    if (IrqAsserted)
                        value |= 0x80;

                    ClearInterrupt(0x7F);
                    return value;
                }

                case 0x0E:
                    return (byte)(registers[0x0E] | 0x80);

                default:
                    return registers[offset];
            }
        }

        public void Write(int offset, byte value)
        {
            offset &= 0x0F;

            switch (offset)
            {
                case 0x00:
                    registers[0x00] = value;
                    PortBWritten?.Invoke(value);
                    PortBChanged?.Invoke();
                    break;

                case 0x01:
                    registers[0x01] = value;
                    PortAWritten?.Invoke(value);
                    PortAChanged?.Invoke();
                    break;

                case 0x02:
                    registers[0x02] = value;
                    PortBChanged?.Invoke();
                    break;

                case 0x03:
                    registers[0x03] = value;
                    PortAChanged?.Invoke();
                    break;

                case 0x04:
                    timer1Latch = (timer1Latch & 0xFF00) | value;
                    break;

                case 0x05:
                    timer1Latch = (timer1Latch & 0x00FF) | (value << 8);
                    timer1Counter = timer1Latch;
                    timer1Running = true;
                    ClearInterrupt(0x40);
                    break;

                case 0x06:
                    timer1Latch = (timer1Latch & 0xFF00) | value;
                    break;

                case 0x07:
                    timer1Latch = (timer1Latch & 0x00FF) | (value << 8);
                    break;

                case 0x08:
                    timer2Latch = (timer2Latch & 0xFF00) | value;
                    break;

                case 0x09:
                    timer2Latch = (timer2Latch & 0x00FF) | (value << 8);
                    timer2Counter = timer2Latch;
                    timer2Running = true;
                    ClearInterrupt(0x20);
                    break;

                case 0x0A:
                    registers[0x0A] = value;
                    break;

                case 0x0B:
                    registers[0x0B] = value;
                    AcrChanged?.Invoke(value);
                    break;

                case 0x0C:
                    registers[0x0C] = value;
                    break;

                case 0x0D:
                    if ((value & 0x80) != 0)
                        ClearInterrupt((byte)(value & 0x7F));
                    break;

                case 0x0E:
                    if ((value & 0x80) != 0)
                        registers[0x0E] |= (byte)(value & 0x7F);
                    else
                        registers[0x0E] &= (byte)~(value & 0x7F);

                    UpdateIrq();
                    break;
            }
        }

        public void Tick(int cycles)
        {
            if (cycles <= 0)
                return;

            if (timer1Running)
                TickTimer1(cycles);

            if (timer2Running)
                TickTimer2(cycles);
        }

        /// <summary>
        /// Drives the CA1 input. The edge selected by PCR bit 0 sets the CA1
        /// interrupt flag. VIA1 uses this for the ATN line.
        /// </summary>
        public void SetCa1(bool high)
        {
            bool positiveEdge = (registers[0x0C] & 0x01) != 0;
            bool edge = positiveEdge ? high && !ca1 : !high && ca1;

            ca1 = high;

            if (edge)
                SetInterrupt(0x02);
        }

        public void SetCb1(bool high)
        {
            bool positiveEdge = (registers[0x0C] & 0x10) != 0;
            bool edge = positiveEdge ? high && !cb1 : !high && cb1;

            cb1 = high;

            if (edge)
                SetInterrupt(0x10);
        }

        /// <summary>
        /// Sets an interrupt flag regardless of the configured edge. The 1541
        /// documents ATN going high as the CA1 event, and this keeps that
        /// behaviour even if the DOS leaves PCR at its reset value.
        /// </summary>
        public void ForceInterrupt(byte bits)
        {
            SetInterrupt(bits);
        }

        private void TickTimer1(int cycles)
        {
            for (int i = 0; i < cycles; i++)
            {
                if (!timer1Running)
                    return;

                if (timer1Counter == 0)
                {
                    SetInterrupt(0x40);

                    if ((registers[0x0B] & 0x40) != 0)
                    {
                        timer1Counter = timer1Latch;
                    }
                    else
                    {
                        timer1Running = false;
                    }

                    if ((registers[0x0B] & 0x80) != 0)
                        TogglePb7();

                    if (!timer1Running)
                        return;
                }
                else
                {
                    timer1Counter--;
                }
            }
        }

        private void TickTimer2(int cycles)
        {
            // The 1541 does not feed PB6 pulses into T2, so only the phi2
            // interval-timer mode is needed here.
            for (int i = 0; i < cycles; i++)
            {
                if (!timer2Running)
                    return;

                if (timer2Counter == 0)
                {
                    SetInterrupt(0x20);
                    timer2Running = false;
                    timer2Counter = timer2Latch;
                    return;
                }

                timer2Counter--;
            }
        }

        private void TogglePb7()
        {
            if ((registers[0x03] & 0x80) == 0)
                return;

            registers[0x00] ^= 0x80;
            PortBChanged?.Invoke();
        }

        private byte ReadPortA()
        {
            byte input = PortAInput?.Invoke() ?? 0xFF;
            byte value = (byte)(
                (registers[0x01] & registers[0x03]) |
                (input & ~registers[0x03]));
            PortARead?.Invoke(value);
            return value;
        }

        private byte ReadPortB()
        {
            byte input = PortBInput?.Invoke() ?? 0xFF;
            byte value = (byte)(
                (registers[0x00] & registers[0x02]) |
                (input & ~registers[0x02]));

            if ((value & 0x80) == 0)
                PortBReadsLow++;

            return value;
        }

        private void SetInterrupt(byte bits)
        {
            registers[0x0D] |= bits;
            UpdateIrq();
        }

        private void ClearInterrupt(byte bits)
        {
            registers[0x0D] &= (byte)~bits;
            UpdateIrq();
        }

        private void UpdateIrq()
        {
            bool asserted = (registers[0x0D] & registers[0x0E] & 0x7F) != 0;

            if (asserted)
                registers[0x0D] |= 0x80;
            else
                registers[0x0D] &= 0x7F;

            setIrq(asserted);
        }
    }
}
