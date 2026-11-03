using System;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class CpuTests : IDisposable
    {
        private readonly TestSystem system;
        private readonly CPU6510 cpu;

        public CpuTests()
        {
            system = new TestSystem();
            cpu = system.Cpu;
            cpu.S = 0xFF;
            cpu.P = 0x20;
        }

        public void Dispose()
        {
            system.Dispose();
        }

        [Fact]
        public void DecimalAdc_09Plus01_Produces10()
        {
            cpu.A = 0x09;
            cpu.P = 0x28; // decimal, no carry

            run(0x69, 0x01);

            Assert.Equal(0x10, cpu.A);
            Assert.False(flag(0x01));
        }

        [Fact]
        public void DecimalAdc_99Plus01_Produces00WithCarry()
        {
            cpu.A = 0x99;
            cpu.P = 0x28;

            run(0x69, 0x01);

            Assert.Equal(0x00, cpu.A);
            Assert.True(flag(0x01));
        }

        [Fact]
        public void DecimalSbc_10Minus01_Produces09()
        {
            cpu.A = 0x10;
            cpu.P = 0x29; // decimal and carry

            run(0xE9, 0x01);

            Assert.Equal(0x09, cpu.A);
            Assert.True(flag(0x01));
        }

        [Fact]
        public void DecimalAdc_79PlusCarry_Produces80WithNmosFlags()
        {
            cpu.A = 0x79;
            cpu.P = 0x29; // decimal and carry

            run(0x69, 0x00);

            Assert.Equal(0x80, cpu.A);
            Assert.True(flag(0x80)); // N
            Assert.True(flag(0x40)); // V
            Assert.False(flag(0x01)); // C
        }

        [Fact]
        public void DecimalAdc_89Plus76_UsesDecimalCarryAndBinaryZ()
        {
            cpu.A = 0x89;
            cpu.P = 0x28;

            run(0x69, 0x76);

            Assert.Equal(0x65, cpu.A);
            Assert.True(flag(0x01)); // C
            Assert.False(flag(0x02)); // Z follows the binary sum
        }

        [Fact]
        public void DecimalSbc_00Minus01_WrapsTo99()
        {
            cpu.A = 0x00;
            cpu.P = 0x29;

            run(0xE9, 0x01);

            Assert.Equal(0x99, cpu.A);
            Assert.False(flag(0x01));
        }

        [Fact]
        public void DecimalSbc_00Minus00WithoutBorrow_Produces99()
        {
            cpu.A = 0x00;
            cpu.P = 0x28;

            run(0xE9, 0x00);

            Assert.Equal(0x99, cpu.A);
            Assert.True(flag(0x80)); // N follows the binary result
            Assert.False(flag(0x02));
            Assert.False(flag(0x01));
        }

        [Fact]
        public void InterruptLine_ServicesIrqAtInstructionBoundary()
        {
            cpu.P = 0x20;
            cpu.SetPC(0x0200);
            write(0x0200, 0xEA); // NOP
            write(0xFFFE, 0x00);
            write(0xFFFF, 0x03);

            system.Interrupts.Set(InterruptSource.Cia1, true);

            int cycles = cpu.Step();

            Assert.Equal(7, cycles);
            Assert.Equal(0x0300, cpu.GetPC());
            Assert.True((cpu.P & 0x04) != 0); // I is set
        }

        [Fact]
        public void AbsoluteXRead_PageCrossCostsOneCycle()
        {
            write(0x1100, 0x42);
            cpu.X = 0x01;

            int cycles = run(0xBD, 0xFF, 0x10); // LDA $10FF,X

            Assert.Equal(0x42, cpu.A);
            Assert.Equal(5, cycles);
        }

        [Fact]
        public void AbsoluteXRead_NoPageCrossCostsBaseCycles()
        {
            write(0x10FE, 0x42);
            cpu.X = 0x00;

            int cycles = run(0xBD, 0xFE, 0x10); // LDA $10FE,X

            Assert.Equal(0x42, cpu.A);
            Assert.Equal(4, cycles);
        }

        [Fact]
        public void AbsoluteXStore_PageCrossDoesNotAddCycle()
        {
            cpu.A = 0x42;
            cpu.X = 0x01;

            int cycles = run(0x9D, 0xFF, 0x10); // STA $10FF,X

            Assert.Equal(0x42, read(0x1100));
            Assert.Equal(5, cycles);
        }

        [Fact]
        public void BranchNotTaken_CostsTwoCycles()
        {
            cpu.P = 0x22; // Z set

            int cycles = run(0xD0, 0x02); // BNE +2

            Assert.Equal(2, cycles);
        }

        [Fact]
        public void BranchTakenSamePage_CostsThreeCycles()
        {
            cpu.P = 0x20; // Z clear

            int cycles = run(0xD0, 0x02); // BNE +2

            Assert.Equal(0x0204, cpu.GetPC());
            Assert.Equal(3, cycles);
        }

        [Fact]
        public void BranchTakenAcrossPage_CostsFourCycles()
        {
            cpu.P = 0x20;
            write(0x02FD, 0xD0);
            write(0x02FE, 0x7F);
            cpu.SetPC(0x02FD);

            int cycles = cpu.Step();

            Assert.Equal(0x037E, cpu.GetPC());
            Assert.Equal(4, cycles);
        }

        [Fact]
        public void IndirectJump_WrapsPointerWithinPage()
        {
            write(0x10FF, 0x34);
            write(0x1000, 0x12);
            write(0x1100, 0xFF); // must not be used

            run(0x6C, 0xFF, 0x10); // JMP ($10FF)

            Assert.Equal(0x1234, cpu.GetPC());
        }

        [Fact]
        public void IndirectX_WrapsPointerInZeroPage()
        {
            write(0x0000, 0x00);
            write(0x0001, 0x20);
            write(0x2000, 0x7E);
            cpu.X = 0x01;

            run(0xA1, 0xFF); // LDA ($FF,X)

            Assert.Equal(0x7E, cpu.A);
        }

        [Fact]
        public void IndirectY_WrapsPointerInZeroPage()
        {
            write(0x00FF, 0x00);
            write(0x0000, 0x20);
            write(0x2010, 0x7E);
            cpu.Y = 0x10;

            run(0xB1, 0xFF); // LDA ($FF),Y

            Assert.Equal(0x7E, cpu.A);
        }

        [Fact]
        public void Php_PushesBreakAndUnusedBits()
        {
            cpu.P = 0x20;

            run(0x08); // PHP

            Assert.Equal(0x30, read((ushort)(0x0100 + cpu.S + 1)));
        }

        [Fact]
        public void OraZeroPage_UsesThreeCycles()
        {
            cpu.A = 0x01;
            write(0x10, 0x02);

            int cycles = run(0x05, 0x10); // ORA $10

            Assert.Equal(0x03, cpu.A);
            Assert.Equal(3, cycles);
        }

        [Fact]
        public void LsrZeroPage_UsesFiveCycles()
        {
            write(0x10, 0x02);

            int cycles = run(0x46, 0x10); // LSR $10

            Assert.Equal(0x01, read(0x10));
            Assert.Equal(5, cycles);
        }

        private int run(params byte[] code)
        {
            const ushort start = 0x0200;

            for (int i = 0; i < code.Length; i++)
                write((ushort)(start + i), code[i]);

            cpu.SetPC(start);
            return cpu.Step();
        }

        private bool flag(byte mask)
        {
            return (cpu.P & mask) != 0;
        }

        private void write(ushort address, byte value)
        {
            system.Memory.Poke(address, value);
        }

        private byte read(ushort address)
        {
            return system.Memory.Peek(address);
        }
    }
}
