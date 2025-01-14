﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using OpenTemple.Core;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.IO.SaveGames.Archive;
using OpenTemple.Interop;

namespace OpenTemple.Windows;

public static class Launcher
{
    public static void Main(string[] args)
    {
        // When a debugger is attached, immediately rethrow unobserved exceptions from asynchronous tasks
        if (Debugger.IsAttached)
        {
            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
            {
                if (!eventArgs.Observed)
                {
                    throw eventArgs.Exception;
                }
            };
        }
        else
        {
            AppDomain.CurrentDomain.UnhandledException += HandleException;
        }

        if (JumpListHandler.Handle(args))
        {
            return;
        }

        if (args.Length > 0 && args[0] == "--extract-save")
        {
            ExtractSaveArchive.Main(args.Skip(1).ToArray());
            return;
        }

        if (args.Length == 2 && args[0] == "--mes-to-json")
        {
            var mesContent = MesFile.Read(args[1]);
            var newFile = Path.ChangeExtension(args[1], ".json");
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            var jsonContent = JsonSerializer.Serialize(mesContent.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            ), options);
            File.WriteAllText(newFile, jsonContent);
            return;
        }

        if (args.Length > 0 && args[0] == "--dump-addresses")
        {
            var dumper = new AddressDumper();
            dumper.DumpAddresses();
            return;
        }

        string dataDir = null;
        if (args.Length > 0 && args[0] == "--data-dir")
        {
            dataDir = args[1];
        }

        using var startup = new GameStartup {DataFolder = dataDir};

        if (startup.Startup())
        {
            startup.EnterMainMenu();

            Globals.GameLoop.Run();
        }
    }

    private static void HandleException(object sender, UnhandledExceptionEventArgs e)
    {
        var errorHeader = "Oops! A fatal error occurred.";

        var errorDetails = "Error Details:\n";
        errorDetails += e.ExceptionObject;

        try
        {
            NativePlatform.ShowMessage(
                true,
                "Fatal Error",
                errorHeader,
                errorDetails
            );
        }
        catch (Exception)
        {
            // In case the entire native library cant be loaded, the above call will fail
            // In those cases, we'll fall back to a super super low level MessageBox call,
            // which really shouldn't fail!
            var message = errorHeader + "\n\n" + errorDetails;
            message += "\n\nNOTE: You can press Ctrl+C to copy the content of this message box to your clipboard.";
            MessageBox(IntPtr.Zero, message, "OpenTemple - Fatal Error", 0x10);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern void MessageBox(IntPtr hwnd, string message, string title, int buttons);
}