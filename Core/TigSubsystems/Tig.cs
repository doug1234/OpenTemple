using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Scripting;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.TigSubsystems;

public static class Tig
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    /// <summary>
    /// This is development scripting.
    /// </summary>
    public static IDynamicScripting DynamicScripting { get; set; }

    public static IFileSystem FS { get; set; }

    public static TigMouse Mouse { get; set; }

    public static TigKeyboard Keyboard { get; set; }

    public static IMainWindow MainWindow { get; set; }
    
    public static EventLoop EventLoop { get; set; }

    public static RenderingDevice RenderingDevice { get; set; }

    public static Textures Textures => RenderingDevice.GetTextures();

    public static IDebugUI DebugUI { get; private set; }

    public static MdfMaterialFactory MdfFactory { get; set; }

    public static ShapeRenderer2d ShapeRenderer2d { get; set; }

    public static ShapeRenderer3d ShapeRenderer3d { get; set; }

    public static TextLayouter TextLayouter { get; set; }

    public static TigSound Sound { get; set; }

    public static TigFonts Fonts { get; set; }

    public static TigConsole Console { get; set; }

    public static void UpdateConfig(GameConfig config)
    {
        var windowConfig = config.Window;
        MainWindow.WindowConfig = windowConfig;
    }

    public static void Startup(GameConfig config, TigSettings? tigSettings = null)
    {
        tigSettings ??= new TigSettings();

        Logger.Info("Initializing TIG");

        FS = CreateFileSystem(config.InstallationFolder, tigSettings.DataFolder);

        DynamicScripting = new AsyncLoadedDynamicScripting();

        if (tigSettings.OffScreen)
        {
            MainWindow = new HeadlessMainWindow();
        }
        else
        {
            MainWindow = new MainWindow(config.Window, FS);
        }

        var configRendering = config.Rendering;
        RenderingDevice = new RenderingDevice(
            FS,
            MainWindow,
            configRendering.AdapterIndex,
            configRendering.DebugDevice);

        if (config.EnableDebugUI)
        {
            DebugUI = new DebugUiSystem(MainWindow, RenderingDevice);
        }
        else
        {
            DebugUI = new NoOpDebugUI();
        }

        MdfFactory = new MdfMaterialFactory(FS, RenderingDevice);
        ShapeRenderer2d = new ShapeRenderer2d(RenderingDevice);
        ShapeRenderer3d = new ShapeRenderer3d(RenderingDevice);
        TextLayouter = new TextLayouter(RenderingDevice, ShapeRenderer2d);

        // TODO mStartedSystems.emplace_back(StartSystem("idxtable.c", 0x101EC400, 0x101ECAD0));
        // TODO mStartedSystems.emplace_back(StartSystem("trect.c", TigStartupNoop, 0x101E4E40));
        // TODO mStartedSystems.emplace_back(StartSystem("color.c", 0x101ECB20, 0x101ED070));

        // TODO mLegacyVideoSystem = std::make_unique<LegacyVideoSystem>(*mMainWindow, *mRenderingDevice);


        // mStartedSystems.emplace_back(StartSystem("video.c", 0x101DC6E0, 0x101D8540));

        // TODO mStartedSystems.emplace_back(StartSystem("shader", 0x101E3350, 0x101E2090));
        // TODO mStartedSystems.emplace_back(StartSystem("palette.c", 0x101EBE30, 0x101EBEB0));
        // TODO mStartedSystems.emplace_back(StartSystem("window.c", 0x101DED20, 0x101DF320));
        // TODO mStartedSystems.emplace_back(StartSystem("timer.c", 0x101E34E0, 0x101E34F0));
        // mStartedSystems.emplace_back(StartSystem("dxinput.c", 0x101FF910, 0x101FF950));
        // mStartedSystems.emplace_back(StartSystem("keyboard.c", 0x101DE430, 0x101DE2D0));
        Keyboard = new TigKeyboard();

        // mStartedSystems.emplace_back(StartSystem("texture.c", 0x101EDF60, 0x101EE0A0));
        Mouse = new TigMouse();
        // TODO mStartedSystems.emplace_back(StartSystem("mouse.c", 0x101DDF50, 0x101DDE30));
        // TODO mStartedSystems.emplace_back(StartSystem("message.c", 0x101DE460, 0x101DE4E0));

        // startedSystems.emplace_back(StartSystem("gfx.c", TigStartupNoop, TigShutdownNoop));
        // TODO mStartedSystems.emplace_back(StartSystem("strparse.c", 0x101EBF00, TigShutdownNoop));
        // TODO mStartedSystems.emplace_back(StartSystem("filecache.c", TigStartupNoop, TigShutdownNoop));
        Sound = new TigSound(
            soundId => GameSystems.SoundGame.FindSoundFilename(soundId),
            tigSettings.DisableSound
        );
        // TODO mSoundSystem = std::make_unique<temple::SoundSystem>();
        // TODO mMovieSystem = std::make_unique<temple::MovieSystem>(*mSoundSystem);
        // mStartedSystems.emplace_back(StartSystem("movie.c", 0x101F1090, TigShutdownNoop));
        // NOTE: WFT -> UiManager
        // TODO mStartedSystems.emplace_back(StartSystem("wft.c", 0x101F98A0, 0x101F9770));

        // TODO mStartedSystems.emplace_back(StartSystem("font.c", 0x101EAEC0, 0x101EA140));
        Fonts = new TigFonts();
        Fonts.LoadAllFrom("art/arial-10");
        Fonts.PushFont("arial-10", 10);

        // TODO mConsole = std::make_unique<Console>();
        // mStartedSystems.emplace_back(StartSystem("console.c", 0x101E0290, 0x101DFBC0));
        Console = new TigConsole(DynamicScripting);
        // TODO mStartedSystems.emplace_back(StartSystem("loadscreen.c", 0x101E8260, TigShutdownNoop));

        // TODO *tigInternal.consoleDisabled = false; // tig init disables console by default

        EventLoop = new EventLoop(MainWindow, Keyboard, Sound);        
    }

    public static IFileSystem CreateFileSystem(string installationFolder, string? dataDirectory)
    {
        Logger.Info("Using ToEE installation from '{0}'", installationFolder);

        var vfs = TroikaVfs.CreateFromInstallationDir(installationFolder);

        if (dataDirectory == null)
        {
            // We usually assume that the Data directory is right below our executable location
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                Logger.Error("Failed to determine entry point assembly.");
                return vfs;
            }

            var location = Path.GetDirectoryName(entryAssembly.Location);
            dataDirectory = Path.Join(location, "Data");
#if DEBUG
            if (!Directory.Exists(dataDirectory))
            {
                dataDirectory = Path.GetFullPath("../Data");
            }
#endif
        }

        if (!Directory.Exists(dataDirectory))
        {
            throw new FileNotFoundException("Failed to find data folder. Tried: " + dataDirectory);
        }

        Logger.Info("Using additional data from: {0}", dataDirectory);
        vfs.AddDataDir(dataDirectory, true);
        return vfs;
    }

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public static void Shutdown()
    {
        var sw = Stopwatch.StartNew();
        Logger.Info("TIG shutdown started...");
        MainWindow?.Dispose();
        RenderingDevice?.Dispose();
        MdfFactory?.Dispose();
        ShapeRenderer2d?.Dispose();
        ShapeRenderer3d?.Dispose();
        TextLayouter?.Dispose();
        Sound?.Dispose();
        Fonts?.Dispose();

        DynamicScripting = null!;
        FS = null!;
        Mouse = null!;
        Keyboard = null!;
        MainWindow = null!;
        RenderingDevice = null!;
        DebugUI = null!;
        MdfFactory = null!;
        ShapeRenderer2d = null!;
        ShapeRenderer3d = null!;
        TextLayouter = null!;
        Sound = null!;
        Fonts = null!;
        Console = null!;
        EventLoop = null!;
        Logger.Info("TIG shutdown completed in {0}.", sw.Elapsed);
    }
}

public class TigSettings
{
    public string? DataFolder { get; set; }

    public bool OffScreen { get; set; }

    public bool DisableSound { get; set; }
}