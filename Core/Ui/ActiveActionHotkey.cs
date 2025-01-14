﻿using System;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

/// <summary>
/// A hotkey that is currently being listened for by some widget in the
/// UI tree.
/// </summary>
public readonly record struct ActiveActionHotkey(
    Hotkey Hotkey,
    Action<KeyboardEvent> Trigger,
    Func<bool> ActiveCondition,
    WidgetBase Owner
);
