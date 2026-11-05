using System;
using C64Emulator.Core;
using C64Emulator.Video;

namespace C64Emulator.Cpu;

/// <summary>
/// A 6502/6510 core. The core is deliberately synchronous: it executes one
/// instruction per <see cref="Step"/> call and leaves timing, pacing and
/// device ticking to <see cref="C64Machine"/>.
/// </summary>
internal sealed class Cpu6510(IMemoryBus bus, InterruptLine interrupts)
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

    private readonly IMemoryBus bus = bus ?? throw new ArgumentNullException(nameof(bus));
    private readonly InterruptLine interrupts = interrupts ?? throw new ArgumentNullException(nameof(interrupts));
    private Registers registers;
    private int cycles;
    private bool jammed;

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
            var mask = (byte)flag;
            registers.P = (byte)(registers.P | mask);
        }
        else
        {
            var mask = (byte)(~flag);
            registers.P = (byte)(registers.P & mask);
        }

    }

    private bool getFlag(StatusFlags flag)
    {
        var mask = (byte)flag;
        return (registers.P & mask) == mask;
    }

    /// <summary>
    /// One CPU read cycle. Counting here instead of in the dispatch keeps
    /// the instruction's cycle total equal to the cycles it actually
    /// issued, including stalls.
    /// </summary>
    private byte read(ushort address)
    {
        cycles++;
        return bus.Peek(address);
    }

    private void write(ushort address, byte value)
    {
        cycles++;
        bus.Poke(address, value);
    }

    /// <summary>
    /// An internal cycle. The 6510 does not commit a value, but the cycle
    /// still occupies the bus, so the VIC keeps fetching while an
    /// instruction is in flight.
    /// </summary>
    private void idle()
    {
        cycles++;
        bus.Idle();
    }

    private void idle(int count)
    {
        for (var i = 0; i < count; i++)
            idle();
    }

    /// <summary>
    /// The NMOS 6502 reads with the carry of the index addition suppressed
    /// before it reads the real address.
    /// </summary>
    private void readUnfixed(ushort baseAddress, ushort address)
    {
        read((ushort)((baseAddress & 0xFF00) | (address & 0x00FF)));
    }

    private ushort peekWord(ushort address)
    {
        return (ushort)(
            read(address) |
            (read((ushort)(address + 1)) << 8));
    }

    private ushort peekWordZeroPage(byte address)
    {
        return
            (ushort)(
            read(address) |
            (read((byte)(address + 1)) << 8)
            );
    }

    #endregion

    #region Stack Functions

    private void stackPush(byte data)
    {
        write((ushort)(0x0100 + registers.S), data);
        registers.S--;
    }

    private byte stackPull()
    {
        registers.S++;
        return read((ushort)(0x0100 + registers.S));
    }

    private void pushStatus(bool isBreak)
    {
        var status = (byte)((registers.P | 0x20) & ~0x10);

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
        var ret = registers.PC;
        registers.PC++;
        return ret;
    }

    private byte zeroPage()
    {
        var ret = read(registers.PC);
        registers.PC++;
        return ret;
    }

    private byte zeroPageX()
    {
        var baseAddress = read(registers.PC);
        registers.PC++;

        // The ALU reads the unindexed zero-page address first.
        read(baseAddress);
        return (byte)(baseAddress + registers.X);
    }

    private byte zeroPageY()
    {
        var baseAddress = read(registers.PC);
        registers.PC++;

        read(baseAddress);
        return (byte)(baseAddress + registers.Y);
    }

    private ushort absolute()
    {
        var low = read(registers.PC);
        var high = read((ushort)(registers.PC + 1));
        registers.PC += 2;
        return (ushort)(low | (high << 8));
    }

    private ushort absoluteX(bool pageCheck)
    {
        var low = read(registers.PC);
        var high = read((ushort)(registers.PC + 1));
        registers.PC += 2;

        var baseAddress = (ushort)(low | (high << 8));
        var address = (ushort)(baseAddress + registers.X);

        // A read pays one extra cycle only when the index crosses a page:
        // the dummy read at the unfixed address is the real read when it
        // does not. Stores and read-modify-writes always pay for it.
        if (pageCheck && (baseAddress & 0xFF00) != (address & 0xFF00))
            readUnfixed(baseAddress, address);

        return address;
    }

    private ushort absoluteXForWrite()
    {
        var low = read(registers.PC);
        var high = read((ushort)(registers.PC + 1));
        registers.PC += 2;

        var baseAddress = (ushort)(low | (high << 8));
        var address = (ushort)(baseAddress + registers.X);

        readUnfixed(baseAddress, address);
        return address;
    }

    private ushort absoluteY(bool pageCheck)
    {
        var low = read(registers.PC);
        var high = read((ushort)(registers.PC + 1));
        registers.PC += 2;

        var baseAddress = (ushort)(low | (high << 8));
        var address = (ushort)(baseAddress + registers.Y);

        if (pageCheck && (baseAddress & 0xFF00) != (address & 0xFF00))
            readUnfixed(baseAddress, address);

        return address;
    }

    private ushort absoluteYForWrite()
    {
        var low = read(registers.PC);
        var high = read((ushort)(registers.PC + 1));
        registers.PC += 2;

        var baseAddress = (ushort)(low | (high << 8));
        var address = (ushort)(baseAddress + registers.Y);

        readUnfixed(baseAddress, address);
        return address;
    }

    private ushort indirect()
    {
        var pointer = peekWord(registers.PC);
        registers.PC += 2;

        // The NMOS 6502 wraps the high byte within the same page.
        var low = read(pointer);
        var high = read(
            (ushort)((pointer & 0xFF00) | ((pointer + 1) & 0x00FF))
            );

        return (ushort)(low | (high << 8));
    }

    private ushort indirectX()
    {
        var pointer = read(registers.PC);
        registers.PC++;

        // Dummy read at the unindexed pointer, then the pointer pair at
        // the indexed zero-page address.
        read(pointer);
        return peekWordZeroPage((byte)(pointer + registers.X));
    }

    private ushort indirectY(bool pageCheck)
    {
        var pointer = read(registers.PC);
        registers.PC++;

        var baseAddress = peekWordZeroPage(pointer);
        var address = (ushort)(baseAddress + registers.Y);

        if (pageCheck && (baseAddress & 0xFF00) != (address & 0xFF00))
            readUnfixed(baseAddress, address);

        return address;
    }

    private ushort indirectYForWrite()
    {
        var pointer = read(registers.PC);
        registers.PC++;

        var baseAddress = peekWordZeroPage(pointer);
        var address = (ushort)(baseAddress + registers.Y);

        readUnfixed(baseAddress, address);
        return address;
    }

    #endregion

    #region Generic Branching Instructions

    private void branch(StatusFlags flag, bool branchIfSet)
    {
        var nextInstruction = (ushort)(registers.PC + 1);
        var offset = (sbyte)read(registers.PC);
        registers.PC = nextInstruction;

        if (getFlag(flag) != branchIfSet)
            return;

        // A taken branch spends one internal cycle, and a second while the
        // target is still in the wrong page.
        idle();

        var target = (ushort)(nextInstruction + offset);

        if ((target & 0xFF00) != (nextInstruction & 0xFF00))
            readUnfixed(nextInstruction, target);

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
        adcValue(read(address));
    }

    private void adcValue(byte value)
    {
        if (getFlag(StatusFlags.D))
        {
            adcDecimal(value);
            return;
        }

        var carry = getFlag(StatusFlags.C) ? 1 : 0;
        var sum = registers.A + value + carry;

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
        var carry = getFlag(StatusFlags.C) ? 1 : 0;
        var binarySum = registers.A + value + carry;

        // NMOS 6502 ADC decimal algorithm (Bruce Clark, 6502.org):
        // adjust the low nibble first, then build the high nibble. N and V
        // come from the pre-final-adjust value; Z comes from the binary sum.
        var low = (registers.A & 0x0F) + (value & 0x0F) + carry;

        if (low >= 0x0A)
            low = ((low + 0x06) & 0x0F) + 0x10;

        var intermediate = (registers.A & 0xF0) + (value & 0xF0) + low;
        var adjusted = intermediate;

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
        sbcValue(read(address));
    }

    private void sbcValue(byte value)
    {
        if (getFlag(StatusFlags.D))
        {
            sbcDecimal(value);
            return;
        }

        var borrow = getFlag(StatusFlags.C) ? 0 : 1;
        var difference = registers.A - value - borrow;

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
        var borrow = getFlag(StatusFlags.C) ? 0 : 1;
        var binaryDifference = registers.A - value - borrow;

        // NMOS 6502 SBC decimal algorithm. The accumulator is adjusted BCD;
        // C, N, V and Z are all based on the binary subtraction.
        var low = (registers.A & 0x0F) - (value & 0x0F) - borrow;

        if (low < 0)
            low = ((low - 0x06) & 0x0F) - 0x10;

        var adjusted = (registers.A & 0xF0) - (value & 0xF0) + low;

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
        andValue(read(address));
    }

    private void andValue(byte value)
    {
        registers.A &= value;
        setFlag(StatusFlags.Z, registers.A == 0);
        setFlag(StatusFlags.N, registers.A >= 0x80);
    }

    private void ora(ushort address)
    {
        oraValue(read(address));
    }

    private void oraValue(byte value)
    {
        registers.A |= value;
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

    // Read-modify-write instructions read the value, write the old value
    // back (the NMOS dummy write), then write the result. The dummy write
    // is a real bus cycle that a VIC fetch can stall.
    private byte asl(ushort address)
    {
        var value = read(address);
        var result = aslValue(value);

        write(address, value);
        write(address, result);
        return result;
    }

    private byte aslValue(byte b)
    {
        setFlag(StatusFlags.C, (b & 0x80) != 0);
        b <<= 1;
        setFlag(StatusFlags.Z, b == 0);
        setFlag(StatusFlags.N, b >= 0x80);
        return b;
    }

    private void rol()
    {
        var bit = getFlag(StatusFlags.C);
        setFlag(StatusFlags.C, (registers.A & 0x80) != 0);
        registers.A <<= 1;
        if (bit) registers.A |= 0x01;
        setFlag(StatusFlags.Z, registers.A == 0);
        setFlag(StatusFlags.N, registers.A >= 0x80);
    }

    private byte rol(ushort address)
    {
        var value = read(address);
        var result = rolValue(value);

        write(address, value);
        write(address, result);
        return result;
    }

    private byte rolValue(byte b)
    {
        var bit = getFlag(StatusFlags.C);
        setFlag(StatusFlags.C, (b & 0x80) != 0);
        b <<= 1;
        if (bit) b |= 0x01;
        setFlag(StatusFlags.Z, b == 0);
        setFlag(StatusFlags.N, b >= 0x80);
        return b;
    }

    private void ror()
    {
        var bit = getFlag(StatusFlags.C);
        setFlag(StatusFlags.C, (registers.A & 0x01) != 0);
        registers.A >>= 1;
        if (bit) registers.A |= 0x80;
        setFlag(StatusFlags.Z, registers.A == 0);
        setFlag(StatusFlags.N, registers.A >= 0x80);
    }

    private byte ror(ushort address)
    {
        var value = read(address);
        var result = rorValue(value);

        write(address, value);
        write(address, result);
        return result;
    }

    private byte rorValue(byte b)
    {
        var bit = getFlag(StatusFlags.C);
        setFlag(StatusFlags.C, (b & 0x01) != 0);
        b >>= 1;
        if (bit) b |= 0x80;
        setFlag(StatusFlags.Z, b == 0);
        setFlag(StatusFlags.N, b >= 0x80);
        return b;
    }

    private void lsr()
    {
        setFlag(StatusFlags.C, (registers.A & 0x01) != 0);
        registers.A >>= 1;
        setFlag(StatusFlags.Z, registers.A == 0);
        setFlag(StatusFlags.N, false);
    }

    private byte lsr(ushort address)
    {
        var value = read(address);
        var result = lsrValue(value);

        write(address, value);
        write(address, result);
        return result;
    }

    private byte lsrValue(byte b)
    {
        setFlag(StatusFlags.C, (b & 0x01) != 0);
        b >>= 1;
        setFlag(StatusFlags.Z, b == 0);
        setFlag(StatusFlags.N, false);
        return b;
    }

    private void bit(ushort address)
    {
        var h = read(address);
        setFlag(StatusFlags.N, (h & 0x80) != 0);
        setFlag(StatusFlags.V, (h & 0x40) != 0);
        setFlag(StatusFlags.Z, (h & registers.A) == 0);
    }

    private void cmp(ushort address)
    {
        cmpValue(read(address));
    }

    private void cmpValue(byte value)
    {
        var h = (ushort)(registers.A - value);
        setFlag(StatusFlags.C, h <= 0xFF);
        setFlag(StatusFlags.Z, getLowByte(h) == 0);
        setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
    }

    private void cpx(ushort address)
    {
        var h = (ushort)(registers.X - read(address));
        setFlag(StatusFlags.C, h <= 0xFF);
        setFlag(StatusFlags.Z, getLowByte(h) == 0);
        setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
    }

    private void cpy(ushort address)
    {
        var h = (ushort)(registers.Y - read(address));
        setFlag(StatusFlags.C, h <= 0xFF);
        setFlag(StatusFlags.Z, getLowByte(h) == 0);
        setFlag(StatusFlags.N, getLowByte(h) >= 0x80);
    }

    private byte dec(ushort address)
    {
        var value = read(address);
        var result = decValue(value);

        write(address, value);
        write(address, result);
        return result;
    }

    private byte decValue(byte value)
    {
        var result = (byte)(value - 1);
        setFlag(StatusFlags.Z, result == 0);
        setFlag(StatusFlags.N, result >= 0x80);
        return result;
    }

    private void dex()
    {
        var h = (ushort)(registers.X - 1);
        registers.X = getLowByte(h);
        setFlag(StatusFlags.Z, registers.X == 0);
        setFlag(StatusFlags.N, registers.X >= 0x80);
    }

    private void dey()
    {
        var h = (ushort)(registers.Y - 1);
        registers.Y = getLowByte(h);
        setFlag(StatusFlags.Z, registers.Y == 0);
        setFlag(StatusFlags.N, registers.Y >= 0x80);
    }

    private void eor(ushort address)
    {
        eorValue(read(address));
    }

    private void eorValue(byte value)
    {
        registers.A ^= value;
        setFlag(StatusFlags.Z, registers.A == 0);
        setFlag(StatusFlags.N, registers.A >= 0x80);
    }

    private byte inc(ushort address)
    {
        var value = read(address);
        var result = incValue(value);

        write(address, value);
        write(address, result);
        return result;
    }

    private byte incValue(byte value)
    {
        var result = (byte)(value + 1);
        setFlag(StatusFlags.Z, result == 0);
        setFlag(StatusFlags.N, result >= 0x80);
        return result;
    }

    private void inx()
    {
        var h = (ushort)(registers.X + 1);
        registers.X = getLowByte(h);
        setFlag(StatusFlags.Z, registers.X == 0);
        setFlag(StatusFlags.N, registers.X >= 0x80);
    }

    private void iny()
    {
        var h = (ushort)(registers.Y + 1);
        registers.Y = getLowByte(h);
        setFlag(StatusFlags.Z, registers.Y == 0);
        setFlag(StatusFlags.N, registers.Y >= 0x80);
    }

    private void jmp(ushort address)
    {
        registers.PC = address;
    }

    /// <summary>
    /// JSR reads its low operand, spends an internal cycle, pushes the
    /// return address, then fetches the high operand on its last cycle.
    /// </summary>
    private void jsr()
    {
        var low = read(registers.PC);
        idle();

        var returnAddress = (ushort)(registers.PC + 1);
        stackPush(getHighByte(returnAddress));
        stackPush(getLowByte(returnAddress));

        var high = read((ushort)(registers.PC + 1));
        registers.PC = (ushort)(low | (high << 8));
    }

    private void rts()
    {
        idle();
        idle();

        var low = stackPull();
        var high = stackPull();
        registers.PC = (ushort)((low | (high << 8)) + 1);

        idle();
    }

    private void rti()
    {
        idle();
        idle();

        pullStatus();
        var low = stackPull();
        var high = stackPull();
        registers.PC = (ushort)(low | (high << 8));
    }

    private void lda(ushort address)
    {
        registers.A = read(address);
        setFlag(StatusFlags.Z, registers.A == 0);
        setFlag(StatusFlags.N, registers.A >= 0x80);
    }

    private void ldx(ushort address)
    {
        registers.X = read(address);
        setFlag(StatusFlags.Z, registers.X == 0);
        setFlag(StatusFlags.N, registers.X >= 0x80);
    }

    private void ldy(ushort address)
    {
        registers.Y = read(address);
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
        write(address, registers.A);
    }

    private void stx(ushort address)
    {
        write(address, registers.X);
    }

    private void sty(ushort address)
    {
        write(address, registers.Y);
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
        // BRK reads the padding byte, pushes PC and status, then reads the
        // IRQ vector.
        read(registers.PC);
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

        idle();
        idle();

        stackPush(getHighByte(registers.PC));
        stackPush(getLowByte(registers.PC));
        pushStatus(false);
        setFlag(StatusFlags.I, true);
        registers.PC = peekWord(0xFFFE);
    }

    private void nmi()
    {
        idle();
        idle();

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
        // The machine intercepts a JSR entry, so charge its fetch cycle
        // before completing the call with an RTS.
        idle();
        rts();
        return 6;
    }

    public void Reset()
    {
        jammed = false;
        registers.PC = bus.PeekWord(AddressMap.ResetVector);
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
            return cycles;
        }

        if (interrupts.IsIrqAsserted && !getFlag(StatusFlags.I))
        {
            irq();
            return cycles;
        }

        var instruction = read(registers.PC);
        registers.PC++;
        ushort address = 0;

        switch ((Instructions)instruction)
        {
            // ADC
            case Instructions.ADC_imm: adc(immediate()); break;
            case Instructions.ADC_zp: adc(zeroPage()); break;
            case Instructions.ADC_zpx: adc(zeroPageX()); break;
            case Instructions.ADC_abs: adc(absolute()); break;
            case Instructions.ADC_abx: adc(absoluteX(true)); break;
            case Instructions.ADC_aby: adc(absoluteY(true)); break;
            case Instructions.ADC_izx: adc(indirectX()); break;
            case Instructions.ADC_izy: adc(indirectY(true)); break;

            // SBC
            case Instructions.SBC_imm:
            case Instructions.SBC:
                sbc(immediate()); break;
            case Instructions.SBC_zp: sbc(zeroPage()); break;
            case Instructions.SBC_zpx: sbc(zeroPageX()); break;
            case Instructions.SBC_abs: sbc(absolute()); break;
            case Instructions.SBC_abx: sbc(absoluteX(true)); break;
            case Instructions.SBC_aby: sbc(absoluteY(true)); break;
            case Instructions.SBC_izx: sbc(indirectX()); break;
            case Instructions.SBC_izy: sbc(indirectY(true)); break;

            // AND
            case Instructions.AND_imm: and(immediate()); break;
            case Instructions.AND_zp: and(zeroPage()); break;
            case Instructions.AND_zpx: and(zeroPageX()); break;
            case Instructions.AND_abs: and(absolute()); break;
            case Instructions.AND_abx: and(absoluteX(true)); break;
            case Instructions.AND_aby: and(absoluteY(true)); break;
            case Instructions.AND_izx: and(indirectX()); break;
            case Instructions.AND_izy: and(indirectY(true)); break;

            // ORA
            case Instructions.ORA_imm: ora(immediate()); break;
            case Instructions.ORA_zp: ora(zeroPage()); break;
            case Instructions.ORA_zpx: ora(zeroPageX()); break;
            case Instructions.ORA_abs: ora(absolute()); break;
            case Instructions.ORA_abx: ora(absoluteX(true)); break;
            case Instructions.ORA_aby: ora(absoluteY(true)); break;
            case Instructions.ORA_izx: ora(indirectX()); break;
            case Instructions.ORA_izy: ora(indirectY(true)); break;

            // ASL
            case Instructions.ASL_imp: idle(); asl(); break;
            case Instructions.ASL_zp: asl(zeroPage()); break;
            case Instructions.ASL_zpx: asl(zeroPageX()); break;
            case Instructions.ASL_abs: asl(absolute()); break;
            case Instructions.ASL_abx: asl(absoluteXForWrite()); break;

            // ROL
            case Instructions.ROL_imp: idle(); rol(); break;
            case Instructions.ROL_zp: rol(zeroPage()); break;
            case Instructions.ROL_zpx: rol(zeroPageX()); break;
            case Instructions.ROL_abs: rol(absolute()); break;
            case Instructions.ROL_abx: rol(absoluteXForWrite()); break;

            // ROR
            case Instructions.ROR_imp: idle(); ror(); break;
            case Instructions.ROR_zp: ror(zeroPage()); break;
            case Instructions.ROR_zpx: ror(zeroPageX()); break;
            case Instructions.ROR_abs: ror(absolute()); break;
            case Instructions.ROR_abx: ror(absoluteXForWrite()); break;

            // LSR
            case Instructions.LSR_imp: idle(); lsr(); break;
            case Instructions.LSR_zp: lsr(zeroPage()); break;
            case Instructions.LSR_zpx: lsr(zeroPageX()); break;
            case Instructions.LSR_abs: lsr(absolute()); break;
            case Instructions.LSR_abx: lsr(absoluteXForWrite()); break;

            // BIT
            case Instructions.BIT_zp: bit(zeroPage()); break;
            case Instructions.BIT_abs: bit(absolute()); break;

            // BRK
            case Instructions.BRK: brk(); break;

            // CMP
            case Instructions.CMP_imm: cmp(immediate()); break;
            case Instructions.CMP_zp: cmp(zeroPage()); break;
            case Instructions.CMP_zpx: cmp(zeroPageX()); break;
            case Instructions.CMP_abs: cmp(absolute()); break;
            case Instructions.CMP_abx: cmp(absoluteX(true)); break;
            case Instructions.CMP_aby: cmp(absoluteY(true)); break;
            case Instructions.CMP_izx: cmp(indirectX()); break;
            case Instructions.CMP_izy: cmp(indirectY(true)); break;

            // CPX
            case Instructions.CPX_imm: cpx(immediate()); break;
            case Instructions.CPX_zp: cpx(zeroPage()); break;
            case Instructions.CPX_abs: cpx(absolute()); break;

            // CPY
            case Instructions.CPY_imm: cpy(immediate()); break;
            case Instructions.CPY_zp: cpy(zeroPage()); break;
            case Instructions.CPY_abs: cpy(absolute()); break;

            // DEC
            case Instructions.DEC_zp: dec(zeroPage()); break;
            case Instructions.DEC_zpx: dec(zeroPageX()); break;
            case Instructions.DEC_abs: dec(absolute()); break;
            case Instructions.DEC_abx: dec(absoluteXForWrite()); break;

            // DEX
            case Instructions.DEX: idle(); dex(); break;

            // DEY
            case Instructions.DEY: idle(); dey(); break;

            // EOR
            case Instructions.EOR_imm: eor(immediate()); break;
            case Instructions.EOR_zp: eor(zeroPage()); break;
            case Instructions.EOR_zpx: eor(zeroPageX()); break;
            case Instructions.EOR_abs: eor(absolute()); break;
            case Instructions.EOR_abx: eor(absoluteX(true)); break;
            case Instructions.EOR_aby: eor(absoluteY(true)); break;
            case Instructions.EOR_izx: eor(indirectX()); break;
            case Instructions.EOR_izy: eor(indirectY(true)); break;

            // INC
            case Instructions.INC_zp: inc(zeroPage()); break;
            case Instructions.INC_zpx: inc(zeroPageX()); break;
            case Instructions.INC_abs: inc(absolute()); break;
            case Instructions.INC_abx: inc(absoluteXForWrite()); break;

            // INX
            case Instructions.INX: idle(); inx(); break;

            // INY
            case Instructions.INY: idle(); iny(); break;

            // JMP
            case Instructions.JMP_abs: jmp(absolute()); break;
            case Instructions.JMP_ind: jmp(indirect()); break;

            // JSR
            case Instructions.JSR: jsr(); break;

            // RTS
            case Instructions.RTS: rts(); break;

            // RTI
            case Instructions.RTI: rti(); break;

            // LDA
            case Instructions.LDA_imm: lda(immediate()); break;
            case Instructions.LDA_zp: lda(zeroPage()); break;
            case Instructions.LDA_zpx: lda(zeroPageX()); break;
            case Instructions.LDA_abs: lda(absolute()); break;
            case Instructions.LDA_abx: lda(absoluteX(true)); break;
            case Instructions.LDA_aby: lda(absoluteY(true)); break;
            case Instructions.LDA_izx: lda(indirectX()); break;
            case Instructions.LDA_izy: lda(indirectY(true)); break;

            // LDX
            case Instructions.LDX_imm: ldx(immediate()); break;
            case Instructions.LDX_zp: ldx(zeroPage()); break;
            case Instructions.LDX_zpy: ldx(zeroPageY()); break;
            case Instructions.LDX_abs: ldx(absolute()); break;
            case Instructions.LDX_aby: ldx(absoluteY(true)); break;

            // LDY
            case Instructions.LDY_imm: ldy(immediate()); break;
            case Instructions.LDY_zp: ldy(zeroPage()); break;
            case Instructions.LDY_zpx: ldy(zeroPageX()); break;
            case Instructions.LDY_abs: ldy(absolute()); break;
            case Instructions.LDY_abx: ldy(absoluteX(true)); break;

            // STA
            case Instructions.STA_zp: sta(zeroPage()); break;
            case Instructions.STA_zpx: sta(zeroPageX()); break;
            case Instructions.STA_abs: sta(absolute()); break;
            case Instructions.STA_abx: sta(absoluteXForWrite()); break;
            case Instructions.STA_aby: sta(absoluteYForWrite()); break;
            case Instructions.STA_izx: sta(indirectX()); break;
            case Instructions.STA_izy: sta(indirectYForWrite()); break;

            // STX
            case Instructions.STX_zp: stx(zeroPage()); break;
            case Instructions.STX_zpy: stx(zeroPageY()); break;
            case Instructions.STX_abs: stx(absolute()); break;

            // STY
            case Instructions.STY_zp: sty(zeroPage()); break;
            case Instructions.STY_zpx: sty(zeroPageX()); break;
            case Instructions.STY_abs: sty(absolute()); break;

            // TAX
            case Instructions.TAX: idle(); tax(); break;

            // TAY
            case Instructions.TAY: idle(); tay(); break;

            // TSX
            case Instructions.TSX: idle(); tsx(); break;

            // TXA
            case Instructions.TXA: idle(); txa(); break;

            // TXS
            case Instructions.TXS: idle(); txs(); break;

            // TYA
            case Instructions.TYA: idle(); tya(); break;

            // CLC
            case Instructions.CLC: idle(); setFlag(StatusFlags.C, false); break;

            // CLD
            case Instructions.CLD: idle(); setFlag(StatusFlags.D, false); break;

            // CLI
            case Instructions.CLI: idle(); setFlag(StatusFlags.I, false); break;

            // CLV
            case Instructions.CLV: idle(); setFlag(StatusFlags.V, false); break;

            // BCC
            case Instructions.BCC: bfc(StatusFlags.C); break;

            // BCS
            case Instructions.BCS: bfs(StatusFlags.C); break;

            // BEQ
            case Instructions.BEQ: bfs(StatusFlags.Z); break;

            // BMI
            case Instructions.BMI: bfs(StatusFlags.N); break;

            // BNE
            case Instructions.BNE: bfc(StatusFlags.Z); break;

            // BPL
            case Instructions.BPL: bfc(StatusFlags.N); break;

            // BVC
            case Instructions.BVC: bfc(StatusFlags.V); break;

            // BVS
            case Instructions.BVS: bfs(StatusFlags.V); break;

            // NOP
            case Instructions.NOP:
            case Instructions.NOP01:
            case Instructions.NOP02:
            case Instructions.NOP03:
            case Instructions.NOP04:
            case Instructions.NOP05:
            case Instructions.NOP06:
                idle(); break;

            case Instructions.NOP_imm01:
            case Instructions.NOP_imm02:
            case Instructions.NOP_imm03:
            case Instructions.NOP_imm04:
            case Instructions.NOP_imm05:
                read(immediate()); break;

            case Instructions.NOP_zp01:
            case Instructions.NOP_zp02:
            case Instructions.NOP_zp03:
                read(zeroPage()); break;

            case Instructions.NOP_zpx01:
            case Instructions.NOP_zpx02:
            case Instructions.NOP_zpx03:
            case Instructions.NOP_zpx04:
            case Instructions.NOP_zpx05:
            case Instructions.NOP_zpx06:
                read(zeroPageX()); break;

            case Instructions.NOP_abs: read(absolute()); break;

            case Instructions.NOP_abx01:
            case Instructions.NOP_abx02:
            case Instructions.NOP_abx03:
            case Instructions.NOP_abx04:
            case Instructions.NOP_abx05:
            case Instructions.NOP_abx06:
                read(absoluteX(true)); break;

            // PHA
            case Instructions.PHA: idle(); stackPush(registers.A); break;

            // PHP
            case Instructions.PHP: idle(); stackPush((byte)(registers.P | 0x30)); break;

            // PLA
            case Instructions.PLA: idle(); idle(); pla(); break;

            // PLP
            case Instructions.PLP: idle(); idle(); pullStatus(); break;

            // SEC
            case Instructions.SEC: idle(); setFlag(StatusFlags.C, true); break;

            // SED
            case Instructions.SED: idle(); setFlag(StatusFlags.D, true); break;

            // SEI
            case Instructions.SEI: idle(); setFlag(StatusFlags.I, true); break;

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
                and(immediate());
                setFlag(StatusFlags.C, (registers.A & 0x80) != 0);
                break;

            // ANE
            case Instructions.ANE: idle(); break;

            // ARR
            case Instructions.ARR: and(immediate()); ror(); break;

            // ASR
            case Instructions.ASR: and(immediate()); lsr(); break;

            // DCP
            case Instructions.DCP_zp: address = zeroPage(); cmpValue(dec(address)); break;
            case Instructions.DCP_zpx: address = zeroPageX(); cmpValue(dec(address)); break;
            case Instructions.DCP_abs: address = absolute(); cmpValue(dec(address)); break;
            case Instructions.DCP_abx: address = absoluteXForWrite(); cmpValue(dec(address)); break;
            case Instructions.DCP_aby: address = absoluteYForWrite(); cmpValue(dec(address)); break;
            case Instructions.DCP_izx: address = indirectX(); cmpValue(dec(address)); break;
            case Instructions.DCP_izy: address = indirectYForWrite(); cmpValue(dec(address)); break;

            // ICB
            case Instructions.ISB_zp: address = zeroPage(); sbcValue(inc(address)); break;
            case Instructions.ISB_zpx: address = zeroPageX(); sbcValue(inc(address)); break;
            case Instructions.ISB_abs: address = absolute(); sbcValue(inc(address)); break;
            case Instructions.ISB_abx: address = absoluteXForWrite(); sbcValue(inc(address)); break;
            case Instructions.ISB_aby: address = absoluteYForWrite(); sbcValue(inc(address)); break;
            case Instructions.ISB_izx: address = indirectX(); sbcValue(inc(address)); break;
            case Instructions.ISB_izy: address = indirectYForWrite(); sbcValue(inc(address)); break;

            //LAE
            case Instructions.LAE:
                registers.S &= read(absoluteY(true));
                tsx();
                txa();
                break;

            // LAX
            case Instructions.LAX_zp: lda(zeroPage()); tax(); break;
            case Instructions.LAX_zpy: lda(zeroPageY()); tax(); break;
            case Instructions.LAX_abs: lda(absolute()); tax(); break;
            case Instructions.LAX_aby: lda(absoluteY(true)); tax(); break;
            case Instructions.LAX_izx: lda(indirectX()); tax(); break;
            case Instructions.LAX_izy: lda(indirectY(true)); tax(); break;

            // LXA
            case Instructions.LXA: idle(); break;

            // RLA
            case Instructions.RLA_zp: address = zeroPage(); andValue(rol(address)); break;
            case Instructions.RLA_zpx: address = zeroPageX(); andValue(rol(address)); break;
            case Instructions.RLA_abs: address = absolute(); andValue(rol(address)); break;
            case Instructions.RLA_abx: address = absoluteXForWrite(); andValue(rol(address)); break;
            case Instructions.RLA_aby: address = absoluteYForWrite(); andValue(rol(address)); break;
            case Instructions.RLA_izx: address = indirectX(); andValue(rol(address)); break;
            case Instructions.RLA_izy: address = indirectYForWrite(); andValue(rol(address)); break;

            // RRA
            case Instructions.RRA_zp: address = zeroPage(); adcValue(ror(address)); break;
            case Instructions.RRA_zpx: address = zeroPageX(); adcValue(ror(address)); break;
            case Instructions.RRA_abs: address = absolute(); adcValue(ror(address)); break;
            case Instructions.RRA_abx: address = absoluteXForWrite(); adcValue(ror(address)); break;
            case Instructions.RRA_aby: address = absoluteYForWrite(); adcValue(ror(address)); break;
            case Instructions.RRA_izx: address = indirectX(); adcValue(ror(address)); break;
            case Instructions.RRA_izy: address = indirectYForWrite(); adcValue(ror(address)); break;

            // SAX
            case Instructions.SAX_zp: write(zeroPage(), (byte)(registers.A & registers.X)); break;
            case Instructions.SAX_zpy: write(zeroPageY(), (byte)(registers.A & registers.X)); break;
            case Instructions.SAX_abs: write(absolute(), (byte)(registers.A & registers.X)); break;
            case Instructions.SAX_izx: write(indirectX(), (byte)(registers.A & registers.X)); break;

            // SBX
            case Instructions.SBX: idle(); break;

            // SHA
            case Instructions.SHA_abx: idle(5); break;
            case Instructions.SHA_aby: idle(5); break;

            // SHS
            case Instructions.SHS: idle(5); break;

            // SHX
            case Instructions.SHX: idle(5); break;

            // SHY
            case Instructions.SHY: idle(5); break;

            // SLO
            case Instructions.SLO_zp: address = zeroPage(); oraValue(asl(address)); break;
            case Instructions.SLO_zpx: address = zeroPageX(); oraValue(asl(address)); break;
            case Instructions.SLO_abs: address = absolute(); oraValue(asl(address)); break;
            case Instructions.SLO_abx: address = absoluteXForWrite(); oraValue(asl(address)); break;
            case Instructions.SLO_aby: address = absoluteYForWrite(); oraValue(asl(address)); break;
            case Instructions.SLO_izx: address = indirectX(); oraValue(asl(address)); break;
            case Instructions.SLO_izy: address = indirectYForWrite(); oraValue(asl(address)); break;

            // SRE
            case Instructions.SRE_zp: address = zeroPage(); eorValue(lsr(address)); break;
            case Instructions.SRE_zpx: address = zeroPageX(); eorValue(lsr(address)); break;
            case Instructions.SRE_abs: address = absolute(); eorValue(lsr(address)); break;
            case Instructions.SRE_abx: address = absoluteXForWrite(); eorValue(lsr(address)); break;
            case Instructions.SRE_aby: address = absoluteYForWrite(); eorValue(lsr(address)); break;
            case Instructions.SRE_izx: address = indirectX(); eorValue(lsr(address)); break;
            case Instructions.SRE_izy: address = indirectYForWrite(); eorValue(lsr(address)); break;
        }

        return cycles;
    }

}
