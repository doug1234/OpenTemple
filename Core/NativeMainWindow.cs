using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Utils;
using OpenTemple.Interop;
using QtQuick;
using D3D11Device = SharpDX.Direct3D11.Device;

namespace OpenTemple.Core
{
    /// <summary>
    ///     Definitions come from Qt's QEvent::Type
    /// </summary>
    public enum NativeMouseEventType
    {
        MouseButtonPress = 2, // mouse button pressed
        MouseButtonRelease = 3, // mouse button released
        MouseButtonDblClick = 4, // mouse button double click
        MouseMove = 5 // mouse move
    }

    public enum NativeKeyEventType
    {
        KeyPress = 6, // key pressed
        KeyRelease = 7 // key released
    }

    [Flags]
    public enum NativeKeyboardModifiers : uint
    {
        NoModifier = 0x00000000,
        ShiftModifier = 0x02000000,
        ControlModifier = 0x04000000,
        AltModifier = 0x08000000,
        MetaModifier = 0x10000000,
        KeypadModifier = 0x20000000,
        GroupSwitchModifier = 0x40000000,

        // Do not extend the mask to include 0x01000000
        KeyboardModifierMask = 0xfe000000
    }

    [Flags]
    public enum NativeMouseButton : uint
    {
        NoButton = 0x00000000,
        LeftButton = 0x00000001,
        RightButton = 0x00000002,
        MidButton = 0x00000004, // ### Qt 6: remove me
        MiddleButton = MidButton,
        BackButton = 0x00000008,
        XButton1 = BackButton,
        ExtraButton1 = XButton1,
        ForwardButton = 0x00000010,
        XButton2 = ForwardButton,
        ExtraButton2 = ForwardButton,
        TaskButton = 0x00000020,
        ExtraButton3 = TaskButton,
        ExtraButton4 = 0x00000040,
        ExtraButton5 = 0x00000080,
        ExtraButton6 = 0x00000100,
        ExtraButton7 = 0x00000200,
        ExtraButton8 = 0x00000400,
        ExtraButton9 = 0x00000800,
        ExtraButton10 = 0x00001000,
        ExtraButton11 = 0x00002000,
        ExtraButton12 = 0x00004000,
        ExtraButton13 = 0x00008000,
        ExtraButton14 = 0x00010000,
        ExtraButton15 = 0x00020000,
        ExtraButton16 = 0x00040000,
        ExtraButton17 = 0x00080000,
        ExtraButton18 = 0x00100000,
        ExtraButton19 = 0x00200000,
        ExtraButton20 = 0x00400000,
        ExtraButton21 = 0x00800000,
        ExtraButton22 = 0x01000000,
        ExtraButton23 = 0x02000000,
        ExtraButton24 = 0x04000000,
        AllButtons = 0x07ffffff,
        MaxMouseButton = ExtraButton24,

        // 4 high-order bits remain available for future use (0x08000000 through 0x40000000).
        MouseButtonMask = 0xffffffff
    }

    [Flags]
    public enum NativeMouseEventFlag
    {
        MouseEventCreatedDoubleClick = 0x01,
        MouseEventFlagMask = 0xFF
    }

    public enum NativeMouseEventSource
    {
        MouseEventNotSynthesized,
        MouseEventSynthesizedBySystem,
        MouseEventSynthesizedByQt,
        MouseEventSynthesizedByApplication
    }

    /// <summary>
    ///     Equivalent to the "MouseEvent" struct found in C++.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMouseEvent
    {
        public NativeMouseEventType type;
        public NativeKeyboardModifiers modifiers;
        public NativeMouseButton button;
        public NativeMouseButton buttons;
        public NativeMouseEventFlag flags;
        public int globalX;
        public int globalY;
        public float x;
        public float y;
        public float screenX;
        public float screenY;
        public NativeMouseEventSource source;
        public float windowX;
        public float windowY;
    }

    public enum NativeWheelEventType
    {
        Wheel = 31
    }

    public enum NativeScrollPhase
    {
        NoScrollPhase = 0,
        ScrollBegin,
        ScrollUpdate,
        ScrollEnd,
        ScrollMomentum
    }

    /// <summary>
    ///     Equivalent to the "WheelEvent" struct found in C++.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeWheelEvent
    {
        public NativeWheelEventType type;
        public NativeKeyboardModifiers modifiers;
        public int angleDeltaX;
        public int angleDeltaY;
        public NativeMouseButton buttons;
        public float globalX;
        public float globalY;

        [MarshalAs(UnmanagedType.U1)]
        public bool inverted;

        public NativeScrollPhase phase;
        public int pixelDeltaX;
        public int pixelDeltaY;
        public float x;
        public float y;
        public NativeMouseEventSource source;
    }

    /// <summary>
    ///     Equivalent to the "KeyEvent" struct found in C++.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeKeyEvent
    {
        public NativeKeyEventType type;
        public NativeKeyboardModifiers modifiers;
        public int count;

        [MarshalAs(UnmanagedType.U1)]
        public bool isAutoRepeat;

        public int key;
        public uint nativeModifiers;
        public uint nativeScanCode;
        public uint nativeVirtualKey;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string text;
    }

    /// <summary>
    ///     Settings for the window on the native side.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeWindowConfig
    {
        public int MinWidth;
        public int MinHeight;
        public int Width;
        public int Height;
        public bool IsFullScreen;
    }

    [SuppressUnmanagedCodeSecurity]
    public delegate bool NativeMouseEventFilter(in NativeMouseEvent keyEvent);

    [SuppressUnmanagedCodeSecurity]
    public delegate bool NativeWheelEventFilter(in NativeWheelEvent keyEvent);

    [SuppressUnmanagedCodeSecurity]
    public delegate bool NativeKeyEventFilter(in NativeKeyEvent keyEvent);

    [SuppressUnmanagedCodeSecurity]
    public class NativeMainWindow : IMainWindow, IDisposable
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly Queue<Action> _taskQueue = new Queue<Action>();

        // Remember on which thread we create the UI in order to properly synchronize access
        private readonly Thread _uiThread;

        internal IntPtr UiHandle { get; private set; }

        public D3D11Device D3D11Device { get; private set; }



        public NativeMainWindow(WindowConfig windowConfig)
        {
            var callbacks = new UiCallbacks();
            callbacks.BeforeRendering = NativeDelegate.Create<Action>(InvokeOnBeforeRendering);
            callbacks.BeforeRenderPassRecording =
                NativeDelegate.Create<Action>(InvokeOnBeforeRenderPassRecording);
            callbacks.AfterRenderPassRecording =
                NativeDelegate.Create<Action>(InvokeOnAfterRenderPassRecording);
            callbacks.AfterRendering = NativeDelegate.Create<Action>(InvokeOnAfterRendering);
            callbacks.MouseEventFilter = NativeDelegate.Create<NativeMouseEventFilter>(InvokeMouseEventFilter);
            callbacks.WheelEventFilter = NativeDelegate.Create<NativeWheelEventFilter>(InvokeWheelEventFilter);
            callbacks.KeyEventFilter = NativeDelegate.Create<NativeKeyEventFilter>(InvokeKeyEventFilter);
            callbacks.OnClose = NativeDelegate.Create<Action>(InvokeOnClose);
            callbacks.DeviceCreated = NativeDelegate.Create<DeviceCallback>(nativeHandle =>
            {
                Trace.Assert(D3D11Device == null);
                D3D11Device = new D3D11Device(nativeHandle);
                InvokeOnDeviceCreated(D3D11Device);
            });
            callbacks.DeviceDestroyed = NativeDelegate.Create<DeviceCallback>(nativeHandle =>
            {
                Trace.Assert(D3D11Device != null);
                Trace.Assert(D3D11Device.NativePointer == nativeHandle);
                InvokeOnDeviceDestroyed(D3D11Device);
                D3D11Device.Dispose();
                D3D11Device = null;
            });

            UiHandle = ui_create(callbacks);
            _uiThread = Thread.CurrentThread;

            ui_set_title(UiHandle, "OpenTemple");
            ui_set_icon(UiHandle, "ui:app.ico");

            // Configures and shows the window
            NativeWindowConfig nativeConfig = default;
            nativeConfig.Width = windowConfig.Width;
            nativeConfig.Height = windowConfig.Height;
            nativeConfig.MinWidth = windowConfig.MinWidth;
            nativeConfig.MinHeight = windowConfig.MinHeight;
            nativeConfig.IsFullScreen = !windowConfig.Windowed;
            ui_set_config(UiHandle, ref nativeConfig);
        }

        private void CheckThrad()
        {
            if (Thread.CurrentThread != _uiThread)
            {
                throw new InvalidOperationException("Trying to access the UI from a non-UI thread!");
            }
        }

        public void Dispose()
        {
            CheckThrad();
            ui_destroy(UiHandle);
            UiHandle = IntPtr.Zero;
        }

        public IntPtr NativeHandle { get; }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
            // TODO
        }

        public event Action<Size> Resized;

        public event Action OnBeforeRendering;
        public event Action OnBeforeRenderPassRecording;
        public event Action OnAfterRenderPassRecording;
        public event Action OnAfterRendering;
        public event Action OnClose;
        public event Action<D3D11Device> OnDeviceCreated;
        public event Action<D3D11Device> OnDeviceDestroyed;

        public NativeMouseEventFilter MouseEventFilter { get; set; }

        public NativeWheelEventFilter WheelEventFilter { get; set; }

        public NativeKeyEventFilter KeyEventFilter { get; set; }

        public Task PostTask(Action work)
        {
            return PostTask(() =>
            {
                work();
                return true;
            });
        }

        public Task<T> PostTask<T>(Func<T> work)
        {
            var completionSource = new TaskCompletionSource<T>();

            void WorkOnUiThread()
            {
                try
                {
                    completionSource.SetResult(work());
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            }

            lock (_taskQueue)
            {
                _taskQueue.Enqueue(WorkOnUiThread);
            }

            return completionSource.Task;
        }

        public Item RootItem => QObjectBase.GetQObjectProxy<Item>(ui_get_root_item(UiHandle));

        public void BeginExternalCommands()
        {
            ui_begin_external_commands(UiHandle);
        }

        public void EndExternalCommands()
        {
            ui_end_external_commands(UiHandle);
        }

        public Size RenderTargetSize
        {
            get
            {
                ui_get_rendertarget_size(UiHandle, out var width, out var height);
                return new Size(width, height);
            }
        }

        public WindowConfig WindowConfig { get; set; }

        public void QueueUpdate()
        {
            ui_update(UiHandle);
        }

        public void Quit()
        {
            ui_quit(UiHandle);
        }

        public string BaseUrl
        {
            get => ui_get_baseurl(UiHandle);
            set => ui_set_baseurl(UiHandle, value);
        }

        public void HideCursor()
        {
            ui_hide_cursor(UiHandle);
        }

        public NativeCursor Cursor
        {
            set
            {
                Trace.Assert(value.Handle != null);
                ui_set_cursor(UiHandle, value.Handle);
            }
        }

        public async Task<T> LoadView<T>(string path) where T : Item
        {
            var result = await LoadViewNative(path);
            return QObjectBase.GetQObjectProxy<T>(result);
        }

        public Task<IntPtr> LoadViewNative(string path)
        {
            var (nativeTask, completionSource) = NativeCompletionSource.Create<IntPtr>();
            PostTask(() => ui_load_view(UiHandle, path, completionSource));
            return nativeTask;
        }

        public static void AddUiSearchPath(string path)
        {
            if (!path.EndsWith("/") || !path.EndsWith("\\"))
            {
                path += "/";
            }

            ui_add_search_path("ui", path);
        }

        private void InvokeOnBeforeRendering()
        {
            OnBeforeRendering?.Invoke();
        }

        private void InvokeOnBeforeRenderPassRecording()
        {
            OnBeforeRenderPassRecording?.Invoke();
        }

        private void InvokeOnAfterRenderPassRecording()
        {
            OnAfterRenderPassRecording?.Invoke();
        }

        private void InvokeOnAfterRendering()
        {
            OnAfterRendering?.Invoke();
        }

        private void InvokeOnClose()
        {
            OnClose?.Invoke();
        }

        private void InvokeOnDeviceCreated(D3D11Device device)
        {
            OnDeviceCreated?.Invoke(device);
        }

        private void InvokeOnDeviceDestroyed(D3D11Device device)
        {
            OnDeviceDestroyed?.Invoke(device);
        }

        private bool InvokeMouseEventFilter(in NativeMouseEvent evt)
        {
            if (MouseEventFilter != null)
            {
                return MouseEventFilter.Invoke(in evt);
            }

            return false;
        }

        private bool InvokeKeyEventFilter(in NativeKeyEvent evt)
        {
            if (KeyEventFilter != null)
            {
                return KeyEventFilter.Invoke(in evt);
            }

            return false;
        }

        private bool InvokeWheelEventFilter(in NativeWheelEvent evt)
        {
            if (WheelEventFilter != null)
            {
                return WheelEventFilter.Invoke(in evt);
            }

            return false;
        }

        public void Show()
        {
            ui_show(UiHandle);
        }

        public void ProcessEvents()
        {
            lock (_taskQueue)
            {
                while (_taskQueue.TryDequeue(out var work))
                {
                    try
                    {
                        work();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Work on main thread failed: {0}", e);
                    }
                }
            }

            ui_process_events();
        }

        [DllImport(OpenTempleLib.Path)]
        private static extern IntPtr ui_create(UiCallbacks callbacks);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_update(IntPtr ui);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_quit(IntPtr ui);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_process_events();

        [DllImport(OpenTempleLib.Path, CharSet = CharSet.Unicode)]
        private static extern string ui_get_baseurl(IntPtr ui);

        [DllImport(OpenTempleLib.Path, CharSet = CharSet.Unicode)]
        private static extern void ui_set_baseurl(IntPtr ui, string baseUrl);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_set_cursor(IntPtr ui, IntPtr cursor);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_hide_cursor(IntPtr ui);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_show(IntPtr ui);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_get_rendertarget_size(IntPtr ui, out int width, out int height);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_set_title(IntPtr ui, [MarshalAs(UnmanagedType.LPWStr)]
            string title);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_set_icon(IntPtr ui, [MarshalAs(UnmanagedType.LPWStr)]
            string path);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_set_config(IntPtr ui, ref NativeWindowConfig config);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_destroy(IntPtr handle);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_begin_external_commands(IntPtr handle);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_end_external_commands(IntPtr handle);

        [DllImport(OpenTempleLib.Path)]
        private static extern IntPtr ui_get_root_item(IntPtr uiHandle);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_add_search_path([MarshalAs(UnmanagedType.LPWStr)]
            string prefix,
            [MarshalAs(UnmanagedType.LPWStr)]
            string path);

        [DllImport(OpenTempleLib.Path)]
        private static extern void ui_load_view(IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string path,
            IntPtr completionSource
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct UiCallbacks
        {
            public NativeDelegate BeforeRendering;
            public NativeDelegate BeforeRenderPassRecording;
            public NativeDelegate AfterRenderPassRecording;
            public NativeDelegate AfterRendering;
            public NativeDelegate MouseEventFilter;
            public NativeDelegate WheelEventFilter;
            public NativeDelegate KeyEventFilter;
            public NativeDelegate OnClose;
            public NativeDelegate DeviceCreated;
            public NativeDelegate DeviceDestroyed;
        }

        private delegate void DeviceCallback(IntPtr device);
    }
}