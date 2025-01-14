﻿using System.Drawing;
using OpenTemple.Core.Ui.Events;
using SDL2;

namespace OpenTemple.Core.Platform;

/// <summary>
/// The root of the user interface component tree. It models the view of the UI that the
/// containing window has and allows it to forward events into the user interface.
/// </summary>
public interface IUiRoot
{
    void TextInput(string text)
    {
    }

    void KeyDown(SDL.SDL_Keycode virtualKey, SDL.SDL_Scancode physicalKey, KeyModifier modifiers, bool repeat)
    {
    }
    
    void KeyUp(SDL.SDL_Keycode virtualKey, SDL.SDL_Scancode physicalKey, KeyModifier modifiers)
    {
    }

    void MouseWheel(Point windowPos, PointF uiPos, float units)
    {
    }

    /// <summary>
    /// Mouse has entered the window.
    /// </summary>
    void MouseEnter()
    {
    }

    /// <summary>
    /// Mouse has left the window.
    /// </summary>
    void MouseLeave()
    {
    }

    void MouseMove(Point windowPos, PointF uiPos)
    {
    }

    void MouseDown(Point windowPos, PointF uiPos, MouseButton button)
    {
    }

    void MouseUp(Point windowPos, PointF uiPos, MouseButton button)
    {
    }

    void Tick()
    {
    }
}