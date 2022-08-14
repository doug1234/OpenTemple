using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using static SDL2.SDL;

namespace OpenTemple.Core.Platform;

public delegate bool SDLEventFilter(ref SDL_Event e);

public class MainWindow : IMainWindow
{
    // The window class name used for RegisterClass and CreateWindow.
    private const string WindowClassName = "OpenTempleMainWnd";
    private const string WindowTitle = "OpenTemple";

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private WindowConfig _config;

    public IntPtr NativeHandle => _windowHandle;

    private SDLEventFilter _eventFilter;
    
    /// <summary>
    /// SDL2 Window Pointer.
    /// </summary>
    private IntPtr _window;

    public IntPtr SDLWindow => _window;

    private uint _windowId;

    // Used to determine whether a MouseEnter event should be emitted when a mouse event is received
    private bool _mouseFocus;

    public Size Size => new(_width, _height);

    private readonly IFileSystem _fs;
    private IntPtr _windowHandle;
    private int _width;
    private int _height;

    // Caches for cursors
    private readonly Dictionary<string, IntPtr> _cursorCache = new();
    private IntPtr _defaultCursor;
    private IntPtr _currentCursor = IntPtr.Zero;

    public WindowConfig WindowConfig
    {
        get => _config;
        set
        {
            var changingFullscreen = _config.Windowed != value.Windowed;
            _config = value.Copy();

            if (changingFullscreen)
            {
                CreateWindowRectAndStyles(
                    out var x,
                    out var y,
                    out var width,
                    out var height,
                    out _
                );

                SDL_SetWindowFullscreen(
                    _window,
                    (uint) (_config.Windowed ? 0 : SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP)
                );

                if (_config.Windowed)
                {
                    SDL_SetWindowPosition(_window, x, y);
                    SDL_SetWindowSize(_window, width, height);
                    if (_config.Maximized)
                    {
                        SDL_MaximizeWindow(_window);
                    }
                }
            }
        }
    }

    public bool IsInForeground { get; set; } = true;

    public event Action<bool>? IsInForegroundChanged;

    public event Action<WindowEvent>? OnInput;

    public event Action<Size>? Resized;

    public event Action? Closed;

    public SizeF UiCanvasSize { get; private set; }

    private Size _uiCanvasTargetSize = new(1024, 768);

    public Size UiCanvasTargetSize
    {
        get => _uiCanvasTargetSize;
        set
        {
            if (value.Width <= 0 || value.Height <= 0)
            {
                throw new ArgumentException("Cannot set target UI size to 0");
            }

            _uiCanvasTargetSize = value;
            UpdateUiCanvasSize();
        }
    }

    public event Action UiCanvasSizeChanged;

    public float UiScale { get; private set; }

    public bool IsCursorVisible
    {
        set
        {
            if (value)
            {
                SDL_ShowCursor(SDL_ENABLE);
                UpdateCursor();
            }
            else
            {
                SDL_ShowCursor(SDL_DISABLE);
            }
        }
    }

    public MainWindow(WindowConfig config, IFileSystem fs)
    {
        try
        {
            WindowsPlatform.RegisterWindowClass(WindowClassName);
        }
        catch (EntryPointNotFoundException)
        {
        }

        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            throw new SDLException("Failed to initialize video subsystem.");
        }

        _defaultCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        _fs = fs;
        _config = config.Copy();
        if (_config.Width < _config.MinWidth)
        {
            _config.Width = _config.MinWidth;
        }

        if (_config.Height < _config.MinHeight)
        {
            _config.Height = _config.MinHeight;
        }

        CreateWindow();
        UpdateUiCanvasSize();
    }

    public void Dispose()
    {
        if (_window != IntPtr.Zero)
        {
            if (_config.DisableScreenSaver)
            {
                SDL_EnableScreenSaver();
            }

            SDL_DestroyWindow(_window);
            try
            {
                WindowsPlatform.UnregisterWindowClass();
            }
            catch (EntryPointNotFoundException)
            {
            }
            
            SDL_FreeCursor(_defaultCursor);
            _defaultCursor = IntPtr.Zero;
            foreach (var cursor in _cursorCache.Values)
            {
                SDL_FreeCursor(cursor);
            }
            _cursorCache.Clear();
            _currentCursor = IntPtr.Zero;

            _window = IntPtr.Zero;
            _windowId = 0;
        }
    }

    public void ProcessEvents()
    {
        while (SDL_PollEvent(out var e) != 0)
        {
            if (_eventFilter != null && _eventFilter(ref e))
            {
                continue;
            }
            
            switch (e.type)
            {
                case SDL_EventType.SDL_APP_TERMINATING:
                case SDL_EventType.SDL_QUIT:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs(0)));
                    Closed?.Invoke();
                    return;
                case SDL_EventType.SDL_WINDOWEVENT:
                    HandleWindowEvent(ref e.window);
                    return;
                
                case SDL_EventType.SDL_KEYDOWN:
                case SDL_EventType.SDL_KEYUP:
                    HandleKeyEvent(ref e.key);
                    break;
                
                case SDL_EventType.SDL_TEXTINPUT:
                    HandleTextInputEvent(ref e.text);
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    HandleMouseMoveEvent(WindowEventType.MouseMove, ref e.motion);
                    break;
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    HandleMouseButtonEvent(WindowEventType.MouseDown, ref e.button);
                    break;
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    HandleMouseButtonEvent(WindowEventType.MouseUp, ref e.button);
                    break;
                case SDL_EventType.SDL_MOUSEWHEEL:
                    HandleMouseWheelEvent(ref e.wheel);
                    break;
                default:
                    break;
            }
        }
    }

    private unsafe void HandleTextInputEvent(ref SDL_TextInputEvent e)
    {
        var maxCharCount = System.Text.Encoding.UTF8.GetMaxCharCount(32);
        var chars = stackalloc char[maxCharCount];
        int charCount;
        fixed (byte* bytes = e.text)
        {
            charCount = System.Text.Encoding.UTF8.GetChars(bytes, 32, chars, maxCharCount);
        }

        for (var i = 0; i < charCount; i++)
        {
            var ch = chars[i];
            if (ch == 0)
            {
                break;
            }
            Tig.MessageQueue.Enqueue(new Message(new MessageCharArgs(ch)));
        }
    }

    private void HandleKeyEvent(ref SDL_KeyboardEvent e)
    {
        // Ignore keyboard events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }
        
        var down = e.state == SDL_PRESSED;
        var repeat = e.repeat != 0;

        var keysym = e.keysym;
        var modAlt = (keysym.mod & SDL_Keymod.KMOD_ALT) != 0;
        var modCtrl = (keysym.mod & SDL_Keymod.KMOD_CTRL) != 0;
        
        // Handle Alt+Enter here
        if (modAlt && keysym.scancode == SDL_Scancode.SDL_SCANCODE_RETURN)
        {
            // Ignore repeats, trigger on down
            if (!repeat && down)
            {
                var config = _config.Copy();
                config.Windowed = !config.Windowed;
                WindowConfig = config;
            }

            return;
        }

        var key = SDLScanCodeMap.GetDIK(keysym.scancode);
        if (key != 0)
        {
            Tig.MessageQueue.Enqueue(new Message(
                new MessageKeyStateChangeArgs
                {
                    key = key,
                    // Means it has changed to pressed
                    down = down,
                    modAlt = modAlt,
                    modCtrl = modCtrl
                }
            ));
        }
    }

    private void HandleWindowEvent(ref SDL_WindowEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }
        
        switch (e.windowEvent)
        {
            case SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
            {
                var width = e.data1;
                var height = e.data2;

                // Ignore resizes to 0 (i.e. due to being minimized)
                if (width == 0 || height == 0)
                {
                    break;
                }

                // Update width/height with window client size
                if (width != _width || height != _height)
                {
                    _width = width;
                    _height = height;

                    Resized?.Invoke(Size);
                    UpdateUiCanvasSize();
                }
                
                // Persist changes to window size in window mode
                if (_config.Windowed && (_config.Width != width || _config.Height != height))
                {
                    _config.Width = width;
                    _config.Height = height;
                    Globals.Config.Window.Width = _config.Width;
                    Globals.Config.Window.Height = _config.Height;
                    Globals.ConfigManager.Save();
                }

                break;
            }
            case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
            case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                if (_config.Windowed)
                {
                    // If the window was maximized by the user, store that in the config
                    _config.Maximized = e.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED;
                    Globals.Config.Window.Maximized = _config.Maximized;
                    Globals.ConfigManager.Save();
                }

                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                HandleMouseFocusEvent(true);
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                HandleMouseFocusEvent(false);
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                Logger.Debug("Main window gained keyboard focus.");

                if (!IsInForeground)
                {
                    IsInForeground = true;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                }

                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                Logger.Debug("Main window lost keyboard focus.");

                if (IsInForeground)
                {
                    IsInForeground = false;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                }

                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs(1)));
                Closed?.Invoke();
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_TAKE_FOCUS:
                Logger.Debug("Main window is being offered keyboard focus.");
                SDL_SetWindowInputFocus(_window);
                SDL_RaiseWindow(_window);
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_DISPLAY_CHANGED:
                break;
        }
    }

    /// <summary>
    /// Changes the currently used cursor to the given surface.
    /// </summary>
    public void SetCursor(int hotspotX, int hotspotY, string imagePath)
    {
        if (!_cursorCache.TryGetValue(imagePath, out var cursor))
        {
            var textureData = _fs.ReadBinaryFile(imagePath);
            try
            {
                cursor = IO.Images.ImageIO.LoadImageToCursor(textureData, hotspotX, hotspotY);
            }
            catch (Exception e)
            {
                cursor = IntPtr.Zero;
                Logger.Error("Failed to load cursor {0}: {1}", imagePath, e);
            }

            _cursorCache[imagePath] = cursor;
        }

        SDL_SetCursor(cursor);
        _currentCursor = cursor;
    }

    private void UpdateUiCanvasSize()
    {
        // Attempt to fit 1024x768 onto the backbuffer
        var horScale = MathF.Max(1, _width / (float) _uiCanvasTargetSize.Width);
        var verScale = MathF.Max(1, _height / (float) _uiCanvasTargetSize.Height);
        UiScale = Math.Min(horScale, verScale);

        UiCanvasSize = new SizeF(MathF.Floor(_width / UiScale), MathF.Floor(_height / UiScale));
        if (UiCanvasSize.IsEmpty)
        {
            Debugger.Break();
        }

        UiCanvasSizeChanged?.Invoke();
    }

    // Locks the mouse cursor to this window
    // if we're in the foreground
    public void ConfineCursor(int x, int y, int w, int h)
    {
        SDL_Rect rect = default;
        rect.x = x;
        rect.y = y;
        rect.w = w;
        rect.h = h;

        if (SDL_SetWindowMouseRect(_window, ref rect) < 0)
        {
            Logger.Warn("Failed to confine cursor to window: {0}", SDL_GetError());
        }
    }

    // Sets a filter that receives a chance at intercepting all window messages
    public void SetWindowMsgFilter(SDLEventFilter filter)
    {
        _eventFilter = filter;
    }

    private void CreateWindow()
    {
        CreateWindowRectAndStyles(
            out var x,
            out var y,
            out var width,
            out var height,
            out var flags
        );

        flags |= SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

        // Show initially maximized in window mode, if the game was previously maximized
        if (_config.Windowed && _config.Maximized)
        {
            flags |= SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
        }

        Logger.Info("Creating window with dimensions {0}x{1}", width, height);

        // Create our window
        _window = SDL_CreateWindow(WindowTitle, x, y, width, height, flags);

        // Make sure creating the window succeeded
        if (_window == IntPtr.Zero)
        {
            throw new SDLException("Failed to create main window");
        }

        _windowId = SDL_GetWindowID(_window);

        SDL_SysWMinfo wmInfo = default;
        SDL_VERSION(out wmInfo.version);
        if (SDL_GetWindowWMInfo(_window, ref wmInfo) != SDL_bool.SDL_TRUE)
        {
            throw new SDLException("Couldn't get HWND from Window");
        }

        _windowHandle = wmInfo.info.win.window;

        SDL_GetWindowSize(_window, out var actualWidth, out var actualHeight);
        // The returned size should never be zero, unless the window was forced by some hook to be minimized
        if (actualWidth > 0 && actualHeight > 0)
        {
            _width = actualWidth;
            _height = actualHeight;
            Logger.Info("Actual window dimensions {0}x{1}", width, height);
        }

        SDL_SetWindowMinimumSize(_window, _config.MinWidth, _config.MinHeight);

        if (_config.DisableScreenSaver)
        {
            SDL_DisableScreenSaver();
        }
    }

    private void CreateWindowRectAndStyles(out int x, out int y, out int width, out int height, out SDL_WindowFlags flags)
    {
        flags = default;

        if (!_config.Windowed)
        {
            x = SDL_WINDOWPOS_CENTERED;
            y = SDL_WINDOWPOS_CENTERED;
            // According to SDL2, this is ignored in fullscreen mode
            width = 1024;
            height = 768;
            flags = SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        }
        else
        {
            x = SDL_WINDOWPOS_CENTERED;
            y = SDL_WINDOWPOS_CENTERED;
            width = _config.Width;
            height = _config.Height;
            flags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        _width = width;
        _height = height;
    }

    private void HandleMouseWheelEvent(ref SDL_MouseWheelEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }
        
        HandleMouseFocusEvent(true);

        SDL_GetMouseState(out var x, out var y);

        var windowPos = new Point(x, y);
        var uiPos = TranslateToUiCanvas(windowPos);

        var units = e.preciseY;
        if (e.direction == (uint) SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED)
        {
            units *= -1;
        }

        OnInput?.Invoke(new MouseWheelWindowEvent(
            WindowEventType.Wheel,
            this,
            windowPos,
            uiPos,
            units
        ));
    }

    private void HandleMouseMoveEvent(WindowEventType type, ref SDL_MouseMotionEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }
        
        HandleMouseFocusEvent(true);
        var windowPos = new Point(e.x, e.y);
        var uiPos = TranslateToUiCanvas(windowPos);
        OnInput?.Invoke(new MouseWindowEvent(
            type,
            this,
            windowPos,
            uiPos
        ));
    }

    private void HandleMouseButtonEvent(WindowEventType type, ref SDL_MouseButtonEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }

        MouseButton button;
        if (e.button == SDL_BUTTON_LEFT)
        {
            button = MouseButton.LEFT;
        }
        else if (e.button == SDL_BUTTON_MIDDLE)
        {
            button = MouseButton.MIDDLE;
        }
        else if (e.button == SDL_BUTTON_RIGHT)
        {
            button = MouseButton.RIGHT;
        }
        else if (e.button == SDL_BUTTON_X1)
        {
            button = MouseButton.EXTRA1;
        }
        else if (e.button == SDL_BUTTON_X2)
        {
            button = MouseButton.EXTRA2;
        }
        else
        {
            Logger.Info("Ignoring event for unknown mouse button {0}", e.button);
            return;
        }

        HandleMouseFocusEvent(true);
        var windowPos = new Point(e.x, e.y);
        var uiPos = TranslateToUiCanvas(windowPos);
        OnInput?.Invoke(new MouseWindowEvent(
            type,
            this,
            windowPos,
            uiPos
        )
        {
            Button = button
        });
    }

    private void HandleMouseFocusEvent(bool focus)
    {
        if (focus == _mouseFocus)
        {
            return;
        }

        _mouseFocus = focus;

        var type = focus ? WindowEventType.MouseEnter : WindowEventType.MouseLeave;
        OnInput?.Invoke(new MouseFocusEvent(type, this));
    }

    private PointF TranslateToUiCanvas(int x, int y) => new(x / UiScale, y / UiScale);

    private PointF TranslateToUiCanvas(Point p) => TranslateToUiCanvas(p.X, p.Y);

    public void UpdateCursor()
    {
        SDL_SetCursor(_currentCursor != IntPtr.Zero ? _currentCursor : _defaultCursor);
    }
}
