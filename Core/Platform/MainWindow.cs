using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using OpenTemple.Core.Config;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Platform
{
    public delegate void MouseMoveHandler(int x, int y, int wheelDelta);

    public delegate bool WindowMsgFilter(uint msg, ulong wparam, long lparam);

    internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, ulong wParam, long lParam);

    [SuppressUnmanagedCodeSecurity]
    public class MainWindow : IMainWindow, IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private WindowConfig _config;

        private GCHandle _gcHandle;

        public IntPtr NativeHandle => _windowHandle;

        public MainWindow(WindowConfig config)
        {
            _config = config.Copy();
            if (_config.Width < _config.MinWidth)
            {
                _config.Width = _config.MinWidth;
            }

            if (_config.Height < _config.MinHeight)
            {
                _config.Height = _config.MinHeight;
            }

            _instanceHandle = GetModuleHandle(null);

            // This GC handle is attached to windows created with CreateWindow
            _gcHandle = GCHandle.Alloc(this);

            RegisterWndClass();
            CreateHwnd();
        }

        public void Dispose()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                DestroyWindow(_windowHandle);
            }

            _gcHandle.Free();

            UnregisterWndClass();
        }

        public IntPtr WindowHandle => _windowHandle;

        public IntPtr InstanceHandle => _instanceHandle;

        public Size Size => new Size(_width, _height);

        public WindowConfig WindowConfig
        {
            get => _config;
            set
            {
                _config = value.Copy();
                CreateWindowRectAndStyles(out var windowRect, out var style, out var styleEx);

                var currentStyles = unchecked((WindowStyles) (long) GetWindowLongPtr(_windowHandle, GWL_STYLE));
                currentStyles &= ~(WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_POPUP);
                currentStyles |= style;

                SetWindowLongPtr(_windowHandle, GWL_STYLE, (IntPtr) currentStyles);
                SetWindowLongPtr(_windowHandle, GWL_EXSTYLE, (IntPtr) styleEx);
                SetWindowPos(
                    _windowHandle,
                    IntPtr.Zero,
                    windowRect.Left,
                    windowRect.Top,
                    windowRect.Width,
                    windowRect.Height,
                    SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOZORDER
                );
            }
        }

        public bool IsInForeground { get; set; } = true;

        public event Action<bool> IsInForegroundChanged;

        private bool unsetClip = false;

        public event Action<Size> Resized;

        public event Action<IEvent> OnEvent;

        public event Action<string> OnTextInput;

        // State tracking for mouse clicks. This is derived from how Firefox does it.
        private Point _lastMousePoint;
        private Point _lastMouseMovePoint;
        private long _lastMouseDownTime;
        private long _lastClickCount;
        private int _lastMouseButton;
        private bool _canClick;
        private bool _canDoubleClick;

        // Locks the mouse cursor to this window
        // if we're in the foreground
        public void ConfineCursor(int x, int y, int w, int h)
        {
            bool isForeground = GetForegroundWindow() == _windowHandle;

            if (isForeground)
            {
                RECT rect = new RECT {X = x, Y = y, Right = x + w, Bottom = y + h};

                ClipCursor(ref rect);
                unsetClip = false;
            }
            else // Unset the CursorClip  (was supposed to happen naturally when alt-tabbing, but sometimes it requires hitting a keystroke before it takes effect so adding it here too)
            {
                // Check if already unset the CursorClip so it doesn't cause interference in case some other app is trying to clip the cursor...
                if (unsetClip)
                    return;
                unsetClip = true;

                ClipCursor(IntPtr.Zero);
            }
        }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
            mMouseMoveHandler = handler;
        }

        // Sets a filter that receives a chance at intercepting all window messages
        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
            mWindowMsgFilter = filter;
        }

        private readonly IntPtr _instanceHandle;
        private IntPtr _windowHandle;
        private int _width;
        private int _height;

        // The window class name used for RegisterClass and CreateWindow.
        private const string WindowClassName = "OpenTempleMainWnd";
        private const string WindowTitle = "OpenTemple";

        private void RegisterWndClass()
        {
            var wndClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                // Make sure to use the delegate object here, otherwise .NET might create a temporary one,
                // but we need to ensure the native twin of this delegate will live as long as the App.
                lpfnWndProc = WndProcTrampolineDelegate,
                hInstance = _instanceHandle,
                hIcon = LoadIcon(_instanceHandle, "icon"),
                hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
                hbrBackground = GetStockObject(StockObjects.BLACK_BRUSH),
                lpszClassName = WindowClassName,
                cbWndExtra = Marshal.SizeOf<GCHandle>()
            };

            if (RegisterClassEx(ref wndClass) == 0)
            {
                throw new Exception("Unable to register window class: " + Marshal.GetLastWin32Error());
            }
        }

        private void UnregisterWndClass()
        {
            if (!UnregisterClass(WindowClassName, _instanceHandle))
            {
                Logger.Error("Unable to unregister window class: " + Marshal.GetLastWin32Error());
            }
        }

        public void CreateHwnd()
        {
            CreateWindowRectAndStyles(out var windowRect, out var style, out var styleEx);

            style |= WindowStyles.WS_VISIBLE;

            var width = windowRect.Width;
            var height = windowRect.Height;
            Logger.Info("Creating window with dimensions {0}x{1}", width, height);
            _windowHandle = CreateWindowEx(
                styleEx,
                WindowClassName,
                WindowTitle,
                style,
                windowRect.Left,
                windowRect.Top,
                width,
                height,
                IntPtr.Zero,
                IntPtr.Zero,
                _instanceHandle,
                IntPtr.Zero);

            if (_windowHandle == IntPtr.Zero)
            {
                throw new Exception("Unable to create main window: " + Marshal.GetLastWin32Error());
            }

            // Store our this pointer in the window
            SetWindowLongPtr(_windowHandle, 0, GCHandle.ToIntPtr(_gcHandle));
        }

        private void CreateWindowRectAndStyles(out RECT windowRect, out WindowStyles style, out WindowStylesEx styleEx)
        {
            var screenWidth = GetSystemMetrics(SystemMetric.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetric.SM_CYSCREEN);

            styleEx = 0;

            if (!_config.Windowed)
            {
                windowRect = new RECT(0, 0, screenWidth, screenHeight);
                style = WindowStyles.WS_POPUP;
                _width = screenWidth;
                _height = screenHeight;
            }
            else
            {
                windowRect = new RECT(
                    (screenWidth - _config.Width) / 2,
                    (screenHeight - _config.Height) / 2,
                    0,
                    0);
                windowRect.Right = windowRect.Left + _config.Width;
                windowRect.Bottom = windowRect.Top + _config.Height;

                // style = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
                style = WindowStyles.WS_OVERLAPPEDWINDOW;

                AdjustWindowRectEx(ref windowRect, style, false, styleEx);
                int extraWidth = (windowRect.Right - windowRect.Left) - _config.Width;
                int extraHeight = (windowRect.Bottom - windowRect.Top) - _config.Height;
                windowRect.Left = (screenWidth - _config.Width) / 2 - (extraWidth / 2);
                windowRect.Top = (screenHeight - _config.Height) / 2 - (extraHeight / 2);
                windowRect.Right = windowRect.Left + _config.Width + extraWidth;
                windowRect.Bottom = windowRect.Top + _config.Height + extraHeight;

                _width = _config.Width;
                _height = _config.Height;
            }
        }

        // Keep a delegate object for the trampoline around to prevent it from being GC'd
        private static readonly WndProc WndProcTrampolineDelegate = WndProcTrampoline;

        private static IntPtr WndProcTrampoline(IntPtr hWnd, uint msg, ulong wparam, long lparam)
        {
            // Retrieve our this pointer from the wnd
            var handlePtr = GetWindowLongPtr(hWnd, 0);
            if (handlePtr != IntPtr.Zero)
            {
                var handle = GCHandle.FromIntPtr(handlePtr);

                if (handle.IsAllocated)
                {
                    var mainWindow = (MainWindow) handle.Target;
                    return mainWindow.WndProc(hWnd, msg, wparam, lparam);
                }
            }

            return DefWindowProc(hWnd, msg, wparam, lparam);
        }

        private int mousePosX = 0; // Replaces memory @ 10D25CEC
        private int mousePosY = 0; // Replaces memory @ 10D25CF0

        private IntPtr WndProc(IntPtr hWnd, uint msg, ulong wParam, long lParam)
        {
            if (hWnd != _windowHandle)
            {
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }

            if (mWindowMsgFilter != null && mWindowMsgFilter(msg, wParam, lParam))
            {
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }

            switch (msg)
            {
                case WM_SETFOCUS:
                    // Make our window topmost unless a debugger is attached
                    if ((IntPtr) wParam == _windowHandle && !IsDebuggerPresent())
                    {
                        SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }

                    break;
                case WM_KILLFOCUS:
                    // Make our window topmost unless a debugger is attached
                    if ((IntPtr) wParam == _windowHandle && !IsDebuggerPresent())
                    {
                        SetWindowPos(_windowHandle, HWND_NOTOPMOST, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }

                    break;
                case WM_SYSCOMMAND:
                    if (wParam == SC_KEYMENU || wParam == SC_SCREENSAVE || wParam == SC_MONITORPOWER)
                    {
                        return IntPtr.Zero;
                    }

                    break;
                case WM_GETMINMAXINFO:
                    unsafe
                    {
                        var minMaxInfo = (MinMaxInfo*) lParam;
                        minMaxInfo->ptMinTrackSize.X = _config.MinWidth;
                        minMaxInfo->ptMinTrackSize.Y = _config.MinHeight;
                    }

                    break;
                case WM_SIZE:
                    Resized?.Invoke(new Size(
                        WindowsMessageUtils.GetXParam(lParam),
                        WindowsMessageUtils.GetYParam(lParam)
                    ));
                    break;
                case WM_ACTIVATEAPP:
                    IsInForeground = wParam == 1;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                    break;
                case WM_ERASEBKGND:
                    return IntPtr.Zero;
                case WM_CLOSE:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs(1)));
                    break;
                case WM_QUIT:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs((int) wParam)));
                    break;
                case WM_LBUTTONDOWN:
                    HandleMouseButtonMessage(0, true, wParam, lParam);
                    break;
                case WM_LBUTTONUP:
                    HandleMouseButtonMessage(0, false, wParam, lParam);
                    break;
                case WM_RBUTTONDOWN:
                    HandleMouseButtonMessage(2, true, wParam, lParam);
                    break;
                case WM_RBUTTONUP:
                    HandleMouseButtonMessage(2, false, wParam, lParam);
                    break;
                case WM_MBUTTONDOWN:
                    HandleMouseButtonMessage(1, true, wParam, lParam);
                    break;
                case WM_MBUTTONUP:
                    HandleMouseButtonMessage(1, false, wParam, lParam);
                    break;
                case WM_XBUTTONDOWN:
                    HandleMouseButtonMessage(2 + WindowsMessageUtils.HiWord(wParam), true, wParam, lParam);
                    break;
                case WM_XBUTTONUP:
                    HandleMouseButtonMessage(2 + WindowsMessageUtils.HiWord(wParam), false, wParam, lParam);
                    break;
                case WM_SYSKEYDOWN:
                case WM_KEYDOWN:
                    HandleKeyMessage(true, wParam, lParam);
                    break;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    HandleKeyMessage(false, wParam, lParam);
                    break;
                case WM_CHAR:
                    HandleCharMessage(false, wParam, lParam);
                    break;
                case WM_MOUSEWHEEL:
                    HandleMouseWheelMessage(true, wParam, lParam);
                    break;
                case WM_MOUSEHWHEEL:
                    HandleMouseWheelMessage(false, wParam, lParam);
                    break;
                case WM_MOUSEMOVE:
                    mousePosX = WindowsMessageUtils.GetXParam(lParam);
                    mousePosY = WindowsMessageUtils.GetYParam(lParam);
                    HandleMouseMoveMessage(wParam, lParam);
                    break;
            }

            if (msg != WM_KEYDOWN)
            {
                // UpdateMousePos(mousePosX, mousePosY, 0);
            }

            // Previously, ToEE called a global window proc here but it did nothing useful.
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        // Dispatches mousedown and mouseup events
        // see https://www.w3.org/TR/uievents/#event-type-mousedown
        // https://www.w3.org/TR/uievents/#event-type-mouseup
        private void HandleMouseButtonMessage(int button, bool down, ulong wParam, long lParam)
        {
            var xPos = WindowsMessageUtils.GetXParam(lParam);
            var yPos = WindowsMessageUtils.GetYParam(lParam);

            var eventType = down ? SystemEventType.MouseDown : SystemEventType.MouseUp;
            var evt = new MouseEvent(eventType, ApplyKeyModifierState(new MouseEventInit()
            {
                ScreenX = xPos,
                ScreenY = yPos,
                ClientX = xPos,
                ClientY = yPos,
                Button = (short) button,
                Buttons = WindowsMessageUtils.GetMouseMessagePressedButtons(wParam),
                Bubbles = true,
                Cancelable = true,
                Composed = true,
                Detail = 0 // TODO: Click-Count
            }));

            EmitEvent(evt);
        }

        /// <summary>
        /// We stash the last keyboard event for which we could not determine the key.
        /// Since we only detect known non-printable keys, this means that for printable keys,
        /// we can copy all properties except the text from this event to generate keydown
        /// events for printable characters on receipt of the WM_CHAR message that follows
        /// the WM_KEYDOWN message.
        /// </summary>
        private KeyboardEvent _lastPrintableEvent;

        private void HandleKeyMessage(bool down, in ulong wParam, in long lParam)
        {
            var virtualKey = (VirtualKey) wParam;
            var scanCode = (byte)((lParam >> 16) & 0xFF);
            var repeat = (lParam & (1 << 30)) != 0;
            var extended = (lParam & (1 << 24)) != 0;

            var eventType = down ? SystemEventType.KeyDown : SystemEventType.KeyUp;
            var evt = new KeyboardEvent(eventType, ApplyKeyModifierState(new KeyboardEventInit()
            {
                Bubbles = true,
                Cancelable = true,
                Composed = true,
                Repeat = repeat,
                Code = ScanCodeIdTable.GetId(extended, scanCode),
                Key = WindowsKeyMapping.FromVirtualKey(virtualKey)
            }));

            if (evt.Key != KeyboardKey.Unidentified)
            {
                EmitEvent(evt);
            }
        }

        /// <summary>
        /// WM_CHAR only delivers UTF-16, which means characters such as Emoji and other languages are
        /// delivered using two characters per actual character. The characters will be identifiable
        /// as surrogates.
        /// We buffer the surrogates and then deliver them as a single text message.
        /// </summary>
        private char _bufferedSurrogate;

        private bool _bufferingSurrogate;

        private void HandleCharMessage(bool down, in ulong wParam, in long lParam)
        {
            var charCode = (char) wParam;
            string text;
            if (char.GetUnicodeCategory(charCode) == UnicodeCategory.Surrogate)
            {
                if (_bufferingSurrogate)
                {
                    Span<char> combinedSurrogates = stackalloc char[2];
                    combinedSurrogates[0] = _bufferedSurrogate;
                    combinedSurrogates[1] = charCode;
                    _bufferingSurrogate = false;
                    text = combinedSurrogates.ToString();
                }
                else
                {
                    _bufferingSurrogate = true;
                    _bufferedSurrogate = charCode;
                    return; // Wait for the low-surrogate value
                }
            }
            else
            {
                if (_bufferingSurrogate)
                {
                    // Received only half of a buffered surrogate
                    Logger.Warn("Dropping half-buffered surrogate character.");
                    _bufferingSurrogate = false;
                }

                text = new string(charCode, 1);
            }

            OnTextInput?.Invoke(text);
        }

        private void EmitEvent(IEvent evt)
        {
            OnEvent?.Invoke(evt);
        }

        private bool InsideClickThreshold(int x, int y)
        {
            // TODO: This seems actually wrong since the SM_CXDOUBLECLK is the "width of the rectangle"
            // TODO: For the first click, this should be SM_CXDRAG
            return Math.Abs(_lastMousePoint.X - x) < GetSystemMetrics(SystemMetric.SM_CXDRAG)
                   && Math.Abs(_lastMousePoint.Y - y) < GetSystemMetrics(SystemMetric.SM_CYDRAG);
        }

        private bool InsideDoubleClickThreshold(int x, int y)
        {
            // TODO: This seems actually wrong since the SM_CXDOUBLECLK is the "width of the rectangle"
            // TODO: For the first click, this should be SM_CXDRAG
            return Math.Abs(_lastMousePoint.X - x) < GetSystemMetrics(SystemMetric.SM_CXDOUBLECLK)
                   && Math.Abs(_lastMousePoint.Y - y) < GetSystemMetrics(SystemMetric.SM_CYDOUBLECLK);
        }

        // Dispatches mousemove events
        private void HandleMouseMoveMessage(ulong wParam, long lParam)
        {
            var xPos = WindowsMessageUtils.GetXParam(lParam);
            var yPos = WindowsMessageUtils.GetYParam(lParam);

            // Swallow duplicate mouse move events
            if (_lastMouseMovePoint.X == xPos && _lastMouseMovePoint.Y == yPos)
            {
                return;
            }

            _lastMouseMovePoint.X = xPos;
            _lastMouseMovePoint.Y = yPos;

            var evt = new MouseEvent(SystemEventType.MouseMove, ApplyKeyModifierState(new MouseEventInit()
            {
                ScreenX = xPos,
                ScreenY = yPos,
                ClientX = xPos,
                ClientY = yPos,
                Buttons = WindowsMessageUtils.GetMouseMessagePressedButtons(wParam),
                Bubbles = true,
                Cancelable = true,
                Composed = true
            }));
            EmitEvent(evt);
        }

        /// <summary>
        /// This is a constant on Windows in terms of "ticks". Whatever a "tick" is.
        /// </summary>
        private const int WHEEL_DELTA = 120;

        /// <summary>
        /// If Windows reports this value as the number of lines to scroll, it is
        /// requesting us to scroll one page at a time instead.
        /// </summary>
        private const uint WHEEL_PAGESCROLL = uint.MaxValue;

        // These are the same defaults as in Chromium
        private const uint DefaultScrollLinesPerWheelDelta = 3;
        private const uint DefaultScrollCharsPerWheelDelta = 1;

        /// <summary>
        /// Dispatches wheel events. Logic is based roughly on what Chromium does, but does not
        /// synthesize wheel events from WM_SCROLL.
        /// </summary>
        private void HandleMouseWheelMessage(bool vertical, ulong wParam, long lParam)
        {
            var xPos = (int) WindowsMessageUtils.GetXParam(lParam);
            var yPos = (int) WindowsMessageUtils.GetYParam(lParam);

            var p = new POINT() {X = xPos, Y = yPos};
            ScreenToClient(_windowHandle, ref p);
            xPos = p.X;
            yPos = p.Y;

            var wheelDelta = WindowsMessageUtils.GetWheelDelta(wParam);
            // One tick is supposed to be one "notch" on a normal scroll wheel
            var tickDelta = wheelDelta / (float) WHEEL_DELTA;
            DeltaModeCode deltaMode;
            var scrollDelta = tickDelta;
            if (vertical)
            {
                var scrollLines = DefaultScrollLinesPerWheelDelta;
                SystemParametersInfoUInt(SPI.SPI_GETWHEELSCROLLLINES, 0, ref scrollLines, 0);
                if (scrollLines == WHEEL_PAGESCROLL)
                {
                    deltaMode = DeltaModeCode.PAGE;
                }
                else
                {
                    scrollDelta *= scrollLines;
                    deltaMode = DeltaModeCode.LINE;
                }
            }
            else
            {
                // Retrieve the system-wide user-preference for horizontal mouse-wheel scrolling
                var scrollChars = DefaultScrollCharsPerWheelDelta;
                SystemParametersInfoUInt(SPI.SPI_GETWHEELSCROLLCHARS, 0, ref scrollChars, 0);
                scrollDelta *= scrollChars;
                // This is kinda bullshit since it's not "lines"
                deltaMode = DeltaModeCode.LINE;
            }

            var wheelEventInit = new WheelEventInit()
            {
                ScreenX = xPos,
                ScreenY = yPos,
                ClientX = xPos,
                ClientY = yPos,
                Buttons = WindowsMessageUtils.GetMouseMessagePressedButtons(wParam),
                Bubbles = true,
                Cancelable = true,
                Composed = true,
                DeltaMode = deltaMode
            };

            // Set scroll amount based on above calculations.  WebKit expects positive
            // deltaY to mean "scroll up" and positive deltaX to mean "scroll left".
            if (vertical)
            {
                wheelEventInit.DeltaY = scrollDelta;
                wheelEventInit.WheelTicksY = tickDelta;
            }
            else
            {
                wheelEventInit.DeltaX = scrollDelta;
                wheelEventInit.WheelTicksX = tickDelta;
            }

            var evt = new WheelEvent(SystemEventType.Wheel, ApplyKeyModifierState(wheelEventInit));
            EmitEvent(evt);
        }

        /// <summary>
        /// Queries and sets the key modifier state on the given event init object.
        /// </summary>
        private T ApplyKeyModifierState<T>(T eventInit) where T : EventModifierInit
        {
            static bool IsDown(VirtualKey key)
            {
                return (GetKeyState(key) & 0x8000) != 0;
            }

            eventInit.ShiftKey = IsDown(VirtualKey.VK_SHIFT);
            eventInit.CtrlKey = IsDown(VirtualKey.VK_CONTROL);
            eventInit.AltKey = IsDown(VirtualKey.VK_MENU);
            eventInit.MetaKey = IsDown(VirtualKey.VK_LWIN) || IsDown(VirtualKey.VK_RWIN);
            return eventInit;
        }

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool IsDebuggerPresent();

        private const uint WM_ACTIVATEAPP = 0x001C;
        private const uint WM_APPCOMMAND = 0x0319;
        private const uint WM_SETFOCUS = 0x0007;
        private const uint WM_KILLFOCUS = 0x0008;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint WM_SIZE = 0x0005;
        private const uint WM_ERASEBKGND = 0x0014;
        private const uint WM_GETMINMAXINFO = 0x0024;

        private const uint WM_CHAR = 0x0102;
        private const uint WM_CLOSE = 0x0010;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MBUTTONDOWN = 0x0207;
        private const uint WM_MBUTTONUP = 0x0208;
        private const uint WM_XBUTTONDOWN = 0x020B;
        private const uint WM_XBUTTONUP = 0x020C;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint WM_MOUSEHWHEEL = 0x020E;
        private const uint WM_QUIT = 0x0012;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
        private const uint WM_SYSKEYDOWN = 0x0104;
        private const uint WM_SYSKEYUP = 0x0105;

        private const uint SC_KEYMENU = 0xF100;
        private const uint SC_SCREENSAVE = 0xF140;
        private const uint SC_MONITORPOWER = 0xF170;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        private const uint
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x004,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private void UpdateMousePos(int xAbs, int yAbs, int wheelDelta)
        {
            mMouseMoveHandler?.Invoke(xAbs, yAbs, wheelDelta);
        }


        private MouseMoveHandler mMouseMoveHandler;
        private WindowMsgFilter mWindowMsgFilter;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, ulong wParam, long lParam);

        [DllImport("user32.dll")]
        private static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyles dwStyle,
            bool bMenu, WindowStylesEx dwExStyle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U2)]
        private static extern short RegisterClassEx([In]
            ref WNDCLASSEX lpwcx);

        [DllImport("gdi32.dll")]
        private static extern IntPtr GetStockObject(StockObjects fnObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

        private static int IDC_ARROW = 32512;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private const int GWL_STYLE = -16;

        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(VirtualKey nVirtKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        private enum SPI
        {
            /// <summary>
            /// Retrieves the number of characters to scroll when the horizontal mouse wheel is moved.
            /// The pvParam parameter must point to a UINT variable that receives the number of lines. The default value is 3.
            /// </summary>
            SPI_GETWHEELSCROLLCHARS = 0x006C,

            /// <summary>
            /// Retrieves the number of lines to scroll when the vertical mouse wheel is moved.
            /// The pvParam parameter must point to a UINT variable that receives the number of lines. The default value is 3.
            /// </summary>
            SPI_GETWHEELSCROLLLINES = 0x0068
        }

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfoUInt(SPI uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            WindowStylesEx dwExStyle,
            string lpClassName,
            string lpWindowName,
            WindowStyles dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);
    }
}