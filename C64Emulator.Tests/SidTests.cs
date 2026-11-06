using System;
using System.Linq;
using C64Emulator.Audio;
using Xunit;

namespace C64Emulator.Tests;

public sealed class SidTests
{
    [Fact]
    public void Clock_ProducesSamplesAtTheHostRate()
    {
        Sid sid = new Sid();

        // 98,525 PAL cycles is approximately 100 ms.
        sid.Clock(98_525);

        var buffer = new short[6000];
        var count = sid.ReadSamples(buffer);

        Assert.InRange(count, 4780, 4820);
    }

    [Fact]
    public void GatedSawVoice_ProducesNonSilentAudio()
    {
        Sid sid = new Sid();
        ConfigureSawVoice(sid);

        sid.Clock(98_525);

        var samples = ReadAll(sid);

        Assert.NotEmpty(samples);
        Assert.Contains(samples, sample => sample != 0);
        Assert.True(samples.Max(sample => Math.Abs((int)sample)) > 500);
    }

    [Fact]
    public void ReleasingTheGate_ReturnsTheVoiceToSilence()
    {
        Sid sid = new Sid();
        ConfigureSawVoice(sid);

        sid.Clock(98_525);
        ReadAll(sid);

        sid.SetRegister(0x04, 0x20); // Sawtooth, gate off.
        sid.Clock(98_525);

        var released = ReadAll(sid);

        Assert.NotEmpty(released);
        Assert.All(released.TakeLast(100), sample => Assert.Equal(0, sample));
    }

    [Fact]
    public void SawVoice_RunsAtTheExpectedPitch()
    {
        Sid sid = new Sid();
        ConfigureSawVoice(sid);

        sid.Clock(98_525);
        var samples = ReadAll(sid);

        var crossings = 0;

        for (var i = 101; i < samples.Length; i++)
        {
            if (samples[i - 1] > 0 && samples[i] < 0)
                crossings++;
        }

        // $1000 * 985248 / 2^24 = 240.6 Hz, so 100 ms contains about 24
        // sawtooth resets. Skip the first 2 ms of envelope attack.
        Assert.InRange(crossings, 23, 25);
    }

    [Fact]
    public void HostVolume_ScalesGeneratedAudio()
    {
        Sid full = new Sid();
        ConfigureSawVoice(full);

        Sid half = new Sid();
        ConfigureSawVoice(half);
        half.OutputVolume = 50;

        full.Clock(98_525);
        half.Clock(98_525);

        var fullPeak = ReadAll(full).Max(sample => Math.Abs((int)sample));
        var halfPeak = ReadAll(half).Max(sample => Math.Abs((int)sample));

        Assert.True(fullPeak > 500);

        // The taper is perceptual, so 50% is a quarter of the amplitude.
        Assert.InRange((double)halfPeak / fullPeak, 0.20, 0.30);
    }

    [Fact]
    public void HostVolume_FivePercent_IsFarQuieterThanFull()
    {
        Sid full = new Sid();
        ConfigureSawVoice(full);

        Sid quiet = new Sid();
        ConfigureSawVoice(quiet);
        quiet.OutputVolume = 5;

        full.Clock(98_525);
        quiet.Clock(98_525);

        var fullPeak = ReadAll(full).Max(sample => Math.Abs((int)sample));
        var quietPeak = ReadAll(quiet).Max(sample => Math.Abs((int)sample));

        Assert.True(fullPeak > 500);

        // 5% is 1/400 of the amplitude, about -52 dB: inaudible next to
        // the music rather than merely quieter.
        Assert.InRange((double)quietPeak / fullPeak, 0.0015, 0.004);
    }

    [Fact]
    public void MutingTheOutput_ProducesSilence()
    {
        Sid sid = new Sid();
        ConfigureSawVoice(sid);
        sid.Muted = true;

        sid.Clock(98_525);

        var samples = ReadAll(sid);

        Assert.NotEmpty(samples);
        Assert.All(samples, sample => Assert.Equal(0, sample));
    }

    [Fact]
    public void HostVolume_IsClampedToTheSupportedRange()
    {
        Sid sid = new Sid();

        sid.OutputVolume = 200;
        Assert.Equal(100, sid.OutputVolume);

        sid.OutputVolume = -10;
        Assert.Equal(0, sid.OutputVolume);
    }

    [Fact]
    public void VoiceThreeOscillatorAndEnvelopeRegistersAreReadable()
    {
        Sid sid = new Sid();

        sid.SetRegister(0x0E, 0x00);
        sid.SetRegister(0x0F, 0x40);
        sid.SetRegister(0x12, 0x21); // Sawtooth, gate on.
        sid.SetRegister(0x13, 0x00); // Fast attack/decay.
        sid.SetRegister(0x14, 0xF0); // Full sustain, fast release.

        sid.Clock(20_000);

        Assert.NotEqual(0, sid.GetRegister(0x1B));
        Assert.InRange(sid.GetRegister(0x1C), 245, 255);
    }

    [Fact]
    public void EnvelopeAttack_UsesTheHardwareRateCounterPeriods()
    {
        Sid sid = new Sid();
        sid.SetRegister(0x13, 0x00); // Attack 0: one step per 9 cycles.
        sid.SetRegister(0x14, 0xF0); // Sustain 15.
        sid.SetRegister(0x12, 0x01); // Gate on (voice 3; $D41C reads it).

        sid.Clock(900);

        Assert.InRange(sid.GetRegister(0x1C), 98, 102);

        sid.Clock(1500); // Past 255 * 9 cycles.

        Assert.Equal(0xFF, sid.GetRegister(0x1C));
    }

    [Fact]
    public void EnvelopeRelease_FollowsTheExponentialDivider()
    {
        Sid sid = new Sid();
        sid.SetRegister(0x13, 0x00);
        sid.SetRegister(0x14, 0xF0); // Sustain 15, release 0.
        sid.SetRegister(0x12, 0x01);

        sid.Clock(3000);
        Assert.Equal(0xFF, sid.GetRegister(0x1C));

        sid.SetRegister(0x12, 0x20); // Gate off.

        // A linear 255-step ramp would be finished after 2295 cycles. The
        // real envelope divides through the exponential counter, so it is
        // still well above zero here and reaches zero at about 6.8 ms.
        sid.Clock(3000);
        Assert.InRange(sid.GetRegister(0x1C), 25, 40);

        sid.Clock(5000);
        Assert.Equal(0, sid.GetRegister(0x1C));
    }

    [Fact]
    public void SwitchingModel_KeepsTheRegisterAndEnvelopeState()
    {
        Sid sid = new Sid();
        sid.SetRegister(0x13, 0x00);
        sid.SetRegister(0x14, 0xF0);
        sid.SetRegister(0x12, 0x01);
        sid.Clock(1500);

        var envelope = sid.GetRegister(0x1C);

        sid.Model = SidModel.Mos8580;

        Assert.Equal(SidModel.Mos8580, sid.Model);
        Assert.Equal(envelope, sid.GetRegister(0x1C));
        Assert.Equal(0x01, sid.GetRegister(0x12));
    }

    private static void ConfigureSawVoice(Sid sid)
    {
        sid.SetRegister(0x00, 0x00);
        sid.SetRegister(0x01, 0x10);
        sid.SetRegister(0x05, 0x00); // Fast attack/decay.
        sid.SetRegister(0x06, 0xF0); // Full sustain, fast release.
        sid.SetRegister(0x18, 0x0F); // Maximum volume.
        sid.SetRegister(0x04, 0x21); // Sawtooth, gate on.
    }

    private static short[] ReadAll(Sid sid)
    {
        var buffer = new short[16384];
        var count = sid.ReadSamples(buffer);
        return buffer.AsSpan(0, count).ToArray();
    }
}
