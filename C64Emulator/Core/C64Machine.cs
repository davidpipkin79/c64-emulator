using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using C64Emulator.Audio;
using C64Emulator.Cpu;
using C64Emulator.Devices;
using C64Emulator.Media;
using C64Emulator.Video;

namespace C64Emulator.Core;

/// <summary>
/// Composition root for a complete C64. It owns the chips, the clock, and
/// the emulation thread. The CPU itself is synchronous and has no knowledge
/// of wall-clock pacing or host devices.
/// </summary>
internal sealed class C64Machine : IDisposable
{
    // PAL C64 defaults: 985248 Hz CPU clock, 50.125 frames per second. The
    // active standard is switchable at runtime; see VideoStandard.
    public const int PalClockHz = 985248;
    public const int PalCyclesPerFrame = 19656;

    static C64Machine()
    {
        // The default Windows timer granularity is about 15.6 ms, which
        // rounds a 20 ms frame wait up and makes audio run slow and choppy.
        if (OperatingSystem.IsWindows())
            TimeBeginPeriod(1);
    }

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static extern uint TimeBeginPeriod(uint period);

    private const uint TimerAllAccess = 0x1F0003;
    private const uint CreateWaitableTimerHighResolution = 0x2;
    private const uint Infinite = 0xFFFFFFFF;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWaitableTimerExW(
        IntPtr timerAttributes,
        string? timerName,
        uint flags,
        uint desiredAccess);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetWaitableTimer(
        IntPtr timer,
        ref long dueTime,
        int period,
        IntPtr completionRoutine,
        IntPtr argument,
        bool resume);

    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    private static IntPtr precisionTimer;
    private static bool precisionTimerCreated;

    /// <summary>
    /// Duration of one frame in Stopwatch ticks for the active standard.
    /// Host presentation can use this when VSync is disabled.
    /// </summary>
    public long FrameDurationTicks => frameDurationTicks;

    /// <summary>CPU clock of the active video standard.</summary>
    public int ClockFrequency => Vic.ClockHz;

    /// <summary>Total CPU cycles in one frame of the active standard.</summary>
    public int CyclesPerFrame => Vic.FrameCycles;

    private readonly ManualResetEventSlim pauseGate = new ManualResetEventSlim(true);
    private readonly ManualResetEventSlim pauseAcknowledged = new(false);
    private readonly Lock lifecycleLock = new();
    private readonly CpuBusClock cpuBusClock;
    private int pauseGeneration;
    private int pauseAcknowledgedGeneration;

    private Thread? thread;
    private volatile bool stopRequested;
    private volatile bool paused;
    private volatile bool fastForward;
    private volatile bool renderFrames = true;
    private volatile bool running;
    private int speedPercentage = 100;
    private long frameDurationTicks = VideoTiming.Pal.FrameTicks;
    private int frameReady;
    private int frameCycles;
    private long nextWarpRender;

    private const int RewindCapacity = 60;
    private const int RewindIntervalFrames = 6;
    private readonly Lock rewindLock = new();
    private readonly List<byte[]> rewindStates = [];
    private int rewindIndex = -1;
    private int rewindFrameCounter;
    private bool rewindEnabled = true;

    private readonly Dictionary<int, KernalChannel> kernalChannels = new Dictionary<int, KernalChannel>();
    private int kernalInputChannel;
    private int kernalOutputChannel;
    private TapeWildcardRestore? tapeWildcardRestore;

    // KERNAL keyboard buffer: ten bytes at $0277 with the count at $C6.
    private const ushort KeyboardBufferAddress = 0x0277;
    private const int KeyboardBufferCapacity = 10;

    private readonly Queue<byte> typeAhead = new();
    private readonly Lock typeAheadLock = new();
    private int typeAheadPending;

    public C64Machine(RomSet roms, byte[]? driveRom = null)
    {
        ArgumentNullException.ThrowIfNull(roms);

        Interrupts = new InterruptLine();
        var dataBus = new BusLatch();
        Memory = new Memory(dataBus);
        ColorRam = new ColorRam();
        Vic = new Vic(Memory, ColorRam, Interrupts);
        Sid = new Sid();
        Iec = new IecBus();
        Drive = new DiskDrive(
            Iec,
            driveRom ?? DiskDrive.TryLoadRom(AppContext.BaseDirectory));
        Tape = new TapeDrive();
        Cia1 = new Cia1(Interrupts);
        Cia2 = new Cia2(Interrupts, Vic, Iec);
        Tape.AttachReadLine(Cia1.SetFlag);
        Memory.ProcessorPortWritten = OnProcessorPortWritten;
        Io = new IoController(Vic, Sid, ColorRam, Cia1, Cia2, dataBus);
        Memory.Configure(roms, Io);
        Sid.ClockFrequency = Vic.ClockHz;
        Cia1.TodClockFrequency = Vic.ClockHz;
        Cia2.TodClockFrequency = Vic.ClockHz;
        Drive.SetC64Clock(Vic.ClockHz);
        cpuBusClock = new CpuBusClock(this);
        Cpu = new Cpu6510(
            new ClockedCpuBus(Memory, cpuBusClock),
            Interrupts);
    }

    public Memory Memory { get; }

    public Cpu6510 Cpu { get; }

    public Vic Vic { get; }

    public Sid Sid { get; }

    public DiskDrive Drive { get; }

    public TapeDrive Tape { get; }

    public Cia1 Cia1 { get; }

    public Cia2 Cia2 { get; }

    public IoController Io { get; }

    public ColorRam ColorRam { get; }

    public IecBus Iec { get; }

    public InterruptLine Interrupts { get; }

    public Cartridge? Cartridge => Memory.Cartridge;

    public bool Paused
    {
        get => paused;
        set
        {
            if (paused == value)
                return;

            paused = value;

            if (paused)
            {
                Interlocked.Increment(ref pauseGeneration);
                pauseAcknowledged.Reset();
                pauseGate.Reset();
            }
            else
            {
                pauseGate.Set();
            }
        }
    }

    public bool FastForward
    {
        get => fastForward;
        set
        {
            if (fastForward == value)
                return;

            fastForward = value;
            nextWarpRender = 0;
            Vic.RenderPixels = true;
        }
    }

    /// <summary>
    /// True while the host is holding the warp key. Warp is explicit and
    /// never follows disk or drive activity.
    /// </summary>
    public bool IsWarping => fastForward;

    /// <summary>
    /// False when the host has no display, as in a headless run without a
    /// capture. Raster, DMA, interrupts and sprite collisions still advance,
    /// but pixels are not composed. Frame events still fire, so a capture can
    /// leave this true for its run.
    /// </summary>
    public bool RenderFrames
    {
        get => renderFrames;
        set
        {
            renderFrames = value;

            if (!value)
                Vic.RenderPixels = false;
        }
    }

    /// <summary>
    /// Switches between the instant virtual KERNAL disk and the
    /// experimental cycle-level 1541. Changing it resets the drive and
    /// discards any virtual KERNAL channels.
    /// </summary>
    public bool TrueDriveEmulation
    {
        get => Drive.TrueDriveEmulation;
        set
        {
            if (Drive.TrueDriveEmulation == value)
                return;

            Drive.TrueDriveEmulation = value;
            CloseAllKernalChannels();
            Drive.Reset();
            ResetRewind();
        }
    }

    public int SpeedPercentage => Volatile.Read(ref speedPercentage);

    /// <summary>
    /// Active video system. Switching reconfigures the VIC geometry, the SID
    /// and CIA clocks and the 1541 speed ratio, then resets the machine so
    /// the KERNAL can redetect the frame rate at $02A6.
    /// </summary>
    public VideoStandard VideoStandard
    {
        get => Vic.VideoStandard;
        set => SetVideoStandard(value, reset: true);
    }

    internal void SetVideoStandard(VideoStandard standard, bool reset)
    {
        if (Vic.VideoStandard == standard)
            return;

        var wasPaused = Paused;

        WithPaused(() =>
        {
            Vic.SetVideoStandard(standard);
            Sid.ClockFrequency = Vic.ClockHz;
            Cia1.TodClockFrequency = Vic.ClockHz;
            Cia2.TodClockFrequency = Vic.ClockHz;
            Drive.SetC64Clock(Vic.ClockHz);
            frameDurationTicks = Vic.FrameTicks;
            frameCycles = 0;
        });

        if (!reset)
            return;

        HardReset();

        if (wasPaused)
            Paused = true;
    }

    internal int FrameCycles
    {
        get => frameCycles;
        set => frameCycles = value;
    }

    public bool IsRunning => running;

    public void Start()
    {
        lock (lifecycleLock)
        {
            if (running)
                return;

            stopRequested = false;
            paused = false;
            pauseGate.Set();
            frameCycles = 0;
            ResetRewind();
            Cpu.Reset();
            Drive.Reset();
            Sid.DiscardSamples();

            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "C64 emulation"
            };

            running = true;
            thread.Start();
        }
    }

    public void Stop()
    {
        Thread? activeThread;

        lock (lifecycleLock)
        {
            stopRequested = true;
            paused = false;
            pauseGate.Set();
            activeThread = thread;
        }

        if (activeThread is not null && activeThread != Thread.CurrentThread)
        {
            activeThread.Join();
            Drive.Flush();
        }

        lock (lifecycleLock)
        {
            if (ReferenceEquals(thread, activeThread))
            {
                thread = null;
                running = false;
            }
        }
    }

    public void WaitForExit()
    {
        Thread? activeThread;

        lock (lifecycleLock)
            activeThread = thread;

        if (activeThread is not null && activeThread != Thread.CurrentThread)
            activeThread.Join();
    }

    public bool ConsumeFrameReady()
    {
        return Interlocked.Exchange(ref frameReady, 0) != 0;
    }

    internal void ResetFrameReady()
    {
        Interlocked.Exchange(ref frameReady, 0);
    }

    /// <summary>
    /// Completed frames since power-on. Headless capture uses this instead of
    /// wall-clock pacing, so screenshot timing follows emulated time.
    /// </summary>
    public long FrameCount { get; private set; }

    /// <summary>
    /// Raised on the emulation thread when the VIC completes a frame. Keep
    /// handlers short; the headless frame dumper is the intended subscriber.
    /// </summary>
    public event Action? FrameCompleted;

    /// <summary>
    /// Default quick-save slot. It lives beside the executable so it is
    /// easy to find and does not depend on the launch directory.
    /// </summary>
    public static string DefaultStatePath =>
        Path.Combine(AppContext.BaseDirectory, "savestate.c64state");

    /// <summary>
    /// Pauses the emulation thread and waits until it has actually reached
    /// the pause gate. Safe to call when the machine is already paused.
    /// </summary>
    public void PauseAndWait()
    {
        if (!running)
        {
            Paused = true;
            return;
        }

        if (Thread.CurrentThread == thread)
            return;

        if (!paused)
            Paused = true;

        int generation = Volatile.Read(ref pauseGeneration);
        long deadline = Environment.TickCount64 + 2000;

        while (Volatile.Read(ref pauseAcknowledgedGeneration) != generation)
        {
            if (!running)
                return;

            if (Environment.TickCount64 > deadline)
            {
                throw new TimeoutException(
                    "The emulation thread did not pause in time.");
            }

            Thread.Sleep(1);
        }
    }

    public T WithPaused<T>(Func<T> action)
    {
        var wasPaused = Paused;

        try
        {
            PauseAndWait();
            return action();
        }
        finally
        {
            if (!wasPaused)
                Paused = false;
        }
    }

    public void WithPaused(Action action)
    {
        WithPaused(() =>
        {
            action();
            return true;
        });
    }

    public void SaveStateToFile(string? path = null)
    {
        var target = path ?? DefaultStatePath;
        var directory = Path.GetDirectoryName(target);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        WithPaused(() =>
        {
            byte[] state = MachineState.Capture(this, flushMedia: true);
            File.WriteAllBytes(target, state);
            AddRewindState(state);
        });
    }

    public void LoadStateFromFile(string? path = null)
    {
        byte[] state = File.ReadAllBytes(path ?? DefaultStatePath);

        WithPaused(() =>
        {
            MachineState.Restore(this, state);
            ResetRewind();
            AddRewindState(state);
        });
    }

    public bool RewindOneStep(out string message)
    {
        var wasPaused = Paused;

        try
        {
            PauseAndWait();

            lock (rewindLock)
            {
                if (rewindStates.Count == 0)
                {
                    message = "No rewind history";
                    return false;
                }

                if (rewindIndex > 0)
                    rewindIndex--;

                MachineState.Restore(this, rewindStates[rewindIndex]);

                if (rewindIndex < rewindStates.Count - 1)
                {
                    rewindStates.RemoveRange(
                        rewindIndex + 1,
                        rewindStates.Count - rewindIndex - 1);
                }

                message = rewindIndex == 0
                    ? "Start of rewind buffer"
                    : "Rewound";
                return true;
            }
        }
        finally
        {
            if (!wasPaused)
                Paused = false;
        }
    }

    private void ResetRewind()
    {
        lock (rewindLock)
        {
            rewindStates.Clear();
            rewindIndex = -1;
            rewindFrameCounter = 0;
            rewindEnabled = true;
        }
    }

    private void AddRewindState(byte[] state)
    {
        lock (rewindLock)
        {
            if (rewindIndex >= 0 && rewindIndex < rewindStates.Count - 1)
            {
                rewindStates.RemoveRange(
                    rewindIndex + 1,
                    rewindStates.Count - rewindIndex - 1);
            }

            rewindStates.Add(state);

            if (rewindStates.Count > RewindCapacity)
                rewindStates.RemoveAt(0);

            rewindIndex = rewindStates.Count - 1;
        }
    }

    private void CaptureRewindSnapshot()
    {
        try
        {
            AddRewindState(MachineState.Capture(this, flushMedia: false));
        }
        catch (Exception ex)
        {
            rewindEnabled = false;
            Debug.WriteLine("Rewind capture failed: " + ex.Message);
        }
    }

    /// <summary>
    /// Runs a bounded number of CPU cycles synchronously. Intended for
    /// tests and deterministic boot probes, not for the interactive loop.
    /// </summary>
    public long RunCycles(long cycles)
    {
        long executedTotal = 0;

        while (executedTotal < cycles)
        {
            var executed = StepCpu();

            if (executed == 0 && Cpu.IsJammed)
                break;

            executedTotal += executed;
        }

        return executedTotal;
    }

    public void SoftReset()
    {
        CancelPulseTapeWildcardRestore();
        Cpu.Reset();
        ResetRewind();
    }

    public void HardReset()
    {
        CancelPulseTapeWildcardRestore();
        var restart = running;

        if (restart)
            Stop();

        Cpu.Reset();
        Memory.ResetProcessorPort();
        Tape.ResetPlayback();
        RefreshTapeSense();
        Vic.Reset();
        Cia1.Reset();
        Cia2.Reset();
        Drive.Reset();
        Sid.Reset();
        Interrupts.Clear();
        CloseAllKernalChannels();
        frameCycles = 0;
        ResetRewind();

        if (restart)
            Start();
    }

    public void LoadProgram(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var program = File.ReadAllBytes(path);
        LoadProgramBytes(program, run: true);
    }

    /// <summary>
    /// Starts a media file the way the file menu's Space action does. PRGs are
    /// injected and run, disks and tapes are mounted and the matching BASIC
    /// LOAD/RUN command is typed, and cartridges are mounted with a reset.
    /// Returns the status text the menu displays.
    /// </summary>
    public string AutoStart(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var name = Path.GetFileName(path);

        switch (Path.GetExtension(path).ToLowerInvariant())
        {
            case ".prg":
                LoadProgram(path);
                return "Loaded " + name;

            case ".d64":
                MountDisk(path);
                TypeText("LOAD\"*\",8,1\rRUN\r");
                return "Mounted " + name + ": LOAD\"*\",8,1";

            case ".t64":
                MountTape(path);
                TypeText("LOAD\"*\",1,1\rRUN\r");
                return "Mounted " + name + ": LOAD\"*\",1,1";

            case ".tap":
                MountTape(path);
                TypeText("LOAD\"\",1,1\rRUN\r");
                return "Mounted " + name + ": LOAD\"\",1,1";

            case ".crt":
                MountCartridge(path);
                return "Mounted cartridge " + name;

            default:
                throw new NotSupportedException(
                    "Unsupported media type: " + Path.GetExtension(path));
        }
    }

    /// <summary>
    /// Queues text as if it had been typed on the keyboard. The KERNAL buffer
    /// only holds ten characters, so the queue is drained into it as the
    /// screen editor consumes them. Characters are plain PETSCII: uppercase
    /// ASCII and <c>\r</c> map directly.
    /// </summary>
    public void TypeText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        lock (typeAheadLock)
        {
            foreach (var character in text)
                typeAhead.Enqueue(character <= 0x7E ? (byte)character : (byte)'?');
        }

        Interlocked.Exchange(ref typeAheadPending, 1);
    }

    private void PumpTypeAhead()
    {
        if (Volatile.Read(ref typeAheadPending) == 0)
            return;

        lock (typeAheadLock)
        {
            while (typeAhead.Count > 0)
            {
                var count = (int)Memory.Peek(0xC6);

                if (count >= KeyboardBufferCapacity)
                    break;

                Memory.Poke(
                    (ushort)(KeyboardBufferAddress + count),
                    typeAhead.Dequeue());
                Memory.Poke(0xC6, (byte)(count + 1));
            }

            if (typeAhead.Count == 0)
                Interlocked.Exchange(ref typeAheadPending, 0);
        }
    }

    public void MountDisk(string path)
    {
        Drive.Mount(path);
        ResetRewind();
    }

    public void EjectDisk()
    {
        Drive.Eject();
        ResetRewind();
    }

    public void LoadDiskProgram(string name)
    {
        D64FileInfo file = Drive.FindFile(name) ??
            throw new FileNotFoundException("File not found on disk: " + name);

        var program = Drive.ReadFile(file);
        Memory.Poke(0xBA, 8);
        LoadProgramBytes(program, run: true);
    }

    public void MountTape(string path)
    {
        CancelPulseTapeWildcardRestore();
        Tape.Mount(path);
        Tape.SetMotor(false);
        RefreshTapeSense();
        ResetRewind();
    }

    public void MountTape(TapImage tap, string? path = null)
    {
        CancelPulseTapeWildcardRestore();
        Tape.Mount(tap, path);
        Tape.SetMotor(false);
        RefreshTapeSense();
        ResetRewind();
    }

    public void EjectTape()
    {
        CancelPulseTapeWildcardRestore();
        Tape.Eject();
        RefreshTapeSense();
        ResetRewind();
    }

    /// <summary>
    /// TAP playback presents the datasette's PLAY switch as pressed. The
    /// T64 virtual loader does not use the processor port, so it leaves the
    /// SENSE input released.
    /// </summary>
    internal void RefreshTapeSense()
    {
        Memory.ProcessorPortInput = Tape.UsesPulsePlayback
            ? (byte)0x00
            : (byte)0x10;
    }

    private void OnProcessorPortWritten(byte value)
    {
        // The cassette motor is active-low on port bit 5.
        Tape.SetMotor((value & 0x20) == 0);
    }

    public void MountCartridge(string path)
    {
        Cartridge cartridge = Cartridge.Load(path);

        Memory.AttachCartridge(cartridge);
        HardReset();
    }

    public void EjectCartridge()
    {
        Memory.AttachCartridge(null);
        HardReset();
    }

    public void LoadTapeProgram(string name)
    {
        ITapeFileInfo file = Tape.FindFile(name) ??
            throw new FileNotFoundException("File not found on tape: " + name);

        var program = Tape.CreatePrg(file);
        LoadProgramBytes(program, run: true);
    }

    private void LoadProgramBytes(byte[] program, bool run)
    {
        ResetRewind();

        if (program.Length < 2)
            throw new InvalidDataException("Program file is too short.");

        var address = program[0] | (program[1] << 8);
        var length = program.Length - 2;

        if (address + length > 0x10000)
            throw new InvalidDataException("Program does not fit in 64 KB.");

        for (var i = 0; i < length; i++)
            Memory.Poke((ushort)(address + i), program[i + 2]);

        // Match the KERNAL LOAD result pointers. Programs loaded through
        // the menu never execute the KERNAL's LOAD routine, and some boot
        // loaders read these while relocating or loading the next file.
        var endAddress = address + Math.Max(length - 1, 0);
        Memory.Poke(0xC1, (byte)(address & 0xFF));
        Memory.Poke(0xC2, (byte)(address >> 8));
        Memory.Poke(0xC3, (byte)(endAddress & 0xFF));
        Memory.Poke(0xC4, (byte)(endAddress >> 8));
        Memory.Poke(0xAE, (byte)(endAddress & 0xFF));
        Memory.Poke(0xAF, (byte)(endAddress >> 8));

        if (!run)
            return;

        if (address == 0x0801)
        {
            var end = address + length;
            Memory.Poke(0x2B, 0x01);
            Memory.Poke(0x2C, 0x08);
            Memory.Poke(0x2D, (byte)(end & 0xFF));
            Memory.Poke(0x2E, (byte)((end >> 8) & 0xFF));
            StartBasicProgram();
        }
        else
        {
            Cpu.SetPC((ushort)address);
        }
    }

    /// <summary>
    /// Starts the BASIC program at $0801 using the same sequence a C64
    /// loader uses: JSR $A659 resets the interpreter, then RTS lands at
    /// the $A7AE interpreter loop.
    /// </summary>
    private void StartBasicProgram()
    {
        const ushort resetEntry = 0xA659;
        const ushort interpreter = 0xA7AE;

        // The reset routine returns with RTS, so seed the stack with the
        // interpreter address as its return address.
        Cpu.S = 0xFD;
        Memory.Poke(0x01FE, (byte)((interpreter - 1) & 0xFF));
        Memory.Poke(0x01FF, (byte)((interpreter - 1) >> 8));
        Cpu.SetPC(resetEntry);
    }

    public void SaveProgram(string path, ushort fromAddress, ushort toAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read);
        using BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(fromAddress);

        for (int address = fromAddress; address <= toAddress; address++)
            writer.Write(Memory.Peek((ushort)address));
    }

    public void Dispose()
    {
        Stop();
        pauseGate.Dispose();
    }

    private void UpdateWarpRendering()
    {
        if (!renderFrames)
        {
            Vic.RenderPixels = false;
            return;
        }

        if (!fastForward)
        {
            Vic.RenderPixels = true;
            return;
        }

        var now = Stopwatch.GetTimestamp();

        if (nextWarpRender == 0 || now >= nextWarpRender)
        {
            Vic.RenderPixels = true;
            nextWarpRender = now + Stopwatch.Frequency / 30;
        }
        else
        {
            Vic.RenderPixels = false;
        }
    }

    private int StepCpu()
    {
        PumpTypeAhead();
        cpuBusClock.Begin();

        // The core issues every cycle itself: reads, writes and internal
        // cycles in the order the 6510 produces them. Stalls therefore
        // land on the real access instead of being applied afterwards,
        // and the clock's elapsed count includes the stalled cycles.
        StepCpuCore();
        var cycles = cpuBusClock.Elapsed;

        cpuBusClock.End();

        return cycles;
    }

    private int StepCpuCore()
    {
        if (Cpu.IsJammed || !Memory.IsKernalVisible)
            return Cpu.Step();

        var pc = Cpu.GetPC();
        var interruptPending = InterruptPending();
        var ioCycles = interruptPending ? null : HandleKernalIo(pc);

        if (ioCycles.HasValue)
            return ioCycles.Value;

        if ((pc == 0xFFD5 || pc == 0xF49E) && !interruptPending)
        {
            NormalizePulseTapeWildcard();
            var device = Memory.Peek(0xBA);

            if (
                (device == 1 || device == 8) &&
                !(device == 8 && Drive.HardwareDriveActive) &&
                !(device == 1 && Tape.UsesPulsePlayback))
            {
                return HandleKernalLoad(device);
            }
        }

        var cycles = Cpu.Step();
        RestorePulseTapeWildcardIfReturned();
        return cycles;
    }

    private bool InterruptPending()
    {
        if (Interrupts.IsNmiAsserted)
            return true;

        return Interrupts.IsIrqAsserted && (Cpu.P & 0x04) == 0;
    }

    private int? SaveKernalFile()
    {
        if (Memory.Peek(0xBA) != 8)
            return null;

        if (!Drive.IsMounted)
            return CompleteKernalIo(5); // DEVICE NOT PRESENT

        var name = ReadKernalFilename();

        if (name.Length == 0 || name == "*" || name == "$")
            return CompleteKernalIo(8); // MISSING FILE NAME

        // SAVE receives a zero-page offset in A, with the actual start
        // address stored at that offset, and an end address plus one in X/Y.
        var start = Memory.PeekWord(Cpu.A);
        var end = (ushort)(Cpu.X | (Cpu.Y << 8));

        if (end <= start)
            return CompleteKernalIo(8);

        var length = end - start;
        var program = new byte[length + 2];
        program[0] = (byte)(start & 0xFF);
        program[1] = (byte)(start >> 8);

        for (var i = 0; i < length; i++)
            program[i + 2] = Memory.Peek((ushort)(start + i));

        try
        {
            Drive.SaveFile(name, program);
        }
        catch (InvalidOperationException)
        {
            return CompleteKernalIo(8);
        }
        catch (IOException)
        {
            return CompleteKernalIo(8);
        }

        return CompleteKernalIo(0);
    }

    private int HandleKernalLoad(byte device)
    {
        ushort endAddress;
        byte error;

        try
        {
            error = device == 1
                ? LoadFromTape(out endAddress)
                : LoadFromDisk(out endAddress);
        }
        catch
        {
            error = 4;
            endAddress = 0;
        }

        var cycles = Cpu.ReturnFromSubroutine();

        if (error == 0)
        {
            Cpu.A = 0;
            Cpu.X = (byte)(endAddress & 0xFF);
            Cpu.Y = (byte)(endAddress >> 8);
            Cpu.P = (byte)(Cpu.P & ~0x01);
            Memory.Poke(0x90, 0);
        }
        else
        {
            Cpu.A = error;
            Cpu.X = 0;
            Cpu.Y = 0;
            Cpu.P = (byte)(Cpu.P | 0x01);
            Memory.Poke(0x90, error);
        }

        return cycles;
    }

    private byte LoadFromDisk(out ushort endAddress)
    {
        endAddress = 0;

        if (!Drive.IsMounted)
            return 5; // DEVICE NOT PRESENT

        var name = ReadKernalFilename();

        if (name == "$")
            return 4; // A directory listing is not provided through LOAD here.

        D64FileInfo? file = Drive.FindFile(name);

        if (file is null)
            return 4; // FILE NOT FOUND

        return LoadProgramImage(Drive.ReadFile(file), out endAddress);
    }

    private byte LoadFromTape(out ushort endAddress)
    {
        endAddress = 0;

        if (!Tape.IsMounted)
            return 5; // DEVICE NOT PRESENT

        var name = ReadKernalFilename();

        if (name == "$")
            return 4; // A directory listing is not provided through LOAD here.

        ITapeFileInfo? file = Tape.FindFile(name);

        if (file is null)
            return 4; // FILE NOT FOUND

        return LoadProgramImage(Tape.CreatePrg(file), out endAddress);
    }

    private byte LoadProgramImage(byte[] program, out ushort endAddress)
    {
        endAddress = 0;

        if (program.Length < 2)
            return 4;

        var fileAddress = program[0] | (program[1] << 8);
        var requestedAddress = Cpu.X | (Cpu.Y << 8);
        var useRequestedAddress =
            Memory.Peek(0xB9) == 0 && requestedAddress != 0;
        var address = useRequestedAddress ? requestedAddress : fileAddress;
        var length = program.Length - 2;

        if (address + length > 0x10000)
            return 4;

        if (Cpu.A != 0)
        {
            for (var i = 0; i < length; i++)
            {
                if (Memory.Peek((ushort)(address + i)) != program[i + 2])
                    return 0x1C; // VERIFY ERROR
            }
        }
        else
        {
            for (var i = 0; i < length; i++)
                Memory.Poke((ushort)(address + i), program[i + 2]);
        }

        endAddress = (ushort)(address + Math.Max(length - 1, 0));
        Memory.Poke(0xC1, (byte)(address & 0xFF));
        Memory.Poke(0xC2, (byte)(address >> 8));
        Memory.Poke(0xC3, (byte)(endAddress & 0xFF));
        Memory.Poke(0xC4, (byte)(endAddress >> 8));
        Memory.Poke(0xAE, (byte)(endAddress & 0xFF));
        Memory.Poke(0xAF, (byte)(endAddress >> 8));

        return 0;
    }

    private int? HandleKernalIo(ushort pc)
    {
        // A TAP image is serviced by the real KERNAL tape routines through
        // the CIA FLAG line, not by the virtual fast path.
        if (Tape.UsesPulsePlayback && Memory.Peek(0xBA) == 1)
            return null;

        // With a hardware 1541 attached, the KERNAL must talk to it over
        // the IEC bus instead of using the virtual disk fast path.
        if (Drive.HardwareDriveActive && Memory.Peek(0xBA) == 8)
            return null;

        switch (pc)
        {
            case 0xFFC0: // OPEN
                return TryOpenKernalChannel(out var openError)
                    ? CompleteKernalIo(openError)
                    : null;

            case 0xFFC3: // CLOSE
                if (!kernalChannels.ContainsKey(Cpu.A))
                    return null;

                CloseKernalChannel(Cpu.A);
                return CompleteKernalIo(0);

            case 0xFFC6: // CHKIN
                if (!kernalChannels.ContainsKey(Cpu.X))
                    return null;

                kernalInputChannel = Cpu.X;
                return CompleteKernalIo(0);

            case 0xFFC9: // CHKOUT
                if (!kernalChannels.ContainsKey(Cpu.X))
                    return null;

                kernalOutputChannel = Cpu.X;
                return CompleteKernalIo(0);

            case 0xFFCC: // CLRCH
                if (kernalChannels.Count == 0)
                    return null;

                kernalInputChannel = 0;
                kernalOutputChannel = 0;
                return CompleteKernalIo(0);

            case 0xFFCF: // CHRIN
                return ReadKernalChannel();

            case 0xFFD2: // CHROUT
                return WriteKernalChannel();

            case 0xFFD8: // SAVE
                return SaveKernalFile();

            case 0xFFE7: // CLALL
                if (kernalChannels.Count == 0)
                    return null;

                CloseAllKernalChannels();
                return CompleteKernalIo(0);

            default:
                return null;
        }
    }

    private bool TryOpenKernalChannel(out byte error)
    {
        error = 0;

        var device = Memory.Peek(0xBA);

        if (device != 1 && device != 8)
            return false; // Let the KERNAL handle screen, printer, etc.

        if (device == 1 && Tape.UsesPulsePlayback)
            return false;

        int logical = Memory.Peek(0xB8);

        if (logical == 0)
        {
            error = 1;
            return true;
        }

        if ((device == 8 && !Drive.IsMounted) || (device == 1 && !Tape.IsMounted))
        {
            error = 5; // DEVICE NOT PRESENT
            return true;
        }

        // Logical file 15 with secondary address 15 is the drive command
        // and status channel, not a named file. Games use it to poll the
        // error status after LOAD and before issuing drive commands.
        if (device == 8 && logical == 15 && Memory.Peek(0xB9) == 15)
        {
            var status = Encoding.ASCII.GetBytes("00, OK,00,00\r");

            CloseKernalChannel(15);
            kernalChannels[15] = new KernalChannel(status);
            Memory.Poke(0x90, 0);
            return true;
        }

        var name = ReadKernalFilename();
        byte[] data;

        if (name == "$")
        {
            data = BuildDirectory(device);
        }
        else if (device == 8)
        {
            D64FileInfo? file = Drive.FindFile(name);

            if (file is null)
            {
                error = 4; // FILE NOT FOUND
                return true;
            }

            data = Drive.ReadFile(file);
        }
        else
        {
            ITapeFileInfo? file = Tape.FindFile(name);

            if (file is null)
            {
                error = 4;
                return true;
            }

            data = Tape.ReadFile(file);
        }

        CloseKernalChannel(logical);
        kernalChannels[logical] = new KernalChannel(data);
        Memory.Poke(0x90, 0);
        return true;
    }

    private int? ReadKernalChannel()
    {
        if (kernalInputChannel == 0)
            return null;

        if (!kernalChannels.TryGetValue(kernalInputChannel, out KernalChannel? channel))
            return null;

        var cycles = Cpu.ReturnFromSubroutine();
        Cpu.P = (byte)(Cpu.P & ~0x01);

        if (channel.Position >= channel.Data.Length)
        {
            Cpu.A = 0;
            SetKernalStatus(0x40);
            return cycles;
        }

        Cpu.A = channel.Data[channel.Position++];

        if (channel.Position >= channel.Data.Length)
            SetKernalStatus(0x40);

        return cycles;
    }

    private int? WriteKernalChannel()
    {
        if (kernalOutputChannel == 0)
            return null;

        if (!kernalChannels.TryGetValue(kernalOutputChannel, out KernalChannel? channel))
            return null;

        channel.Output.Add(Cpu.A);
        Cpu.P = (byte)(Cpu.P & ~0x01);
        return Cpu.ReturnFromSubroutine();
    }

    private int CompleteKernalIo(byte error)
    {
        var cycles = Cpu.ReturnFromSubroutine();

        if (error == 0)
        {
            Cpu.A = 0;
            Cpu.P = (byte)(Cpu.P & ~0x01);
            Memory.Poke(0x90, 0);
        }
        else
        {
            Cpu.A = error;
            Cpu.P = (byte)(Cpu.P | 0x01);
            Memory.Poke(0x90, error);
        }

        return cycles;
    }

    private void CloseKernalChannel(int logical)
    {
        kernalChannels.Remove(logical);

        if (kernalInputChannel == logical)
            kernalInputChannel = 0;

        if (kernalOutputChannel == logical)
            kernalOutputChannel = 0;
    }

    private void CloseAllKernalChannels()
    {
        kernalChannels.Clear();
        kernalInputChannel = 0;
        kernalOutputChannel = 0;
    }

    private void SetKernalStatus(byte bits)
    {
        Memory.Poke(0x90, (byte)(Memory.Peek(0x90) | bits));
    }

    private byte[] BuildDirectory(byte device)
    {
        StringBuilder builder = new StringBuilder();

        if (device == 8)
        {
            if (Drive.DiskName.Length > 0)
                builder.Append(Drive.DiskName).Append('\r');

            foreach (D64FileInfo file in Drive.Directory)
            {
                builder
                    .Append(file.Name)
                    .Append(' ')
                    .Append(FileTypeName(file.Type))
                    .Append('\r');
            }
        }
        else
        {
            if (Tape.TapeName.Length > 0)
                builder.Append(Tape.TapeName).Append('\r');

            foreach (ITapeFileInfo file in Tape.Directory)
            {
                builder
                    .Append(file.Name)
                    .Append(' ')
                    .Append(FileTypeName(file.Type))
                    .Append('\r');
            }
        }

        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string FileTypeName(CbmFileType type)
    {
        return type switch
        {
            CbmFileType.Sequential => "SEQ",
            CbmFileType.Program => "PRG",
            CbmFileType.User => "USR",
            CbmFileType.Relative => "REL",
            _ => "DEL"
        };
    }

    private string ReadKernalFilename()
    {
        var length = Math.Min((int)Memory.Peek(0xB7), 16);

        if (length == 0)
            return "*";

        var pointer = Memory.PeekWord(0xBB);
        var characters = new char[length];

        for (var i = 0; i < length; i++)
        {
            characters[i] = Petscii.ToAscii(Memory.Peek((ushort)(pointer + i)));
        }

        return new string(characters).Trim();
    }

    /// <summary>
    /// The stock KERNAL compares tape filenames literally, so LOAD"*" never
    /// matches a header. Treat the common TAP form as an unnamed load of the
    /// next header, while leaving disk and virtual T64 loads unchanged.
    /// </summary>
    private void NormalizePulseTapeWildcard()
    {
        if (
            !Tape.UsesPulsePlayback ||
            Memory.Peek(0xBA) != 1 ||
            Memory.Peek(0xB7) != 1)
        {
            return;
        }

        var pointer = Memory.PeekWord(0xBB);

        if (Memory.Peek(pointer) != (byte)'*')
            return;

        var stack = Cpu.S;
        tapeWildcardRestore = new TapeWildcardRestore(
            Memory.Peek(0xB7),
            stack,
            (ushort)(Memory.PeekWord((ushort)(0x0100 + stack + 1)) + 1));
        Memory.Poke(0xB7, 0);
    }

    private void RestorePulseTapeWildcardIfReturned()
    {
        if (tapeWildcardRestore is not { } restore)
            return;

        if (
            Cpu.GetPC() != restore.ReturnAddress ||
            Cpu.S != (byte)(restore.Stack + 2))
        {
            return;
        }

        Memory.Poke(0xB7, restore.FilenameLength);
        tapeWildcardRestore = null;
    }

    private void CancelPulseTapeWildcardRestore()
    {
        if (tapeWildcardRestore is not { } restore)
            return;

        Memory.Poke(0xB7, restore.FilenameLength);
        tapeWildcardRestore = null;
    }

    private sealed record TapeWildcardRestore(
        byte FilenameLength,
        byte Stack,
        ushort ReturnAddress);

    private sealed class KernalChannel(byte[] data)
    {
        public byte[] Data { get; } = data;

        public int Position { get; set; }

        public List<byte> Output { get; } = [];
    }

    internal void WriteKernalState(BinaryWriter writer)
    {
        writer.Write(kernalInputChannel);
        writer.Write(kernalOutputChannel);
        writer.Write(kernalChannels.Count);

        foreach (KeyValuePair<int, KernalChannel> pair in kernalChannels)
        {
            KernalChannel channel = pair.Value;
            writer.Write(pair.Key);
            writer.Write(channel.Data.Length);
            writer.Write(channel.Data);
            writer.Write(channel.Position);
            writer.Write(channel.Output.Count);

            foreach (var value in channel.Output)
                writer.Write(value);
        }

        lock (typeAheadLock)
        {
            writer.Write(typeAhead.Count);

            foreach (var value in typeAhead)
                writer.Write(value);
        }
    }

    internal void ReadKernalState(BinaryReader reader)
    {
        CloseAllKernalChannels();

        kernalInputChannel = reader.ReadInt32();
        kernalOutputChannel = reader.ReadInt32();
        int channelCount = reader.ReadInt32();

        if (channelCount < 0 || channelCount > 256)
            throw new InvalidDataException("Invalid channel count in state.");

        for (var i = 0; i < channelCount; i++)
        {
            int logical = reader.ReadInt32();
            int length = reader.ReadInt32();

            if (length < 0 || length > 16_000_000)
                throw new InvalidDataException("Invalid channel data in state.");

            var data = reader.ReadBytes(length);

            if (data.Length != length)
                throw new EndOfStreamException();

            KernalChannel channel = new(data)
            {
                Position = reader.ReadInt32()
            };

            int outputCount = reader.ReadInt32();

            if (outputCount < 0 || outputCount > 16_000_000)
                throw new InvalidDataException("Invalid channel output in state.");

            for (var j = 0; j < outputCount; j++)
                channel.Output.Add(reader.ReadByte());

            kernalChannels[logical] = channel;
        }

        lock (typeAheadLock)
        {
            typeAhead.Clear();
            int count = reader.ReadInt32();

            if (count < 0 || count > 16_000_000)
                throw new InvalidDataException("Invalid type-ahead state.");

            for (var i = 0; i < count; i++)
                typeAhead.Enqueue(reader.ReadByte());

            Interlocked.Exchange(
                ref typeAheadPending,
                typeAhead.Count > 0 ? 1 : 0);
        }
    }

    private void Run()
    {
        var last = Stopwatch.GetTimestamp();
        var target = last + frameDurationTicks;

        while (!stopRequested)
        {
            if (paused)
            {
                Volatile.Write(
                    ref pauseAcknowledgedGeneration,
                    Volatile.Read(ref pauseGeneration));
                pauseAcknowledged.Set();
                pauseGate.Wait();
                pauseAcknowledged.Reset();
                last = Stopwatch.GetTimestamp();
                target = last + frameDurationTicks;
                continue;
            }

            var busCycles = StepCpu();

            if (busCycles == 0 && Cpu.IsJammed)
            {
                Paused = true;
                continue;
            }

            frameCycles += busCycles;

            if (frameCycles < CyclesPerFrame)
                continue;

            if (!IsWarping)
                WaitUntil(target);

            var now = Stopwatch.GetTimestamp();
            var measured = (int)Math.Round(
                frameDurationTicks * 100.0 / Math.Max(now - last, 1));

            Volatile.Write(ref speedPercentage, measured);
            last = now;

            if (IsWarping)
            {
                // Warp is unpaced; do not accumulate a deadline that
                // would have to be waited out when the key is released.
                target = now + frameDurationTicks;
            }
            else
            {
                // Advance by exactly one frame. Rebasing on the current
                // time after every wake-up turned each late wake-up into
                // a permanently slower cadence, which made PAL music play
                // flat and slow.
                target += frameDurationTicks;

                // A long host stall (debugger, sleep) must not produce a
                // burst of catch-up frames.
                if (now - target > frameDurationTicks)
                    target = now + frameDurationTicks;
            }

            frameCycles -= CyclesPerFrame;

            if (
                !IsWarping &&
                rewindEnabled &&
                ++rewindFrameCounter >= RewindIntervalFrames)
            {
                rewindFrameCounter = 0;
                CaptureRewindSnapshot();
            }
        }

        Volatile.Write(ref speedPercentage, 100);
    }

    private void TickBusCycle()
    {
        // The SID advances on the same cycle as the access that can change
        // its registers, so a write only affects the cycles after it
        // instead of the whole instruction.
        Sid.Clock(1);

        Cia1.ProcessTimers(1);
        Cia2.ProcessTimers(1);
        Tape.Tick(1);

        // Warp only changes host pacing. Keep the VIC and SID running so
        // the display and audio remain live while the CPU runs faster.
        if (Vic.Tick(1))
        {
            Interlocked.Exchange(ref frameReady, 1);
            FrameCount++;
            FrameCompleted?.Invoke();
            UpdateWarpRendering();
        }

        if (Drive.HardwareDriveActive)
            Drive.Tick(1);
    }

    /// <summary>
    /// Applies VIC bus contention at the CPU's individual read and write
    /// accesses. Stalls advance the whole machine one cycle at a time, so
    /// the VIC can finish its fetch and release BA/AEC.
    /// </summary>
    private sealed class CpuBusClock(C64Machine machine) : ICpuBusClock
    {
        private readonly C64Machine machine = machine;
        private bool active;

        /// <summary>
        /// Machine cycles this instruction consumed, including the cycles
        /// the VIC stole before an access was allowed.
        /// </summary>
        public int Elapsed { get; private set; }

        public void Begin()
        {
            Elapsed = 0;
            active = true;
        }

        public void End()
        {
            active = false;
        }

        public void Idle()
        {
            if (!active)
                return;

            Tick();
        }

        public void WaitForAccess(bool isWrite)
        {
            if (!active)
                return;

            while (!machine.Vic.IsCpuAccessAllowed(isWrite))
                Tick();
        }

        public void CompleteAccess()
        {
            if (!active)
                return;

            Tick();
        }

        private void Tick()
        {
            machine.TickBusCycle();
            Elapsed++;
        }
    }

    private static void WaitUntil(long target)
    {
        while (true)
        {
            var remaining = target - Stopwatch.GetTimestamp();

            if (remaining <= 0)
                return;

            if (OperatingSystem.IsWindows())
            {
                var timer = GetPrecisionTimer();

                if (timer != IntPtr.Zero)
                {
                    var due = -Math.Max(
                        1,
                        remaining * 10_000_000 / Stopwatch.Frequency);

                    if (
                        SetWaitableTimer(timer, ref due, 0, IntPtr.Zero, IntPtr.Zero, false) &&
                        WaitForSingleObject(timer, Infinite) == 0)
                    {
                        continue;
                    }
                }
            }

            Thread.SpinWait(50);
        }
    }

    private static IntPtr GetPrecisionTimer()
    {
        if (!precisionTimerCreated)
        {
            precisionTimerCreated = true;
            precisionTimer = CreateWaitableTimerExW(
                IntPtr.Zero,
                null,
                CreateWaitableTimerHighResolution,
                TimerAllAccess);
        }

        return precisionTimer;
    }
}
