using System;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class CiaTests : IDisposable
    {
        private readonly TestSystem system;

        public CiaTests()
        {
            system = new TestSystem(withPeripherals: true);
        }

        public void Dispose()
        {
            system.Dispose();
        }

        [Fact]
        public void BothTimerUnderflows_SetBothIcrFlags()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x0D, 0x00);
            cia.Poke(0x04, 0x00);
            cia.Poke(0x05, 0x00);
            cia.Poke(0x06, 0x00);
            cia.Poke(0x07, 0x00);
            cia.Poke(0x0E, 0x11);
            cia.Poke(0x0F, 0x11);

            cia.ProcessTimers(1);

            Assert.Equal(0x03, cia.Peek(0x0D));
        }

        [Fact]
        public void MaskedTimerUnderflow_SetsIrqBit()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x0D, 0x81);
            cia.Poke(0x04, 0x00);
            cia.Poke(0x05, 0x00);
            cia.Poke(0x0E, 0x11);

            cia.ProcessTimers(1);

            Assert.Equal(0x81, cia.Peek(0x0D));
            Assert.Equal(0x00, cia.Peek(0x0D));
        }

        [Fact]
        public void MaskedTimerUnderflow_AssertsAndClearsTheIrqLine()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x0D, 0x81);
            cia.Poke(0x04, 0x00);
            cia.Poke(0x05, 0x00);
            cia.Poke(0x0E, 0x11);

            cia.ProcessTimers(1);

            Assert.True(system.Interrupts.IsIrqAsserted);
            Assert.Equal(0x81, cia.Peek(0x0D));
            Assert.False(system.Interrupts.IsIrqAsserted);
        }

        [Fact]
        public void TodRead_LatchesTheClockUntilTenthsIsRead()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x08, 0x09);
            cia.Poke(0x09, 0x59);
            cia.Poke(0x0A, 0x59);
            cia.Poke(0x0B, 0x92); // 12 PM, stops the clock
            cia.Poke(0x08, 0x09); // writing tenths restarts it

            Assert.Equal(0x92, cia.Peek(0x0B));
            cia.ProcessTimers(98500);

            Assert.Equal(0x59, cia.Peek(0x0A));
            Assert.Equal(0x59, cia.Peek(0x09));
            Assert.Equal(0x09, cia.Peek(0x08));

            // The tenths read released the latch, so these now show the
            // post-carry values (1:00:00.0 PM).
            Assert.Equal(0x00, cia.Peek(0x09));
            Assert.Equal(0x00, cia.Peek(0x0A));
            Assert.Equal(0x81, cia.Peek(0x0B));
        }

        [Fact]
        public void TodWriteToHours_StopsClockUntilTenthsIsWritten()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x08, 0x00);
            cia.Poke(0x09, 0x30);
            cia.Poke(0x0A, 0x10);
            cia.Poke(0x0B, 0x91); // 11 PM; hours write halts TOD

            cia.ProcessTimers(98500 * 2);

            Assert.Equal(0x30, cia.Peek(0x09));
            Assert.Equal(0x10, cia.Peek(0x0A));

            cia.Poke(0x08, 0x00); // restart
            cia.ProcessTimers(98500 * 10);

            Assert.Equal(0x31, cia.Peek(0x09));
        }

        [Fact]
        public void Cia2PortA_DrivesInvertedVicBank()
        {
            CIA2 cia = system.Cia2!;
            VIC vic = system.Vic!;

            cia.Poke(0x02, 0x03); // Port A bits 0-1 are outputs.

            cia.Poke(0x00, 0x00);
            Assert.Equal(0xC000, vic.BankAddress);

            cia.Poke(0x00, 0x01);
            Assert.Equal(0x8000, vic.BankAddress);

            cia.Poke(0x00, 0x02);
            Assert.Equal(0x4000, vic.BankAddress);

            cia.Poke(0x00, 0x03);
            Assert.Equal(0x0000, vic.BankAddress);
        }

        [Fact]
        public void TodRegisters_UseBcd()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x08, 0x09);
            cia.Poke(0x09, 0x59);
            cia.Poke(0x0A, 0x59);
            cia.Poke(0x0B, 0x92); // 12 PM

            Assert.Equal(0x09, cia.Peek(0x08));
            Assert.Equal(0x59, cia.Peek(0x09));
            Assert.Equal(0x59, cia.Peek(0x0A));
            Assert.Equal(0x92, cia.Peek(0x0B));
        }

        [Fact]
        public void TodTicksAtTenHertzAndRollsPastMidnight()
        {
            CIA1 cia = system.Cia1!;

            cia.Poke(0x08, 0x09);
            cia.Poke(0x09, 0x59);
            cia.Poke(0x0A, 0x59);
            cia.Poke(0x0B, 0x91); // 11 PM, stops the clock
            cia.Poke(0x08, 0x09); // restart at 23:59:59.9

            cia.ProcessTimers(98500);

            Assert.Equal(0x00, cia.Peek(0x08));
            Assert.Equal(0x00, cia.Peek(0x09));
            Assert.Equal(0x00, cia.Peek(0x0A));
            Assert.Equal(0x12, cia.Peek(0x0B)); // 12 AM
        }

        [Fact]
        public void Cia2TimerUnderflow_AssertsNmiNotIrq()
        {
            CIA2 cia = system.Cia2!;

            cia.Poke(0x0D, 0x81);
            cia.Poke(0x04, 0x00);
            cia.Poke(0x05, 0x00);
            cia.Poke(0x0E, 0x11);

            cia.ProcessTimers(1);

            Assert.True(system.Interrupts.IsNmiAsserted);
            Assert.False(system.Interrupts.IsIrqAsserted);

            Assert.Equal(0x81, cia.Peek(0x0D));
            Assert.False(system.Interrupts.IsNmiAsserted);
        }

        [Fact]
        public void Cia2SharesTimerInterruptBehavior()
        {
            CIA2 cia = system.Cia2!;

            cia.Poke(0x0D, 0x00);
            cia.Poke(0x04, 0x00);
            cia.Poke(0x05, 0x00);
            cia.Poke(0x0E, 0x11);

            cia.ProcessTimers(1);

            Assert.Equal(0x01, cia.Peek(0x0D));
        }
    }
}
