using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace C64Emulator
{
    /// <summary>
    /// Composition root for a complete C64. It owns the chips, the clock, and
    /// the emulation thread. The CPU itself is synchronous and has no knowledge
    /// of wall-clock pacing or host devices.
    /// </summary>
    internal sealed class C64Machine : IDisposable
    {
        // PAL C64: 985248 Hz CPU clock, 50.125 frames per second. A frame is
        // 19.95 ms, not the 20 ms a 50 Hz approximation would give.
        public const int ClockHz = 985248;
        public const int CyclesPerFrame = 19656;

        private static readonly long FrameTicks =
            Stopwatch.Frequency * 1995 / 100_000;

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
        /// Duration of one PAL frame in Stopwatch ticks. Host presentation can
        /// use this when VSync is disabled.
        /// </summary>
        public static long FrameDurationTicks => FrameTicks;

        private readonly ManualResetEventSlim pauseGate = new ManualResetEventSlim(true);
        private readonly object lifecycleLock = new object();
        private readonly CpuBusClock cpuBusClock;

        private Thread? thread;
        private volatile bool stopRequested;
        private volatile bool paused;
        private volatile bool fastForward;
        private volatile bool running;
        private int speedPercentage = 100;
        private int frameReady;
        private long nextWarpRender;
        private readonly Dictionary<int, KernalChannel> kernalChannels = new Dictionary<int, KernalChannel>();
        private int kernalInputChannel;
        private int kernalOutputChannel;

        public C64Machine(RomSet roms, byte[]? driveRom = null)
        {
            ArgumentNullException.ThrowIfNull(roms);

            Interrupts = new InterruptLine();
            Memory = new Memory();
            ColorRam = new ColorRam();
            Vic = new VIC(Memory, ColorRam, Interrupts);
            Sid = new SID();
            Iec = new IecBus();
            Drive = new C1541Drive(
                Iec,
                driveRom ?? C1541Drive.TryLoadRom(AppContext.BaseDirectory));
            Tape = new TapeDrive();
            Cia1 = new CIA1(Interrupts);
            Cia2 = new CIA2(Interrupts, Vic, Iec);
            Io = new IO(Vic, Sid, ColorRam, Cia1, Cia2);
            Memory.Configure(roms, Io);
            cpuBusClock = new CpuBusClock(this);
            Cpu = new CPU6510(
                new ClockedCpuBus(Memory, cpuBusClock),
                Interrupts);
        }

        public Memory Memory { get; }

        public CPU6510 Cpu { get; }

        public VIC Vic { get; }

        public SID Sid { get; }

        public C1541Drive Drive { get; }

        public TapeDrive Tape { get; }

        public CIA1 Cia1 { get; }

        public CIA2 Cia2 { get; }

        public IO Io { get; }

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
                    pauseGate.Reset();
                else
                    pauseGate.Set();
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
            }
        }

        public int SpeedPercentage => Volatile.Read(ref speedPercentage);

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

        /// <summary>
        /// Runs a bounded number of CPU cycles synchronously. Intended for
        /// tests and deterministic boot probes, not for the interactive loop.
        /// </summary>
        public void RunCycles(int cycles)
        {
            int executedTotal = 0;

            while (executedTotal < cycles)
            {
                int executed = StepCpu();

                if (executed == 0 && Cpu.IsJammed)
                    break;

                executedTotal += executed;
            }
        }

        public void SoftReset()
        {
            Cpu.Reset();
        }

        public void HardReset()
        {
            bool restart = running;

            if (restart)
                Stop();

            Cpu.Reset();
            Memory.ResetProcessorPort();
            Vic.Reset();
            Cia1.Reset();
            Cia2.Reset();
            Drive.Reset();
            Sid.Reset();
            Interrupts.Clear();
            CloseAllKernalChannels();

            if (restart)
                Start();
        }

        public void LoadProgram(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            byte[] program = File.ReadAllBytes(path);
            LoadProgramBytes(program, run: true);
        }

        public void MountDisk(string path)
        {
            Drive.Mount(path);
        }

        public void EjectDisk()
        {
            Drive.Eject();
        }

        public void LoadDiskProgram(string name)
        {
            D64FileInfo file = Drive.FindFile(name) ??
                throw new FileNotFoundException("File not found on disk: " + name);

            byte[] program = Drive.ReadFile(file);
            Memory.Poke(0xBA, 8);
            LoadProgramBytes(program, run: true);
        }

        public void MountTape(string path)
        {
            Tape.Mount(path);
        }

        public void EjectTape()
        {
            Tape.Eject();
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
            T64FileInfo file = Tape.FindFile(name) ??
                throw new FileNotFoundException("File not found on tape: " + name);

            byte[] program = Tape.CreatePrg(file);
            LoadProgramBytes(program, run: true);
        }

        private void LoadProgramBytes(byte[] program, bool run)
        {
            if (program.Length < 2)
                throw new InvalidDataException("Program file is too short.");

            int address = program[0] | (program[1] << 8);
            int length = program.Length - 2;

            if (address + length > 0x10000)
                throw new InvalidDataException("Program does not fit in 64 KB.");

            for (int i = 0; i < length; i++)
                Memory.Poke((ushort)(address + i), program[i + 2]);

            // Match the KERNAL LOAD result pointers. Programs loaded through
            // the menu never execute the KERNAL's LOAD routine, and some boot
            // loaders read these while relocating or loading the next file.
            int endAddress = address + Math.Max(length - 1, 0);
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
                int end = address + length;
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
            if (!fastForward)
            {
                Vic.RenderPixels = true;
                return;
            }

            long now = Stopwatch.GetTimestamp();

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
            cpuBusClock.Begin();

            // The core issues every cycle itself: reads, writes and internal
            // cycles in the order the 6510 produces them. Stalls therefore
            // land on the real access instead of being applied afterwards,
            // and the clock's elapsed count includes the stalled cycles.
            StepCpuCore();
            int cycles = cpuBusClock.Elapsed;

            cpuBusClock.End();

            return cycles;
        }

        private int StepCpuCore()
        {
            if (Cpu.IsJammed || !Memory.IsKernalVisible)
                return Cpu.Step();

            ushort pc = Cpu.GetPC();
            bool interruptPending = InterruptPending();
            int? ioCycles = interruptPending ? null : HandleKernalIo(pc);

            if (ioCycles.HasValue)
                return ioCycles.Value;

            if ((pc == 0xFFD5 || pc == 0xF49E) && !interruptPending)
            {
                byte device = Memory.Peek(0xBA);

                if (
                    (device == 1 || device == 8) &&
                    !(device == 8 && Drive.HardwareDriveActive))
                {
                    return HandleKernalLoad(device);
                }
            }

            return Cpu.Step();
        }

        private bool InterruptPending()
        {
            if (Interrupts.IsNmiAsserted)
                return true;

            return Interrupts.IsIrqAsserted && (Cpu.P & 0x04) == 0;
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

            int cycles = Cpu.ReturnFromSubroutine();

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

            string name = ReadKernalFilename();

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

            string name = ReadKernalFilename();

            if (name == "$")
                return 4; // A directory listing is not provided through LOAD here.

            T64FileInfo? file = Tape.FindFile(name);

            if (file is null)
                return 4; // FILE NOT FOUND

            return LoadProgramImage(Tape.CreatePrg(file), out endAddress);
        }

        private byte LoadProgramImage(byte[] program, out ushort endAddress)
        {
            endAddress = 0;

            if (program.Length < 2)
                return 4;

            int fileAddress = program[0] | (program[1] << 8);
            int requestedAddress = Cpu.X | (Cpu.Y << 8);
            bool useRequestedAddress =
                Memory.Peek(0xB9) == 0 && requestedAddress != 0;
            int address = useRequestedAddress ? requestedAddress : fileAddress;
            int length = program.Length - 2;

            if (address + length > 0x10000)
                return 4;

            if (Cpu.A != 0)
            {
                for (int i = 0; i < length; i++)
                {
                    if (Memory.Peek((ushort)(address + i)) != program[i + 2])
                        return 0x1C; // VERIFY ERROR
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
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
            // With a hardware 1541 attached, the KERNAL must talk to it over
            // the IEC bus instead of using the virtual disk fast path.
            if (Drive.HardwareDriveActive && Memory.Peek(0xBA) == 8)
                return null;

            switch (pc)
            {
                case 0xFFC0: // OPEN
                    return TryOpenKernalChannel(out byte openError)
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

            byte device = Memory.Peek(0xBA);

            if (device != 1 && device != 8)
                return false; // Let the KERNAL handle screen, printer, etc.

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
                byte[] status = Encoding.ASCII.GetBytes("00, OK,00,00\r");

                CloseKernalChannel(15);
                kernalChannels[15] = new KernalChannel(status);
                Memory.Poke(0x90, 0);
                return true;
            }

            string name = ReadKernalFilename();
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
                T64FileInfo? file = Tape.FindFile(name);

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

            int cycles = Cpu.ReturnFromSubroutine();
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
            int cycles = Cpu.ReturnFromSubroutine();

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

                foreach (T64FileInfo file in Tape.Directory)
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
            int length = Math.Min((int)Memory.Peek(0xB7), 16);

            if (length == 0)
                return "*";

            ushort pointer = Memory.PeekWord(0xBB);
            char[] characters = new char[length];

            for (int i = 0; i < length; i++)
            {
                characters[i] = Petscii.ToAscii(Memory.Peek((ushort)(pointer + i)));
            }

            return new string(characters).Trim();
        }

        private sealed class KernalChannel
        {
            public KernalChannel(byte[] data)
            {
                Data = data;
            }

            public byte[] Data { get; }

            public int Position { get; set; }

            public List<byte> Output { get; } = new List<byte>();
        }

        private void Run()
        {
            long last = Stopwatch.GetTimestamp();
            long target = last + FrameTicks;
            int frameCycles = 0;

            while (!stopRequested)
            {
                if (paused)
                {
                    pauseGate.Wait();
                    last = Stopwatch.GetTimestamp();
                    target = last + FrameTicks;
                    continue;
                }

                int busCycles = StepCpu();

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

                long now = Stopwatch.GetTimestamp();
                int measured = (int)Math.Round(
                    FrameTicks * 100.0 / Math.Max(now - last, 1));

                Volatile.Write(ref speedPercentage, measured);
                last = now;
                target = now + FrameTicks;
                frameCycles -= CyclesPerFrame;
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

            // Warp only changes host pacing. Keep the VIC and SID running so
            // the display and audio remain live while the CPU runs faster.
            if (Vic.Tick(1))
            {
                Interlocked.Exchange(ref frameReady, 1);
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
        private sealed class CpuBusClock : ICpuBusClock
        {
            private readonly C64Machine machine;
            private bool active;

            public CpuBusClock(C64Machine machine)
            {
                this.machine = machine;
            }

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
                long remaining = target - Stopwatch.GetTimestamp();

                if (remaining <= 0)
                    return;

                if (OperatingSystem.IsWindows())
                {
                    IntPtr timer = GetPrecisionTimer();

                    if (timer != IntPtr.Zero)
                    {
                        long due = -Math.Max(
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
}
