using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using OpenTemple.Core;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Styles;
using SDL2;
using SixLabors.ImageSharp.Advanced;
using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace OpenTemple.Tests.TestUtils;

/// <summary>
/// Provides a running game instance that can render to an off-screen surface for tests.
/// </summary>
public class HeadlessGameHelper : IDisposable
{
    private readonly TempDirectory _userData = new();

    private static HeadlessGameHelper? _activeInstance;

    private TimePoint _currentTime = new(0);

    /// <summary>
    /// Text annotations for the current/next frame that will be part of screenshots / videos.
    /// </summary>
    private readonly List<(TimePoint, PointF, string)> _annotations = new();

    private HeadlessGame Game { get; }

    public HeadlessMainWindow Window { get; }

    public event Action? OnRenderFrame;

    public HeadlessGameHelper(bool withGameSystems = true, bool withUserInterface = true)
    {
        if (_activeInstance != null)
        {
            throw new InvalidOperationException(
                "There can only be a single HeadlessGameHelper active at the same time"
            );
        }

        var toeeDir = Environment.GetEnvironmentVariable("TOEE_DIR");
        if (toeeDir == null)
        {
            throw new NotSupportedException(
                "Cannot run a test based on real data because TOEE_DIR environment variable is not set."
            );
        }

        // In test cases and CLIs we want to see the error immediately
        ErrorReporting.DisableErrorReporting = true;

        LoggingSystem.ChangeLogger(new TestLogger());

        var options = new HeadlessGameOptions(toeeDir)
        {
            UserDataFolder = _userData.Path,
            WithGameSystems = withGameSystems,
            WithUserInterface = withUserInterface
        };

        Game = HeadlessGame.Start(options);

        Window = (HeadlessMainWindow) Tig.MainWindow;

        Globals.GameLoop = new GameLoop(
            Tig.EventLoop,
            Tig.RenderingDevice,
            Tig.DebugUI
        );

        TimePoint.SetFakeTime(_currentTime);
    }

    public void Dispose()
    {
        if (_activeInstance == this)
        {
            _activeInstance = null;
        }

        Game.Dispose();
        _userData.Dispose();
        TimePoint.ClearFakeTime();
    }

    public void RenderFrame()
    {
        Globals.GameLoop.RenderFrame();
        var now = TimePoint.Now;
        foreach (var (when, where, what) in _annotations)
        {
            if (now - when <= TimeSpan.FromSeconds(1))
            {
                Tig.RenderingDevice.TextEngine.RenderText(
                    new RectangleF(where, SizeF.Empty),
                    Globals.UiStyles.StyleResolver.DefaultStyle,
                    what
                );
            }
        }

        OnRenderFrame?.Invoke();
    }

    // Advance one "frame"
    public void AdvanceTimeAndRender(int time)
    {
        _currentTime = new TimePoint(_currentTime.Time + TimePoint.TicksPerMillisecond * time);
        TimePoint.SetFakeTime(_currentTime);
        Globals.GameLoop.RunOneIteration();
        OnRenderFrame?.Invoke();
    }

    public void RunUntil(Func<bool> condition, int maxSimulationTime = 1000)
    {
        if (condition())
        {
            return;
        }

        // When advancing time, simulate 30fps
        var advancePerRound = 1000 / 30;
        var totalAdvanced = 0;
        for (var i = 0; i < maxSimulationTime / advancePerRound; i++)
        {
            AdvanceTimeAndRender(advancePerRound);
            totalAdvanced += advancePerRound;
            if (condition())
            {
                return;
            }
        }

        if (totalAdvanced < maxSimulationTime)
        {
            AdvanceTimeAndRender(maxSimulationTime - totalAdvanced);
        }

        condition().Should().BeTrue("Condition didn't become true after running for " + totalAdvanced);
    }

    public void RunFor(int milliseconds)
    {
        // When advancing time, simulate 30fps
        var advancePerRound = 1000 / 30;
        var totalAdvanced = 0;
        for (var i = 0; i < milliseconds / advancePerRound; i++)
        {
            AdvanceTimeAndRender(advancePerRound);
            totalAdvanced += advancePerRound;
        }

        if (totalAdvanced < milliseconds)
        {
            AdvanceTimeAndRender(milliseconds - totalAdvanced);
        }
    }

    public void RunOneIteration()
    {
        Globals.GameLoop.RunOneIteration();
    }

    public Image<Bgra32> TakeScreenshot()
    {
        return TakeScreenshot(Tig.RenderingDevice);
    }

    public static Image<Bgra32> TakeScreenshot(RenderingDevice device)
    {
        Image<Bgra32> result = null;

        device.ReadRenderTarget(
            device.GetCurrentRenderTargetColorBuffer(),
            (data, stride, width, height) =>
            {
                // Boooo.... It doesn't support stride
                if (stride == width * 4)
                {
                    result = Image.LoadPixelData<Bgra32>(data, width, height);
                }
                else
                {
                    result = new Image<Bgra32>(width, height);
                    for (var i = 0; i < height; i++)
                    {
                        var destRow = MemoryMarshal.Cast<Bgra32, byte>(result.DangerousGetPixelRowMemory(i).Span);
                        data.Slice(i + stride, width * 4).CopyTo(destRow);
                    }
                }
            }
        );

        return result;
    }

    public PointF ToUiCanvas(Point screenPoint) => new Point(
        (int) (screenPoint.X / Window.UiScale),
        (int) (screenPoint.Y / Window.UiScale)
    );

    public Point FromUiCanvas(PointF uiPoint) => new Point(
        (int) (uiPoint.X * Window.UiScale),
        (int) (uiPoint.Y * Window.UiScale)
    );

    public void SendMouseDown(Point screenPoint, MouseButton button)
    {
        Window.UiRoot?.MouseDown(
            screenPoint,
            ToUiCanvas(screenPoint),
            button
        );
    }

    public void SendMouseUp(Point screenPoint, MouseButton button)
    {
        Window.UiRoot?.MouseUp(
            screenPoint,
            ToUiCanvas(screenPoint),
            button
        );
    }

    public void Click(Point screenPoint, MouseButton button = MouseButton.Left)
    {
        SendMouseDown(screenPoint, button);
        SendMouseUp(screenPoint, button);
    }

    public void ClickUi(float x, float y, MouseButton button = MouseButton.Left)
    {
        Click(FromUiCanvas(new PointF(x, y)), button);
    }

    public void AnnotateScreenPos(PointF screenPos, string text)
    {
        _annotations.Add((TimePoint.Now, screenPos, text));
    }

    public void AnnotateWorldPos(Vector3 worldPos, string text)
    {
        var screenPos = GameViews.Primary.WorldToScreen(worldPos);
        AnnotateScreenPos(new PointF(screenPos), text);
    }
}