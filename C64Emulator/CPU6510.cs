using System;

namespace C64Emulator
{
    /// <summary>
    /// A 6502/6510 core. The core is deliberately synchronous: it executes one
    /// instruction per <see cref="Step"/> call and leaves timing, pacing and
    /// device ticking to <see cref="C64Machine"/>.
    /// </summary>
    internal sealed class CPU6510
    {
        public enum StatusFlags : byte
        {
            C = 0x01, // carry flag
            Z = 0x02, // zero flag
            I = 0x04, // interrupt disable flag
            D = 0x08, // decimal mode flag
            B = 0x10, // break flag (only in pushed status)
            X = 0x20, // unused flag (reads as 1)
            V = 0x40, // overflow flag
            N = 0x80  // negative flag
        }

        private enum Instructions : byte
        {
            ORA_imm = 0x09,
            ORA_zp = 0x05,
            ORA_zpx = 0x15,
            ORA_izx = 0x01,
            ORA_izy = 0x11,
            ORA_abs = 0x0D,
            ORA_abx = 0x1D,
            ORA_aby = 0x19,
            AND_imm = 0x29,
            AND_zp = 0x25,
            AND_zpx = 0x35,
            AND_izx = 0x21,
            AND_izy = 0x31,
            AND_abs = 0x2D,
            AND_abx = 0x3D,
            AND_aby = 0x39,
            EOR_imm = 0x49,
            EOR_zp = 0x45,
            EOR_zpx = 0x55,
            EOR_izx = 0x41,
            EOR_izy = 0x51,
            EOR_abs = 0x4D,
            EOR_abx = 0x5D,
            EOR_aby = 0x59,
            ADC_imm = 0x69,
            ADC_zp = 0x65,
            ADC_zpx = 0x75,
            ADC_izx = 0x61,
            ADC_izy = 0x71,
            ADC_abs = 0x6D,
            ADC_abx = 0x7D,
            ADC_aby = 0x79,
            SBC_imm = 0xE9,
            SBC_zp = 0xE5,
            SBC_zpx = 0xF5,
            SBC_izx = 0xE1,
            SBC_izy = 0xF1,
            SBC_abs = 0xED,
            SBC_abx = 0xFD,
            SBC_aby = 0xF9,
            CMP_imm = 0xC9,
            CMP_zp = 0xC5,
            CMP_zpx = 0xD5,
            CMP_izx = 0xC1,
            CMP_izy = 0xD1,
            CMP_abs = 0xCD,
            CMP_abx = 0xDD,
            CMP_aby = 0xD9,
            CPX_imm = 0xE0,
            CPX_zp = 0xE4,
            CPX_abs = 0xEC,
            CPY_imm = 0xC0,
            CPY_zp = 0xC4,
            CPY_abs = 0xCC,
            DEC_zp = 0xC6,
            DEC_zpx = 0xD6,
            DEC_abs = 0xCE,
            DEC_abx = 0xDE,
            DEX = 0xCA,
            DEY = 0x88,
            INC_zp = 0xE6,
            INC_zpx = 0xF6,
            INC_abs = 0xEE,
            INC_abx = 0xFE,
            INX = 0xE8,
            INY = 0xC8,
            ASL_imp = 0x0A,
            ASL_zp = 0x06,
            ASL_zpx = 0x16,
            ASL_abs = 0x0E,
            ASL_abx = 0x1E,
            ROL_imp = 0x2A,
            ROL_zp = 0x26,
            ROL_zpx = 0x36,
            ROL_abs = 0x2E,
            ROL_abx = 0x3E,
            LSR_imp = 0x4A,
            LSR_zp = 0x46,
            LSR_zpx = 0x56,
            LSR_abs = 0x4E,
            LSR_abx = 0x5E,
            ROR_imp = 0x6A,
            ROR_zp = 0x66,
            ROR_zpx = 0x76,
            ROR_abs = 0x6E,
            ROR_abx = 0x7E,
            LDA_imm = 0xA9,
            LDA_zp = 0xA5,
            LDA_zpx = 0xB5,
            LDA_izx = 0xA1,
            LDA_izy = 0xB1,
            LDA_abs = 0xAD,
            LDA_abx = 0xBD,
            LDA_aby = 0xB9,
            STA_zp = 0x85,
            STA_zpx = 0x95,
            STA_izx = 0x81,
            STA_izy = 0x91,
            STA_abs = 0x8D,
            STA_abx = 0x9D,
            STA_aby = 0x99,
            LDX_imm = 0xA2,
            LDX_zp = 0xA6,
            LDX_zpy = 0xB6,
            LDX_abs = 0xAE,
            LDX_aby = 0xBE,
            STX_zp = 0x86,
            STX_zpy = 0x96,
            STX_abs = 0x8E,
            LDY_imm = 0xA0,
            LDY_zp = 0xA4,
            LDY_zpx = 0xB4,
            LDY_abs = 0xAC,
            LDY_abx = 0xBC,
            STY_zp = 0x84,
            STY_zpx = 0x94,
            STY_abs = 0x8C,
            TAX = 0xAA,
            TXA = 0x8A,
            TAY = 0xA8,
            TYA = 0x98,
            TSX = 0xBA,
            TXS = 0x9A,
            PLA = 0x68,
            PHA = 0x48,
            PLP = 0x28,
            PHP = 0x08,
            BPL = 0x10,
            BMI = 0x30,
            BVC = 0x50,
            BVS = 0x70,
            BCC = 0x90,
            BCS = 0xB0,
            BNE = 0xD0,
            BEQ = 0xF0,
            BRK = 0x00,
            RTI = 0x40,
            JSR = 0x20,
            RTS = 0x60,
            JMP_abs = 0x4C,
            JMP_ind = 0x6C,
            BIT_zp = 0x24,
            BIT_abs = 0x2C,
            CLC = 0x18,
            SEC = 0x38,
            CLD = 0xD8,
            SED = 0xF8,
            CLI = 0x58,
            SEI = 0x78,
            CLV = 0xB8,
            NOP = 0xEA,

            // Illegal Opcodes
            
            ANC01 = 0x0B,
            ANC02 = 0x2B,
            ANE = 0x8B,
            ARR = 0x6B,
            ASR = 0x4B,
            DCP_zp = 0xC7,
            DCP_zpx = 0xD7,
            DCP_abs = 0xCF,
            DCP_abx = 0xDF,
            DCP_aby = 0xDB,
            DCP_izx = 0xC3,
            DCP_izy = 0xD3,
            ISB_zp = 0xE7,
            ISB_zpx = 0xF7,
            ISB_abs = 0xEF,
            ISB_abx = 0xFF,
            ISB_aby = 0xFB,
            ISB_izx = 0xE3,
            ISB_izy = 0xF3,
            JAM01 = 0x02,
            JAM02 = 0x12,
            JAM03 = 0x22,
            JAM04 = 0x32,
            JAM05 = 0x42,
            JAM06 = 0x52,
            JAM07 = 0x62,
            JAM08 = 0x72,
            JAM09 = 0x92,
            JAM10 = 0xB2,
            JAM11 = 0xD2,
            JAM12 = 0xF2,
            LAE = 0xBB,
            LAX_zp = 0xA7,
            LAX_zpy = 0xB7,
            LAX_abs = 0xAF,
            LAX_aby = 0xBF,
            LAX_izx = 0xA3,
            LAX_izy = 0xB3,
            LXA = 0xAB,
            NOP01 = 0x1A,
            NOP02 = 0x3A,
            NOP03 = 0x5A,
            NOP04 = 0x7A,
            NOP05 = 0xDA,
            NOP06 = 0xFA,
            NOP_imm01 = 0x80,
            NOP_imm02 = 0x82,
            NOP_imm03 = 0x89,
            NOP_imm04 = 0xC2,
            NOP_imm05 = 0xE2,
            NOP_zp01 = 0x04,
            NOP_zp02 = 0x44,
            NOP_zp03 = 0x64,
            NOP_zpx01 = 0x14,
            NOP_zpx02 = 0x34,
            NOP_zpx03 = 0x54,
            NOP_zpx04 = 0x74,
            NOP_zpx05 = 0xD4,
            NOP_zpx06 = 0xF4,
            NOP_abs = 0x0C,
            NOP_abx01 = 0x1C,
            NOP_abx02 = 0x3C,
            NOP_abx03 = 0x5C,
            NOP_abx04 = 0x7C,
            NOP_abx05 = 0xDC,
            NOP_abx06 = 0xFC,
            RLA_zp = 0x27,
            RLA_zpx = 0x37,
            RLA_abs = 0x2F,
            RLA_abx = 0x3F,
            RLA_aby = 0x3B,
            RLA_izx = 0x23,
            RLA_izy = 0x33,
            RRA_zp = 0x67,
            RRA_zpx = 0x77,
            RRA_abs = 0x6F,
            RRA_abx = 0x7F,
            RRA_aby = 0x7B,
            RRA_izx = 0x63,
            RRA_izy = 0x73,
            SAX_zp = 0x87,
            SAX_zpy = 0x97,
            SAX_abs = 0x8F,
            SAX_izx = 0x83,
            SBC = 0xEB,
            SBX = 0xCB,
            SHA_abx = 0x93,
            SHA_aby = 0x9F,
            SHS = 0x9B,
            SHX = 0x9E,
            SHY = 0x9C,
            SLO_zp = 0x07,
            SLO_zpx = 0x17,
            SLO_abs = 0x0F,
            SLO_abx = 0x1F,
            SLO_aby = 0x1B,
            SLO_izx = 0x03,
            SLO_izy = 0x13,
            SRE_zp = 0x47,
            SRE_zpx = 0x57,
            SRE_abs = 0x4F,
            SRE_abx = 0x5F,
            SRE_aby = 0x5B,
            SRE_izx = 0x43,
            SRE_izy = 0x53
        }

        public struct Registers
        {
            public ushort PC;   // program counter
            public byte S;      // stack pointer
            public byte P;      // processor status
            public byte A;      // accumulator
            public byte X;      // index register
            public byte Y;      // index register
        }

        private readonly IMemoryBus bus;
        private readonly InterruptLine interrupts;
        private Registers registers;
        private int cycles;
        private bool jammed;

        public CPU6510(IMemoryBus bus, InterruptLine interrupts)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
            this.interrupts = interrupts ?? throw new ArgumentNullException(nameof(interrupts));
        }

        public void SetPC(ushort address)
        {
            registers.PC = address;
        }

        public ushort GetPC()
        {
            return registers.PC;
        }

        public byte A
        {
            get { return registers.A; }
            set { registers.A = value; }
        }

        public byte X
        {
            get { return registers.X; }
            set { registers.X = value; }
        }

        public byte Y
        {
            get { return registers.Y; }
            set { registers.Y = value; }
        }

        public byte S
        {
            get { return registers.S; }
            set { registers.S = value; }
        }

        public byte P
        {
            get { return registers.P; }
            set { registers.P = value; }
        }

        #region Helper Functions

        private byte getHighByte(ushort data)
        {
            return (byte)(data >> 8);
        }

        private void setHighByte(ref ushort data, byte value)
        {
            data =
                (ushort)(
                (data & 0x00FF) |
                ((ushort)value << 8)
                );
        }

        private byte getLowByte(ushort data)
        {
            return (byte)(data & 0x00FF);
        }

        private void setLowByte(ref ushort data, byte value)
        {
            data =
                (ushort)(
                (data & 0xFF00) |
                (ushort)value
                );
        }

        private void setFlag(StatusFlags flag, bool on)
        {
            if (on)
            {
                byte mask = (byte)flag;
                registers.P = (byte)(registers.P | mask);
            }
            else
            {
                byte mask = (byte)(~flag);
                registers.P = (byte)(registers.P & mask);
            }

        }

        private bool getFlag(StatusFlags flag)
        {
            byte mask = (byte)flag;
            return (registers.P & mask) == mask;
        }

        private ushort peekWord(ushort address)
        {
            return bus.PeekWord(address);
        }

        private ushort peekWordZeroPage(byte address)
        {
            return
                (ushort)(
                bus.Peek(address) |
                (bus.Peek((byte)(address + 1)) << 8)
                );
        }

        #endregion

        #region Stack Functions

        private void stackPush(byte data)
        {
            bus.Poke((ushort)(0x0100 + registers.S), data);
            registers.S--;
        }

        private byte stackPull()
        {
            registers.S++;
            return bus.Peek((ushort)(0x0100 + registers.S));
        }

        private void pushStatus(bool isBreak)
        {
            byte status = (byte)((registers.P | 0x20) & ~0x10);

            if (isBreak)
                status |= 0x10;

            stackPush(status);
        }

        private void pullStatus()
        {
            registers.P = (byte)((stackPull() & ~0x10) | 0x20);
        }

        #endregion
        
        #region Addressing Modes

        private ushort immediate()
        {
            ushort ret = registers.PC;
            registers.PC++;
            return ret;
        }

        private byte zeroPage()
        {
            byte ret = bus.Peek(registers.PC);
            registers.PC++;
            return ret;
        }

        private byte zeroPageX()
        {
            byte ret = (byte)(bus.Peek(registers.PC) + registers.X);
            registers.PC++;
            return ret;
        }

        private byte zeroPageY()
        {
            byte ret = (byte)(bus.Peek(registers.PC) + registers.Y);
            registers.PC++;
            return ret;
        }

        private ushort absolute()
        {
            ushort ret = peekWord(registers.PC);
            registers.PC += 2;
            return ret;
        }

        private ushort absoluteX(bool pageCheck)
        {
            ushort baseAddress = peekWord(registers.PC);
            ushort address = (ushort)(baseAddress + registers.X);

            if (pageCheck && (baseAddress & 0xFF00) != (address & 0xFF00))
                cycles++;

            registers.PC += 2;
            return address;
        }

        private ushort absoluteY(bool pageCheck)
        {
            ushort baseAddress = peekWord(registers.PC);
            ushort address = (ushort)(baseAddress + registers.Y);

            if (pageCheck && (baseAddress & 0xFF00) != (address & 0xFF00))
                cycles++;

            registers.PC += 2;
            return address;
        }

        private ushort indirect()
        {
            ushort pointer = peekWord(registers.PC);
            registers.PC += 2;

            // The NMOS 6502 wraps the high byte within the same page.
            byte low = bus.Peek(pointer);
            byte high = bus.Peek(
                (ushort)((pointer & 0xFF00) | ((pointer + 1) & 0x00FF))
                );

            return (ushort)(low | (high << 8));
        }

        private ushort indirectX()
        {
            byte pointer = (byte)(bus.Peek(registers.PC) + registers.X);
            registers.PC++;
            return peekWordZeroPage(pointer);
        }

        private ushort indirectY(bool pageCheck)
        {
            byte pointer = bus.Peek(registers.PC);
            ushort baseAddress = peekWordZeroPage(pointer);
            ushort address = (ushort)(baseAddress + registers.Y);

            if (pageCheck && (baseAddress & 0xFF00) != (address & 0xFF00))
                cycles++;

            registers.PC++;
            return address;
        }

        #endregion

        #region Generic Branching Instructions

        private void branch(StatusFlags flag, bool branchIfSet)
        {
            ushort nextInstruction = (ushort)(registers.PC + 1);

            if (getFlag(flag) != branchIfSet)
            {
                registers.PC = nextInstruction;
                return;
            }

            ushort target =
                (ushort)(nextInstruction + (sbyte)bus.Peek(registers.PC));

            cycles++;

            if ((target & 0xFF00) != (nextInstruction & 0xFF00))
                cycles++;

            registers.PC = target;
        }

        private void bfs(StatusFlags flag)
        {
            branch(flag, true);
        }

        private void bfc(StatusFlags flag)
        {
            branch(flag, false);
        }

        #endregion

		#region Instructions

        private void adc(ushort address)
        {
            byte value = bus.Peek(address);

            if (getFlag(StatusFlags.D))
            {
                adcDecimal(value);
                return;
            }

            int carry = getFlag(StatusFlags.C) ? 1 : 0;
            int sum = registers.A + value + carry;

            setFlag(
                StatusFlags.V,
                ((~(registers.A ^ value) & (registers.A ^ sum)) & 0x80) != 0
                );

            registers.A = (byte)sum;
            setFlag(StatusFlags.C, sum > 0xFF);
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, (registers.A & 0x80) != 0);
        }

        private void adcDecimal(byte value)
        {
            int carry = getFlag(StatusFlags.C) ? 1 : 0;
            int binarySum = registers.A + value + carry;

            // NMOS 6502 ADC decimal algorithm (Bruce Clark, 6502.org):
            // adjust the low nibble first, then build the high nibble. N and V
            // come from the pre-final-adjust value; Z comes from the binary sum.
            int low = (registers.A & 0x0F) + (value & 0x0F) + carry;

            if (low >= 0x0A)
                low = ((low + 0x06) & 0x0F) + 0x10;

            int intermediate = (registers.A & 0xF0) + (value & 0xF0) + low;
            int adjusted = intermediate;

            if (adjusted >= 0xA0)
                adjusted += 0x60;

            setFlag(StatusFlags.N, (intermediate & 0x80) != 0);
            setFlag(
                StatusFlags.V,
                ((~(registers.A ^ value) & (registers.A ^ intermediate)) & 0x80) != 0
                );
            setFlag(StatusFlags.Z, (binarySum & 0xFF) == 0);
            setFlag(StatusFlags.C, adjusted >= 0x100);

            registers.A = (byte)adjusted;
        }

        private void sbc(ushort address)
        {
            byte value = bus.Peek(address);

            if (getFlag(StatusFlags.D))
            {
                sbcDecimal(value);
                return;
            }

            int borrow = getFlag(StatusFlags.C) ? 0 : 1;
            int difference = registers.A - value - borrow;

            setFlag(
                StatusFlags.V,
                (((registers.A ^ value) & (registers.A ^ difference)) & 0x80) != 0
                );

            registers.A = (byte)difference;
            setFlag(StatusFlags.C, difference >= 0);
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, (registers.A & 0x80) != 0);
        }

        private void sbcDecimal(byte value)
        {
            int borrow = getFlag(StatusFlags.C) ? 0 : 1;
            int binaryDifference = registers.A - value - borrow;

            // NMOS 6502 SBC decimal algorithm. The accumulator is adjusted BCD;
            // C, N, V and Z are all based on the binary subtraction.
            int low = (registers.A & 0x0F) - (value & 0x0F) - borrow;

            if (low < 0)
                low = ((low - 0x06) & 0x0F) - 0x10;

            int adjusted = (registers.A & 0xF0) - (value & 0xF0) + low;

            if (adjusted < 0)
                adjusted -= 0x60;

            setFlag(StatusFlags.C, binaryDifference >= 0);
            setFlag(StatusFlags.Z, (binaryDifference & 0xFF) == 0);
            setFlag(StatusFlags.N, (binaryDifference & 0x80) != 0);
            setFlag(
                StatusFlags.V,
                (((registers.A ^ value) & (registers.A ^ binaryDifference)) & 0x80) != 0
                );

            registers.A = (byte)adjusted;
        }

        private void and(ushort address)
        {
            registers.A &= bus.Peek(address);
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void ora(ushort address)
        {
            registers.A |= bus.Peek(address);
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void asl()
        {
            setFlag(StatusFlags.C, (registers.A & 0x80) != 0);
            registers.A <<= 1;
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void asl(ushort address)
        {
            byte b = bus.Peek(address);
            setFlag(StatusFlags.C, (b & 0x80) != 0);
            b <<= 1;
            bus.Poke(address, b);
            setFlag(StatusFlags.Z, b == 0);
            setFlag(StatusFlags.N, b >= 0x80);
        }

        private void rol()
        {
            bool bit = getFlag(StatusFlags.C);
            setFlag(StatusFlags.C, (registers.A & 0x80) != 0);
            registers.A <<= 1;
            if (bit) registers.A |= 0x01;
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void rol(ushort address)
        {
            byte b = bus.Peek(address);
            bool bit = getFlag(StatusFlags.C);
            setFlag(StatusFlags.C, (b & 0x80) != 0);
            b <<= 1;
            if (bit) b |= 0x01;
            bus.Poke(address, b);
            setFlag(StatusFlags.Z, b == 0);
            setFlag(StatusFlags.N, b >= 0x80);
        }

        private void ror()
        {
            bool bit = getFlag(StatusFlags.C);
            setFlag(StatusFlags.C, (registers.A & 0x01) != 0);
            registers.A >>= 1;
            if (bit) registers.A |= 0x80;
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void ror(ushort address)
        {
            byte b = bus.Peek(address);
            bool bit = getFlag(StatusFlags.C);
            setFlag(StatusFlags.C, (b & 0x01) != 0);
            b >>= 1;
            if (bit) b |= 0x80;
            bus.Poke(address, b);
            setFlag(StatusFlags.Z, b == 0);
            setFlag(StatusFlags.N, b >= 0x80);
        }

        private void lsr()
        {
            setFlag(StatusFlags.C, (registers.A & 0x01) != 0);
            registers.A >>= 1;
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, false);
        }

        private void lsr(ushort address)
        {
            byte b = bus.Peek(address);
            setFlag(StatusFlags.C, (b & 0x01) != 0);
            b >>= 1;
            bus.Poke(address, b);
            setFlag(StatusFlags.Z, b == 0);
            setFlag(StatusFlags.N, false);
        }

        private void bit(ushort address)
        {
            byte h = bus.Peek(address);
            setFlag(StatusFlags.N, (h & 0x80) != 0);
            setFlag(StatusFlags.V, (h & 0x40) != 0);
            setFlag(StatusFlags.Z, (h & registers.A) == 0);
        }

        private void cmp(ushort address)
        {
            ushort h = (ushort)(registers.A - bus.Peek(address));
            setFlag(StatusFlags.C, h <= 0xFF);
            setFlag(StatusFlags.Z, getLowByte(h) == 0);
            setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
        }

        private void cpx(ushort address)
        {
            ushort h = (ushort)(registers.X - bus.Peek(address));
            setFlag(StatusFlags.C, h <= 0xFF);
            setFlag(StatusFlags.Z, getLowByte(h) == 0);
            setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
        }

        private void cpy(ushort address)
        {
            ushort h = (ushort)(registers.Y - bus.Peek(address));
            setFlag(StatusFlags.C, h <= 0xFF);
            setFlag(StatusFlags.Z, getLowByte(h) == 0);
            setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
        }

        private void dec(ushort address)
        {
            ushort h = (ushort)(bus.Peek(address) - 1);
            bus.Poke(address, getLowByte(h));
            setFlag(StatusFlags.Z, getLowByte(h) == 0);
            setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
        }

        private void dex()
        {
            ushort h = (ushort)(registers.X - 1);
            registers.X = getLowByte(h);
            setFlag(StatusFlags.Z, registers.X == 0);
            setFlag(StatusFlags.N, registers.X >= 0x80);
        }

        private void dey()
        {
            ushort h = (ushort)(registers.Y - 1);
            registers.Y = getLowByte(h);
            setFlag(StatusFlags.Z, registers.Y == 0);
            setFlag(StatusFlags.N, registers.Y >= 0x80);
        }

        private void eor(ushort address)
        {
            registers.A ^= bus.Peek(address);
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void inc(ushort address)
        {
            ushort h = (ushort)(bus.Peek(address) + 1);
            bus.Poke(address, getLowByte(h));
            setFlag(StatusFlags.Z, getLowByte(h) == 0);
            setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
        }

        private void inx()
        {
            ushort h = (ushort)(registers.X + 1);
            registers.X = getLowByte(h);
            setFlag(StatusFlags.Z, registers.X == 0);
            setFlag(StatusFlags.N, registers.X >= 0x80);
        }

        private void iny()
        {
            ushort h = (ushort)(registers.Y + 1);
            registers.Y = getLowByte(h);
            setFlag(StatusFlags.Z, registers.Y == 0);
            setFlag(StatusFlags.N, registers.Y >= 0x80);
        }

        private void jmp(ushort address)
        {
            registers.PC = address;
        }

        private void jsr(ushort address)
        {
            registers.PC--;
            stackPush(getHighByte(registers.PC));
            stackPush(getLowByte(registers.PC));
            registers.PC = address;
        }

        private void rts()
        {
            setLowByte(ref registers.PC, stackPull());
            setHighByte(ref registers.PC, stackPull());
            registers.PC++;
        }

        private void rti()
        {
            pullStatus();
            setLowByte(ref registers.PC, stackPull());
            setHighByte(ref registers.PC, stackPull());
        }

        private void lda(ushort address)
        {
            registers.A = bus.Peek(address);
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void ldx(ushort address)
        {
            registers.X = bus.Peek(address);
            setFlag(StatusFlags.Z, registers.X == 0);
            setFlag(StatusFlags.N, registers.X >= 0x80);
        }

        private void ldy(ushort address)
        {
            registers.Y = bus.Peek(address);
            setFlag(StatusFlags.Z, registers.Y == 0);
            setFlag(StatusFlags.N, registers.Y >= 0x80);
        }

        private void pla()
        {
            registers.A = stackPull();
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void sta(ushort address)
        {
            bus.Poke(address, registers.A);
        }

        private void stx(ushort address)
        {
            bus.Poke(address, registers.X);
        }

        private void sty(ushort address)
        {
            bus.Poke(address, registers.Y);
        }

        private void tax()
        {
            registers.X = registers.A;
            setFlag(StatusFlags.Z, registers.X == 0);
            setFlag(StatusFlags.N, registers.X >= 0x80);
        }

        private void tay()
        {
            registers.Y = registers.A;
            setFlag(StatusFlags.Z, registers.Y == 0);
            setFlag(StatusFlags.N, registers.Y >= 0x80);
        }

        private void tsx()
        {
            registers.X = registers.S;
            setFlag(StatusFlags.Z, registers.X == 0);
            setFlag(StatusFlags.N, registers.X >= 0x80);
        }

        private void txa()
        {
            registers.A = registers.X;
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

        private void txs()
        {
            registers.S = registers.X;
        }

        private void tya()
        {
            registers.A = registers.Y;
            setFlag(StatusFlags.Z, registers.A == 0);
            setFlag(StatusFlags.N, registers.A >= 0x80);
        }

		#endregion

        #region Interrupts/Reset

        private void brk()
        {
            registers.PC++;
            stackPush(getHighByte(registers.PC));
            stackPush(getLowByte(registers.PC));
            pushStatus(true);
            setFlag(StatusFlags.I, true);
            registers.PC = peekWord(0xFFFE);
        }

        private void irq()
        {
            if (getFlag(StatusFlags.I))
                return;

            stackPush(getHighByte(registers.PC));
            stackPush(getLowByte(registers.PC));
            pushStatus(false);
            setFlag(StatusFlags.I, true);
            registers.PC = peekWord(0xFFFE);
        }

        private void nmi()
        {
            stackPush(getHighByte(registers.PC));
            stackPush(getLowByte(registers.PC));
            pushStatus(false);
            setFlag(StatusFlags.I, true);
            registers.PC = peekWord(0xFFFA);
        }

        public bool IsJammed => jammed;

        /// <summary>
        /// Completes a trapped JSR without executing the called routine. Used
        /// by the machine's KERNAL LOAD fast path.
        /// </summary>
        public int ReturnFromSubroutine()
        {
            rts();
            return 6;
        }

        public void Reset()
        {
            jammed = false;
            registers.PC = peekWord(AddressMap.ResetVector);
            setFlag(StatusFlags.I, true);
        }

        #endregion

        public int LastInstructionCycles => cycles;

        /// <summary>
        /// Executes exactly one instruction, or services a pending interrupt at
        /// the instruction boundary. Devices are ticked by the machine loop.
        /// </summary>
        public int Step()
        {
            cycles = 0;

            if (jammed)
                return 0;

            if (interrupts.IsNmiAsserted)
            {
                interrupts.AcknowledgeNmi();
                nmi();
                return cycles = 7;
            }

            if (interrupts.IsIrqAsserted && !getFlag(StatusFlags.I))
            {
                irq();
                return cycles = 7;
            }

            byte instruction = bus.Peek(registers.PC);
            registers.PC++;
            ushort address = 0;

            switch ((Instructions)instruction)
            {
                // ADC
                case Instructions.ADC_imm: cycles += 2; adc(immediate()); break;
                case Instructions.ADC_zp: cycles += 3; adc(zeroPage()); break;
                case Instructions.ADC_zpx: cycles += 4; adc(zeroPageX()); break;
                case Instructions.ADC_abs: cycles += 4; adc(absolute()); break;
                case Instructions.ADC_abx: cycles += 4; adc(absoluteX(true)); break;
                case Instructions.ADC_aby: cycles += 4; adc(absoluteY(true)); break;
                case Instructions.ADC_izx: cycles += 6; adc(indirectX()); break;
                case Instructions.ADC_izy: cycles += 5; adc(indirectY(true)); break;

                // SBC
                case Instructions.SBC_imm:
                case Instructions.SBC:
                    cycles += 2; sbc(immediate()); break;
                case Instructions.SBC_zp: cycles += 3; sbc(zeroPage()); break;
                case Instructions.SBC_zpx: cycles += 4; sbc(zeroPageX()); break;
                case Instructions.SBC_abs: cycles += 4; sbc(absolute()); break;
                case Instructions.SBC_abx: cycles += 4; sbc(absoluteX(true)); break;
                case Instructions.SBC_aby: cycles += 4; sbc(absoluteY(true)); break;
                case Instructions.SBC_izx: cycles += 6; sbc(indirectX()); break;
                case Instructions.SBC_izy: cycles += 5; sbc(indirectY(true)); break;

                // AND
                case Instructions.AND_imm: cycles += 2; and(immediate()); break;
                case Instructions.AND_zp: cycles += 3; and(zeroPage()); break;
                case Instructions.AND_zpx: cycles += 4; and(zeroPageX()); break;
                case Instructions.AND_abs: cycles += 4; and(absolute()); break;
                case Instructions.AND_abx: cycles += 4; and(absoluteX(true)); break;
                case Instructions.AND_aby: cycles += 4; and(absoluteY(true)); break;
                case Instructions.AND_izx: cycles += 6; and(indirectX()); break;
                case Instructions.AND_izy: cycles += 5; and(indirectY(true)); break;

                // ORA
                case Instructions.ORA_imm: cycles += 2; ora(immediate()); break;
                case Instructions.ORA_zp: cycles += 3; ora(zeroPage()); break;
                case Instructions.ORA_zpx: cycles += 4; ora(zeroPageX()); break;
                case Instructions.ORA_abs: cycles += 4; ora(absolute()); break;
                case Instructions.ORA_abx: cycles += 4; ora(absoluteX(true)); break;
                case Instructions.ORA_aby: cycles += 4; ora(absoluteY(true)); break;
                case Instructions.ORA_izx: cycles += 6; ora(indirectX()); break;
                case Instructions.ORA_izy: cycles += 5; ora(indirectY(true)); break;

                // ASL
                case Instructions.ASL_imp: cycles += 2; asl(); break;
                case Instructions.ASL_zp: cycles += 5; asl(zeroPage()); break;
                case Instructions.ASL_zpx: cycles += 6; asl(zeroPageX()); break;
                case Instructions.ASL_abs: cycles += 6; asl(absolute()); break;
                case Instructions.ASL_abx: cycles += 7; asl(absoluteX(false)); break;

                // ROL
                case Instructions.ROL_imp: cycles += 2; rol(); break;
                case Instructions.ROL_zp: cycles += 5; rol(zeroPage()); break;
                case Instructions.ROL_zpx: cycles += 6; rol(zeroPageX()); break;
                case Instructions.ROL_abs: cycles += 6; rol(absolute()); break;
                case Instructions.ROL_abx: cycles += 7; rol(absoluteX(false)); break;

                // ROR
                case Instructions.ROR_imp: cycles += 2; ror(); break;
                case Instructions.ROR_zp: cycles += 5; ror(zeroPage()); break;
                case Instructions.ROR_zpx: cycles += 6; ror(zeroPageX()); break;
                case Instructions.ROR_abs: cycles += 6; ror(absolute()); break;
                case Instructions.ROR_abx: cycles += 7; ror(absoluteX(false)); break;

                // LSR
                case Instructions.LSR_imp: cycles += 2; lsr(); break;
                case Instructions.LSR_zp: cycles += 5; lsr(zeroPage()); break;
                case Instructions.LSR_zpx: cycles += 6; lsr(zeroPageX()); break;
                case Instructions.LSR_abs: cycles += 6; lsr(absolute()); break;
                case Instructions.LSR_abx: cycles += 7; lsr(absoluteX(false)); break;

                // BIT
                case Instructions.BIT_zp: cycles += 3; bit(zeroPage()); break;
                case Instructions.BIT_abs: cycles += 4; bit(absolute()); break;

                // BRK
                case Instructions.BRK: cycles += 7; brk(); break;

                // CMP
                case Instructions.CMP_imm: cycles += 2; cmp(immediate()); break;
                case Instructions.CMP_zp: cycles += 3; cmp(zeroPage()); break;
                case Instructions.CMP_zpx: cycles += 4; cmp(zeroPageX()); break;
                case Instructions.CMP_abs: cycles += 4; cmp(absolute()); break;
                case Instructions.CMP_abx: cycles += 4; cmp(absoluteX(true)); break;
                case Instructions.CMP_aby: cycles += 4; cmp(absoluteY(true)); break;
                case Instructions.CMP_izx: cycles += 6; cmp(indirectX()); break;
                case Instructions.CMP_izy: cycles += 5; cmp(indirectY(true)); break;

                // CPX
                case Instructions.CPX_imm: cycles += 2; cpx(immediate()); break;
                case Instructions.CPX_zp: cycles += 3; cpx(zeroPage()); break;
                case Instructions.CPX_abs: cycles += 4; cpx(absolute()); break;

                // CPY
                case Instructions.CPY_imm: cycles += 2; cpy(immediate()); break;
                case Instructions.CPY_zp: cycles += 3; cpy(zeroPage()); break;
                case Instructions.CPY_abs: cycles += 4; cpy(absolute()); break;

                // DEC
                case Instructions.DEC_zp: cycles += 5; dec(zeroPage()); break;
                case Instructions.DEC_zpx: cycles += 6; dec(zeroPageX()); break;
                case Instructions.DEC_abs: cycles += 6; dec(absolute()); break;
                case Instructions.DEC_abx: cycles += 7; dec(absoluteX(false)); break;

                // DEX
                case Instructions.DEX: cycles += 2; dex(); break;

                // DEY
                case Instructions.DEY: cycles += 2; dey(); break;

                // EOR
                case Instructions.EOR_imm: cycles += 2; eor(immediate()); break;
                case Instructions.EOR_zp: cycles += 3; eor(zeroPage()); break;
                case Instructions.EOR_zpx: cycles += 4; eor(zeroPageX()); break;
                case Instructions.EOR_abs: cycles += 4; eor(absolute()); break;
                case Instructions.EOR_abx: cycles += 4; eor(absoluteX(true)); break;
                case Instructions.EOR_aby: cycles += 4; eor(absoluteY(true)); break;
                case Instructions.EOR_izx: cycles += 6; eor(indirectX()); break;
                case Instructions.EOR_izy: cycles += 5; eor(indirectY(true)); break;

                // INC
                case Instructions.INC_zp: cycles += 5; inc(zeroPage()); break;
                case Instructions.INC_zpx: cycles += 6; inc(zeroPageX()); break;
                case Instructions.INC_abs: cycles += 6; inc(absolute()); break;
                case Instructions.INC_abx: cycles += 7; inc(absoluteX(false)); break;

                // INX
                case Instructions.INX: cycles += 2; inx(); break;

                // INY
                case Instructions.INY: cycles += 2; iny(); break;

                // JMP
                case Instructions.JMP_abs: cycles += 3; jmp(absolute()); break;
                case Instructions.JMP_ind: cycles += 5; jmp(indirect()); break;

                // JSR
                case Instructions.JSR: cycles += 6; jsr(absolute()); break;

                // RTS
                case Instructions.RTS: cycles += 6; rts(); break;

                // RTI
                case Instructions.RTI: cycles += 6; rti(); break;

                // LDA
                case Instructions.LDA_imm: cycles += 2; lda(immediate()); break;
                case Instructions.LDA_zp: cycles += 3; lda(zeroPage()); break;
                case Instructions.LDA_zpx: cycles += 4; lda(zeroPageX()); break;
                case Instructions.LDA_abs: cycles += 4; lda(absolute()); break;
                case Instructions.LDA_abx: cycles += 4; lda(absoluteX(true)); break;
                case Instructions.LDA_aby: cycles += 4; lda(absoluteY(true)); break;
                case Instructions.LDA_izx: cycles += 6; lda(indirectX()); break;
                case Instructions.LDA_izy: cycles += 5; lda(indirectY(true)); break;

                // LDX
                case Instructions.LDX_imm: cycles += 2; ldx(immediate()); break;
                case Instructions.LDX_zp: cycles += 3; ldx(zeroPage()); break;
                case Instructions.LDX_zpy: cycles += 4; ldx(zeroPageY()); break;
                case Instructions.LDX_abs: cycles += 4; ldx(absolute()); break;
                case Instructions.LDX_aby: cycles += 4; ldx(absoluteY(true)); break;

                // LDY
                case Instructions.LDY_imm: cycles += 2; ldy(immediate()); break;
                case Instructions.LDY_zp: cycles += 3; ldy(zeroPage()); break;
                case Instructions.LDY_zpx: cycles += 4; ldy(zeroPageX()); break;
                case Instructions.LDY_abs: cycles += 4; ldy(absolute()); break;
                case Instructions.LDY_abx: cycles += 4; ldy(absoluteX(true)); break;

                // STA
                case Instructions.STA_zp: cycles += 3; sta(zeroPage()); break;
                case Instructions.STA_zpx: cycles += 4; sta(zeroPageX()); break;
                case Instructions.STA_abs: cycles += 4; sta(absolute()); break;
                case Instructions.STA_abx: cycles += 5; sta(absoluteX(false)); break;
                case Instructions.STA_aby: cycles += 5; sta(absoluteY(false)); break;
                case Instructions.STA_izx: cycles += 6; sta(indirectX()); break;
                case Instructions.STA_izy: cycles += 6; sta(indirectY(false)); break;

                // STX
                case Instructions.STX_zp: cycles += 3; stx(zeroPage()); break;
                case Instructions.STX_zpy: cycles += 4; stx(zeroPageY()); break;
                case Instructions.STX_abs: cycles += 4; stx(absolute()); break;

                // STY
                case Instructions.STY_zp: cycles += 3; sty(zeroPage()); break;
                case Instructions.STY_zpx: cycles += 4; sty(zeroPageX()); break;
                case Instructions.STY_abs: cycles += 4; sty(absolute()); break;

                // TAX
                case Instructions.TAX: cycles += 2; tax(); break;

                // TAY
                case Instructions.TAY: cycles += 2; tay(); break;

                // TSX
                case Instructions.TSX: cycles += 2; tsx(); break;

                // TXA
                case Instructions.TXA: cycles += 2; txa(); break;

                // TXS
                case Instructions.TXS: cycles += 2; txs(); break;

                // TYA
                case Instructions.TYA: cycles += 2; tya(); break;

                // CLC
                case Instructions.CLC: cycles += 2; setFlag(StatusFlags.C, false); break;

                // CLD
                case Instructions.CLD: cycles += 2; setFlag(StatusFlags.D, false); break;

                // CLI
                case Instructions.CLI: cycles += 2; setFlag(StatusFlags.I, false); break;

                // CLV
                case Instructions.CLV: cycles += 2; setFlag(StatusFlags.V, false); break;

                // BCC
                case Instructions.BCC: cycles += 2; bfc(StatusFlags.C); break;

                // BCS
                case Instructions.BCS: cycles += 2; bfs(StatusFlags.C); break;

                // BEQ
                case Instructions.BEQ: cycles += 2; bfs(StatusFlags.Z); break;

                // BMI
                case Instructions.BMI: cycles += 2; bfs(StatusFlags.N); break;

                // BNE
                case Instructions.BNE: cycles += 2; bfc(StatusFlags.Z); break;

                // BPL
                case Instructions.BPL: cycles += 2; bfc(StatusFlags.N); break;

                // BVC
                case Instructions.BVC: cycles += 2; bfc(StatusFlags.V); break;

                // BVS
                case Instructions.BVS: cycles += 2; bfs(StatusFlags.V); break;

                // NOP
                case Instructions.NOP:
                case Instructions.NOP01:
                case Instructions.NOP02:
                case Instructions.NOP03:
                case Instructions.NOP04:
                case Instructions.NOP05:
                case Instructions.NOP06:
                    cycles += 2; break;

                case Instructions.NOP_imm01:
                case Instructions.NOP_imm02:
                case Instructions.NOP_imm03:
                case Instructions.NOP_imm04:
                case Instructions.NOP_imm05:
                    cycles += 2; break;

                case Instructions.NOP_zp01:
                case Instructions.NOP_zp02:
                case Instructions.NOP_zp03:
                    cycles += 3; break;

                case Instructions.NOP_zpx01:
                case Instructions.NOP_zpx02:
                case Instructions.NOP_zpx03:
                case Instructions.NOP_zpx04:
                case Instructions.NOP_zpx05:
                case Instructions.NOP_zpx06:
                    cycles += 4; break;

                case Instructions.NOP_abs: cycles += 4; break;

                case Instructions.NOP_abx01:
                case Instructions.NOP_abx02:
                case Instructions.NOP_abx03:
                case Instructions.NOP_abx04:
                case Instructions.NOP_abx05:
                case Instructions.NOP_abx06:
                    cycles += 4; absoluteX(true); break;

                // PHA
                case Instructions.PHA: cycles += 3; stackPush(registers.A); break;

                // PHP
                case Instructions.PHP: cycles += 3; stackPush((byte)(registers.P | 0x30)); break;

                // PLA
                case Instructions.PLA: cycles += 4; pla(); break;

                // PLP
                case Instructions.PLP: cycles += 4; pullStatus(); break;

                // SEC
                case Instructions.SEC: cycles += 2; setFlag(StatusFlags.C, true); break;

                // SED
                case Instructions.SED: cycles += 2; setFlag(StatusFlags.D, true); break;

                // SEI
                case Instructions.SEI: cycles += 2; setFlag(StatusFlags.I, true); break;

                // ILLEGAL OPCODES------------------------------------------------------

                // JAM
                case Instructions.JAM01:
                case Instructions.JAM02:
                case Instructions.JAM03:
                case Instructions.JAM04:
                case Instructions.JAM05:
                case Instructions.JAM06:
                case Instructions.JAM07:
                case Instructions.JAM08:
                case Instructions.JAM09:
                case Instructions.JAM10:
                case Instructions.JAM11:
                case Instructions.JAM12:
                    // A JAM opcode locks the real CPU until reset.
                    jammed = true;
                    break;

                // ANC
                case Instructions.ANC01:
                case Instructions.ANC02:
                    cycles += 2;
                    and(immediate());
                    setFlag(StatusFlags.C, (registers.A & 0x80) != 0);
                    break;

                // ANE
                case Instructions.ANE: cycles += 2; break;

                // ARR
                case Instructions.ARR: cycles += 2; and(immediate()); ror(); break;

                // ASR
                case Instructions.ASR: cycles += 2; and(immediate()); lsr(); break;

                // DCP
                case Instructions.DCP_zp: cycles += 5; address = zeroPage(); dec(address); cmp(address); break;
                case Instructions.DCP_zpx: cycles += 6; address = zeroPageX(); dec(address); cmp(address); break;
                case Instructions.DCP_abs: cycles += 6; address = absolute(); dec(address); cmp(address); break;
                case Instructions.DCP_abx: cycles += 7; address = absoluteX(false); dec(address); cmp(address); break;
                case Instructions.DCP_aby: cycles += 7; address = absoluteY(false); dec(address); cmp(address); break;
                case Instructions.DCP_izx: cycles += 8; address = indirectX(); dec(address); cmp(address); break;
                case Instructions.DCP_izy: cycles += 8; address = indirectY(false); dec(address); cmp(address); break;

                // ICB
                case Instructions.ISB_zp: cycles += 5; address = zeroPage(); inc(address); sbc(address); break;
                case Instructions.ISB_zpx: cycles += 6; address = zeroPageX(); inc(address); sbc(address); break;
                case Instructions.ISB_abs: cycles += 6; address = absolute(); inc(address); sbc(address); break;
                case Instructions.ISB_abx: cycles += 7; address = absoluteX(false); inc(address); sbc(address); break;
                case Instructions.ISB_aby: cycles += 7; address = absoluteY(false); inc(address); sbc(address); break;
                case Instructions.ISB_izx: cycles += 8; address = indirectX(); inc(address); sbc(address); break;
                case Instructions.ISB_izy: cycles += 8; address = indirectY(false); inc(address); sbc(address); break;

                //LAE
                case Instructions.LAE:
                    cycles += 4;
                    registers.S &= bus.Peek(absoluteY(true));
                    tsx();
                    txa();
                    break;

                // LAX
                case Instructions.LAX_zp: cycles += 3; lda(zeroPage()); tax(); break;
                case Instructions.LAX_zpy: cycles += 4; lda(zeroPageY()); tax(); break;
                case Instructions.LAX_abs: cycles += 4; lda(absolute()); tax(); break;
                case Instructions.LAX_aby: cycles += 4; lda(absoluteY(true)); tax(); break;
                case Instructions.LAX_izx: cycles += 6; lda(indirectX()); tax(); break;
                case Instructions.LAX_izy: cycles += 5; lda(indirectY(true)); tax(); break;

                // LXA
                case Instructions.LXA: cycles += 2; break;

                // RLA
                case Instructions.RLA_zp: cycles += 5; address = zeroPage(); rol(address); and(address); break;
                case Instructions.RLA_zpx: cycles += 6; address = zeroPageX(); rol(address); and(address); break;
                case Instructions.RLA_abs: cycles += 6; address = absolute(); rol(address); and(address); break;
                case Instructions.RLA_abx: cycles += 7; address = absoluteX(false); rol(address); and(address); break;
                case Instructions.RLA_aby: cycles += 7; address = absoluteY(false); rol(address); and(address); break;
                case Instructions.RLA_izx: cycles += 8; address = indirectX(); rol(address); and(address); break;
                case Instructions.RLA_izy: cycles += 8; address = indirectY(false); rol(address); and(address); break;

                // RRA
                case Instructions.RRA_zp: cycles += 5; address = zeroPage(); ror(address); adc(address); break;
                case Instructions.RRA_zpx: cycles += 6; address = zeroPageX(); ror(address); adc(address); break;
                case Instructions.RRA_abs: cycles += 6; address = absolute(); ror(address); adc(address); break;
                case Instructions.RRA_abx: cycles += 7; address = absoluteX(false); ror(address); adc(address); break;
                case Instructions.RRA_aby: cycles += 7; address = absoluteY(false); ror(address); adc(address); break;
                case Instructions.RRA_izx: cycles += 8; address = indirectX(); ror(address); adc(address); break;
                case Instructions.RRA_izy: cycles += 8; address = indirectY(false); ror(address); adc(address); break;

                // SAX
                case Instructions.SAX_zp: cycles += 3; bus.Poke(zeroPage(), (byte)(registers.A & registers.X)); break;
                case Instructions.SAX_zpy: cycles += 4; bus.Poke(zeroPageY(), (byte)(registers.A & registers.X)); break;
                case Instructions.SAX_abs: cycles += 4; bus.Poke(absolute(), (byte)(registers.A & registers.X)); break;
                case Instructions.SAX_izx: cycles += 6; bus.Poke(indirectX(), (byte)(registers.A & registers.X)); break;

                // SBX
                case Instructions.SBX: cycles += 2; break;

                // SHA
                case Instructions.SHA_abx: cycles += 5; break;
                case Instructions.SHA_aby: cycles += 5; break;

                // SHS
                case Instructions.SHS: cycles += 5; break;

                // SHX
                case Instructions.SHX: cycles += 5; break;

                // SHY
                case Instructions.SHY: cycles += 5; break;

                // SLO
                case Instructions.SLO_zp: cycles += 5; address = zeroPage(); asl(address); ora(address); break;
                case Instructions.SLO_zpx: cycles += 6; address = zeroPageX(); asl(address); ora(address); break;
                case Instructions.SLO_abs: cycles += 6; address = absolute(); asl(address); ora(address); break;
                case Instructions.SLO_abx: cycles += 7; address = absoluteX(false); asl(address); ora(address); break;
                case Instructions.SLO_aby: cycles += 7; address = absoluteY(false); asl(address); ora(address); break;
                case Instructions.SLO_izx: cycles += 8; address = indirectX(); asl(address); ora(address); break;
                case Instructions.SLO_izy: cycles += 8; address = indirectY(false); asl(address); ora(address); break;

                // SRE
                case Instructions.SRE_zp: cycles += 5; address = zeroPage(); lsr(address); eor(address); break;
                case Instructions.SRE_zpx: cycles += 6; address = zeroPageX(); lsr(address); eor(address); break;
                case Instructions.SRE_abs: cycles += 6; address = absolute(); lsr(address); eor(address); break;
                case Instructions.SRE_abx: cycles += 7; address = absoluteX(false); lsr(address); eor(address); break;
                case Instructions.SRE_aby: cycles += 7; address = absoluteY(false); lsr(address); eor(address); break;
                case Instructions.SRE_izx: cycles += 8; address = indirectX(); lsr(address); eor(address); break;
                case Instructions.SRE_izy: cycles += 8; address = indirectY(false); lsr(address); eor(address); break;
            }

            return cycles;
        }

    }
}
