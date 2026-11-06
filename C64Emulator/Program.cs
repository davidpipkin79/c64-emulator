using System;
using C64Emulator.Host;

namespace C64Emulator;

internal static class Program
{
    /// <summary>
    /// The main entry point. With no arguments it opens the window; any
    /// command-line arguments select a headless run so a media path can be
    /// started deterministically from a script.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0)
            ConsoleBridge.AttachToParent();

        CommandLineOptions options;

        try
        {
            options = CommandLineOptions.Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine("c64: " + ex.Message);
            Console.Error.WriteLine();
            Console.Error.WriteLine(CommandLineOptions.Usage);
            return ExitCodes.Usage;
        }

        if (options.ShowHelp)
        {
            Console.Out.WriteLine(CommandLineOptions.Usage);
            Console.Out.Flush();
            return ExitCodes.Success;
        }

        var suitePath = options.SuitePath;

        if (suitePath is not null)
        {
            var code = options.ListCases
                ? RegressionSuite.ListCases(
                    suitePath,
                    options.SelectedSuiteCases,
                    options.ListCasesJson,
                    options.RelativePaths,
                    options.CheckFiles)
                : RegressionSuite.Run(
                    suitePath,
                    options.RecordMissingReferences,
                    options.RecordAllReferences,
                    options.SelectedSuiteCases);
            Console.Out.Flush();
            Console.Error.Flush();
            return code;
        }

        if (options.Headless)
        {
            var code = HeadlessRunner.Run(options);
            Console.Out.Flush();
            Console.Error.Flush();
            return code;
        }

        new EmuWin().Start();
        return ExitCodes.Success;
    }
}
