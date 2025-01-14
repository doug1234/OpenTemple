using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using static SDL2.SDL;

namespace OpenTemple.Core.Ui.InGame;

public class InGameUi : IDisposable, ISaveGameAwareUi, IResetAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    // Is set to the widget that we capture input for during drag-selection.
    private WidgetBase? _capturedInputWidget;

    private static readonly TimeSpan DoubleClickWindow = TimeSpan.FromMilliseconds(200);

    [TempleDllLocation(0x10BD3B5C)]
    private bool _normalLmbClicked;

    [TempleDllLocation(0x10BD3B60)]
    private bool _uiDragSelectOn;

    [TempleDllLocation(0x10BD3AC8)]
    private bool uiDragViewport;

    [TempleDllLocation(0x10BD3AD8)]
    private GameObject? mouseDragTgt;

    [TempleDllLocation(0x10BD3AE0)]
    private GameObject? qword_10BD3AE0;

    [TempleDllLocation(0x10BD3ACC)]
    private float uiDragSelectXMax = 0;

    [TempleDllLocation(0x10BD3AD0)]
    private float uiDragSelectYMax = 0;

    private TimePoint _lastMoveCommandTime;

    private PointF _lastMoveCommandLocation;

    [TempleDllLocation(0x10112e70)]
    public InGameUi()
    {
    }

    [TempleDllLocation(0x10112eb0)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x10112f10)]
    public void ResetInput()
    {
        _uiDragSelectOn = false;
        _normalLmbClicked = false;
        ReleaseMouseCapture();

        UiSystems.InGameSelect.FocusClear();
    }

    public void SaveGame(SavedUiState savedState)
    {
    }

    [TempleDllLocation(0x101140c0)]
    public void LoadGame(SavedUiState savedState)
    {
        UiSystems.InGameSelect.FocusClear();
    }

    [TempleDllLocation(0x10112ec0)]
    public void ResizeViewport(Size size)
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x101140b0)]
    public void Reset()
    {
    }
    
    [TempleDllLocation(0x10113ce0)]
    public void CenterOnPartyLeader()
    {
        var leader = GameSystems.Party.GetConsciousLeader();
        if (leader != null)
        {
            var location = leader.GetLocation();
            GameSystems.Location.CenterOn(location.locx, location.locy);
        }
    }

    [TempleDllLocation(0x10113370)]
    public void CenterOnParty()
    {
        GameObject centerOn;
        if (GameSystems.Combat.IsCombatActive())
        {
            centerOn = GameSystems.D20.Initiative.CurrentActor;
            if (centerOn == null)
            {
                return;
            }
        }
        else
        {
            centerOn = GameSystems.Party.GetConsciousLeader();
            if (centerOn == null)
            {
                centerOn = GameSystems.Party.GetLeader();
                if (centerOn == null)
                {
                    return;
                }
            }
        }

        var loc = centerOn.GetLocation();
        GameSystems.Scroll.CenterOnSmooth(loc.locx, loc.locy);
    }

    [TempleDllLocation(0x10113f30)]
    private GameObject? GetMouseTarget(IGameViewport viewport, float x, float y)
    {
        // Pick the object using the screen coordinates first
        var mousedOver = PickObject(viewport, x, y, GameRaycastFlags.HITTEST_3D);
        if (mousedOver == null)
        {
            return null;
        }

        if (GameSystems.MapObject.IsUntargetable(mousedOver))
        {
            return null;
        }

        if (mousedOver.IsCritter() && GameSystems.Critter.IsConcealed(mousedOver))
        {
            return null;
        }

        return mousedOver;
    }

    private GameObject? PickObject(IGameViewport viewport, float x, float y, GameRaycastFlags flags)
    {
        if (GameSystems.Raycast.PickObjectOnScreen(viewport, x, y, out var result, flags))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    [TempleDllLocation(0x10113010)]
    private void PartySelectedStandUpAndMoveToPosition(LocAndOffsets loc, bool walkFlag)
    {
        // make Prone party members do a Stand Up action first if they're prone
        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (!GameSystems.D20.D20Query(partyMember, D20DispatcherKey.QUE_Prone))
            {
                continue;
            }

            GameSystems.D20.Actions.TurnBasedStatusInit(partyMember);
            GameSystems.D20.Actions.CurSeqReset(partyMember);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.STAND_UP, 0);
            GameSystems.D20.Actions.ActionAddToSeq();
            GameSystems.D20.Actions.sequencePerform();
        }

        GameSystems.Formation.PartySelectedFormationMoveToPosition(loc, walkFlag);
    }

    [TempleDllLocation(0x10114af0)]
    private void HandleNormalLeftMouseReleased(IGameViewport viewport, MouseEvent e)
    {
        if (UiSystems.CharSheet.HasCurrentCritter)
        {
            return;
        }

        GameSystems.Anim.StartFidgetTimer();
        UiSystems.InGameSelect.FocusClear();
        if (!_normalLmbClicked)
            return;
        _normalLmbClicked = false;
        if (_uiDragSelectOn)
        {
            if (!e.IsShiftHeld)
            {
                GameSystems.Party.ClearSelection();
            }

            var left = Math.Min(uiDragSelectXMax, e.X);
            var top = Math.Min(uiDragSelectYMax, e.Y);
            var right = Math.Max(uiDragSelectXMax, e.X);
            var bottom = Math.Max(uiDragSelectYMax, e.Y);
            var rect = RectangleF.FromLTRB(left, top, right, bottom);
            UiSystems.InGameSelect.SelectInRectangle(viewport, rect);
            _uiDragSelectOn = false;
            ReleaseMouseCapture();
            return;
        }

        var mouseTgt = GetMouseTarget(viewport, e.X, e.Y);
        if (mouseTgt == null)
        {
            if (GameSystems.Party.GetConsciousLeader() == null)
            {
                return;
            }

            var worldPos = viewport.ScreenToTile(e.X, e.Y);

            // This is a new feature
            var centerScreen = TimePoint.Now - _lastMoveCommandTime < DoubleClickWindow;
            if (centerScreen)
            {
                GameSystems.Scroll.CenterOnSmooth(worldPos.location.locx, worldPos.location.locy);
                return;
            }

            _lastMoveCommandTime = TimePoint.Now;
            _lastMoveCommandLocation = e.Pos;

            if (!e.IsShiftHeld)
            {
                var alwaysRun = Globals.Config.AlwaysRun;
                PartySelectedStandUpAndMoveToPosition(worldPos, !alwaysRun);
            }

            if (uiDragViewport || mouseDragTgt != null)
            {
                // TODO I think this is always true
                return;
            }
        }

        var tgtIsInParty = GameSystems.Party.IsInParty(mouseTgt);
        if (tgtIsInParty && UiSystems.CharSheet.State != CharInventoryState.LevelUp
                         && !GameSystems.D20.Actions.SeqPickerHasTargetingType())
        {
            var currentChar = UiSystems.CharSheet.CurrentCritter;
            if (currentChar != null && mouseTgt != currentChar)
            {
                var state = UiSystems.CharSheet.Looting.GetLootingState();
                UiSystems.CharSheet.ShowInState(state, mouseTgt);
            }

            if (e.IsShiftHeld)
            {
                if (GameSystems.Party.IsSelected(mouseTgt))
                {
                    GameSystems.Party.RemoveFromSelection(mouseTgt);
                    return;
                }
            }
            else
            {
                GameSystems.Party.ClearSelection();
            }

            GameSystems.Party.AddToSelection(mouseTgt);
            return;
        }

        TriggerClickActionOnObject(mouseTgt);
    }

    private void CaptureMouse(IGameViewport viewport)
    {
        // Release previous mouse capture if applicable
        ReleaseMouseCapture();

        if (viewport is WidgetBase widget && widget.SetMouseCapture())
        {
            _capturedInputWidget = widget;
        }
    }

    private void ReleaseMouseCapture()
    {
        _capturedInputWidget?.ReleaseMouseCapture();
        _capturedInputWidget = null;
    }

    [TempleDllLocation(0x101140F0)]
    private void TriggerClickActionOnObject(GameObject clickedObj)
    {
        bool playVoiceConfirmation = false;
        AnimGoalType goalType;
        var targetTile = locXY.Zero;
        var useObjectGoal = false;
        var requirePlayerCharacter = false;

        var partyLeader = GameSystems.Party.GetConsciousLeader();
        if (partyLeader == null)
        {
            partyLeader = GameSystems.Party.GetPCGroupMemberN(0);
            GameSystems.Party.AddToSelection(partyLeader);
            partyLeader = GameSystems.Party.GetPCGroupMemberN(0);
            if (partyLeader == null)
            {
                return;
            }
        }

        if (partyLeader.IsCritter() && partyLeader.IsDeadOrUnconscious())
        {
            return;
        }

        GameSystems.Anim.StartFidgetTimer();
        var partyLeaderSpellFlags = partyLeader.GetSpellFlags();
        var partyLeaderCritterFlags = partyLeader.GetCritterFlags();
        if (partyLeaderSpellFlags.HasFlag(SpellFlag.STONED))
        {
            return;
        }

        if ((partyLeaderCritterFlags & (CritterFlag.PARALYZED | CritterFlag.STUNNED)) != default)
        {
            return;
        }

        if (!GameSystems.Combat.IsCombatModeActive(partyLeader) &&
            GameSystems.Anim.HasAttackAnim(clickedObj, partyLeader))
        {
            GameSystems.Combat.EnterCombat(partyLeader);
        }

        if (GameSystems.Combat.IsCombatModeActive(partyLeader))
        {
            return;
        }

        switch (clickedObj.type)
        {
            case ObjectType.portal:
                if ((partyLeaderSpellFlags & SpellFlag.POLYMORPHED) != 0)
                {
                    return;
                }

                goalType = AnimGoalType.use_object;
                useObjectGoal = true;
                playVoiceConfirmation = true;
                break;
            case ObjectType.container:
            case ObjectType.weapon:
            case ObjectType.ammo:
            case ObjectType.armor:
            case ObjectType.money:
            case ObjectType.food:
            case ObjectType.scroll:
            case ObjectType.key:
            case ObjectType.written:
            case ObjectType.generic:
            case ObjectType.bag:
                PerformDefaultAction(partyLeader, clickedObj);
                return;
            case ObjectType.scenery:
                goalType = AnimGoalType.use_object;
                useObjectGoal = true;
                playVoiceConfirmation = true;
                var jumppointId = clickedObj.GetInt32(obj_f.scenery_teleport_to);
                if (jumppointId != 0 && GameSystems.JumpPoint.TryGet(jumppointId, out _, out _, out _))
                {
                    requirePlayerCharacter = true;
                }

                break;
            default:
                return;
            case ObjectType.pc:
            case ObjectType.npc:
                if (GameSystems.D20.Actions.SeqPickerHasTargetingType())
                {
                    PerformDefaultAction(partyLeader, clickedObj);
                    return;
                }

                if (!clickedObj.IsDeadOrUnconscious() ||
                    clickedObj.GetSpellFlags().HasFlag(SpellFlag.SPOKEN_WITH_DEAD))
                {
                    if (GameSystems.Party.IsInParty(clickedObj))
                    {
                        return;
                    }

                    goalType = AnimGoalType.talk;
                }
                else
                {
                    if (partyLeaderSpellFlags.HasFlag(SpellFlag.POLYMORPHED))
                    {
                        return;
                    }

                    if (Tig.Keyboard.IsAltPressed)
                    {
                        MoveCorpseToPlayer(partyLeader, clickedObj);
                        return;
                    }

                    goalType = AnimGoalType.use_container;
                    useObjectGoal = true;
                    playVoiceConfirmation = true;
                }

                break;
            case ObjectType.trap:
                goalType = AnimGoalType.move_to_tile;
                targetTile = clickedObj.GetLocation();
                playVoiceConfirmation = true;
                break;
        }

        GameObject closestPartyMember = null;
        var closestDistance = float.MaxValue;

        // Find the closest selected party member to the clicked object
        foreach (var partyMember in GameSystems.Party.Selected)
        {
            if (!partyMember.IsDeadOrUnconscious())
            {
                if (partyMember.IsPC() || !requirePlayerCharacter)
                {
                    var dist = clickedObj.DistanceToObjInFeet(partyMember);
                    if (closestPartyMember == null || dist < closestDistance)
                    {
                        closestPartyMember = partyMember;
                        closestDistance = dist;
                    }
                }
            }
        }

        if (closestPartyMember == null)
        {
            return;
        }

        PushClickActionGoal(closestPartyMember, goalType, clickedObj, targetTile, useObjectGoal);

        if (playVoiceConfirmation)
        {
            closestPartyMember.PlayVoiceConfirmationSound();
        }
    }

    private static void PerformDefaultAction(GameObject critter, GameObject target)
    {
        if (GameSystems.D20.Actions.IsCurrentlyPerforming(critter))
        {
            return;
        }

        GameSystems.D20.Actions.TurnBasedStatusInit(critter);
        GameSystems.D20.Actions.GlobD20ActnInit();
        GameSystems.D20.Actions.ActionTypeAutomatedSelection(target);
        GameSystems.D20.Actions.GlobD20ActnSetTarget(target, null);
        GameSystems.D20.Actions.ActionAddToSeq();
        GameSystems.D20.Actions.sequencePerform();
        GameSystems.D20.Actions.SeqPickerTargetingTypeReset();
        critter.PlayVoiceConfirmationSound();
    }

    // This function seems borked and previously checked for object type
    // but essentially it is ONLY used for corpses.
    [TempleDllLocation(0x100264A0)]
    private void MoveCorpseToPlayer(GameObject partyLeader, GameObject clickedObj)
    {
        var objLoc = partyLeader.GetLocation();
        var tgtLoc = clickedObj.GetLocation();
        if (objLoc.EstimateDistance(tgtLoc) <= 1)
        {
            GameSystems.MapObject.Move(clickedObj, new LocAndOffsets(objLoc));
        }
    }

    [TempleDllLocation(0x10113470)]
    private void PushClickActionGoal(GameObject animObj, AnimGoalType animGoalType, GameObject targetObj,
        locXY targetTile, bool a11)
    {
        var goal = new AnimSlotGoalStackEntry(animObj, animGoalType);

        goal.target.obj = targetObj;
        if (targetTile != locXY.Zero)
        {
            goal.targetTile.location = new LocAndOffsets(targetTile);
        }

        bool IsActionPaused()
        {
            // I take bets that this is never ever used
            return (animObj.GetCritterFlags2() & CritterFlag2.ACTION0_PAUSED) != 0;
        }

        if (animGoalType is AnimGoalType.attack or AnimGoalType.attempt_attack &&
            IsActionPaused())
            return;

        // When auto attack is configured, things seem MUCH simpler
        if (GameSystems.Combat.IsGameConfigAutoAttack())
        {
            if (GameSystems.Anim.GetSlotForGoalAndObjs(animObj, goal).IsNull
                && GameSystems.Anim.Interrupt(animObj, AnimGoalPriority.AGP_3, false)
                && GameSystems.Anim.PushGoal(goal, out var slotId))
            {
                SetGoalFlags(animObj, slotId, a11);
            }
        }
        else
        {
            var slotId = GameSystems.Anim.GetFirstRunSlotId(animObj);
            if (slotId.IsNull)
            {
                if (!GameSystems.Anim.PushGoal(goal, out slotId))
                {
                    return;
                }
            }
            else if (GameSystems.Combat.IsCombatActive())
            {
                if (GameSystems.Anim.IsRunningGoal(animObj, AnimGoalType.anim_fidget, out var runId) &&
                    slotId == runId)
                {
                    PushGoalWithExistingSlot(animObj, ref slotId, goal);
                    return;
                }
            }
            else
            {
                if (GameSystems.Anim.GetSlotForGoalAndObjs(animObj, goal).IsNull
                    || (animGoalType != AnimGoalType.attack && animGoalType != AnimGoalType.attempt_attack))
                {
                    if (GameSystems.Anim.Interrupt(animObj, AnimGoalPriority.AGP_3, false))
                    {
                        if (!GameSystems.Anim.PushGoal(goal, out slotId))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (!PushGoalWithExistingSlot(animObj, ref slotId, goal))
                    {
                        return;
                    }
                }
            }

            SetGoalFlags(animObj, slotId, a11);
        }
    }

    private bool PushGoalWithExistingSlot(GameObject animObj, ref AnimSlotId slotId,
        AnimSlotGoalStackEntry goal)
    {
        if (GameSystems.Anim.GetGoalSubslotsInUse(slotId) < 4)
        {
            if (GameSystems.Anim.IsAnimatingForever(slotId))
            {
                if (GameSystems.Anim.Interrupt(animObj, AnimGoalPriority.AGP_3, false))
                {
                    if (!GameSystems.Anim.PushGoal(goal, out slotId))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!GameSystems.Anim.AddSubGoal(slotId, goal))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void SetGoalFlags(GameObject animObj, AnimSlotId slotId, bool a11)
    {
        if (Tig.Keyboard.IsShiftPressed)
        {
            GameSystems.Anim.TurnOn100(slotId);
        }
        else if (GameSystems.Anim.ShouldRun(animObj))
        {
            GameSystems.Anim.TurnOnRunning(slotId);
        }

        if (a11)
        {
            GameSystems.Anim.TurnOn4000(slotId);
        }
    }

    [TempleDllLocation(0x101148b0)]
    private void HandleNormalLeftMouseDragHandler(IGameViewport viewport, MouseEvent e)
    {
        var target = GetMouseTarget(viewport, e.X, e.Y);

        if (e.Type == UiEventType.MouseDown)
        {
            if (_normalLmbClicked)
                return;
            mouseDragTgt = target;
            _normalLmbClicked = true;
            uiDragSelectXMax = e.X;
            uiDragSelectYMax = e.Y;
            qword_10BD3AE0 = target;
            uiDragViewport = target == null;
        }
        else if (e.Type == UiEventType.MouseMove && _normalLmbClicked)
        {
            if (!UiSystems.Dialog.IsConversationOngoing && !UiSystems.RadialMenu.IsOpen)
            {
                if (GetEstimatedSelectionSize(e.X, e.Y) > 25)
                {
                    _uiDragSelectOn = true;
                    CaptureMouse(viewport);
                }
            }

            if (_uiDragSelectOn)
            {
                UiSystems.InGameSelect.FocusClear();
                UiSystems.InGameSelect.SetFocusToRect(viewport, uiDragSelectXMax, uiDragSelectYMax, e.X, e.Y);
            }

            if (target != null
                && !uiDragViewport
                && target == mouseDragTgt
                && UiSystems.CharSheet.State != CharInventoryState.LevelUp
                && (UiSystems.CharSheet.Looting.GetLootingState() != CharInventoryState.Looting
                    && UiSystems.CharSheet.Looting.GetLootingState() != CharInventoryState.Bartering
                    || UiSystems.CharSheet.Looting.LootingContainer != target
                    || target.type.IsEquipment()))
            {
                UiSystems.InGameSelect.AddToFocusGroup(target);
                UiSystems.Party.ForcePressed = target;
            }
        }
    }

    [TempleDllLocation(0x10113C80)]
    private float GetEstimatedSelectionSize(float x, float y)
    {
        return MathF.Abs(uiDragSelectXMax - x) + MathF.Abs(uiDragSelectYMax - y);
    }

    [TempleDllLocation(0x10114d90)]
    private void HandleNormalRightMouseButton(IGameViewport viewport, MouseEvent e)
    {
        var partyLeader = GameSystems.Party.GetConsciousLeader();
        if (GameSystems.Party.IsPlayerControlled(partyLeader))
        {
            if (GameSystems.D20.Actions.SeqPickerHasTargetingType())
            {
                GameSystems.D20.Actions.SeqPickerTargetingTypeReset();
            }
            else
            {
                Logger.Info("state_default_process_mouse_right_down");
                UiSystems.RadialMenu.SpawnFromMouse(viewport, e);
            }
        }
    }

    [TempleDllLocation(0x10114690)]
    private void HandleMouseMove(IGameViewport viewport, MouseEvent e)
    {
        UiSystems.Party.ForcePressed = null;
        UiSystems.Party.ForceHovered = null;
        UiSystems.InGameSelect.Focus = null;
        UiSystems.InGameSelect.ClearFocusGroup();

        if (GameSystems.Map.IsClearingMap())
        {
            return;
        }

        var mouseTarget = GetMouseTarget(viewport, e.X, e.Y);
        if (mouseTarget == null)
        {
            return;
        }

        if (UiSystems.CharSheet.HasCurrentCritter
            || UiSystems.TownMap.IsVisible
            || UiSystems.Logbook.IsVisible
            || UiSystems.CharSheet.State == CharInventoryState.LevelUp)
        {
            return;
        }

        e.StopImmediatePropagation();
        
        var type = mouseTarget.type;
        if (_normalLmbClicked)
        {
            if (!uiDragViewport && mouseTarget == mouseDragTgt)
            {
                UiSystems.InGameSelect.AddToFocusGroup(mouseTarget);
                UiSystems.Party.ForcePressed = mouseTarget;
            }
        }
        else
        {
            if (type.IsCritter()
                || type.IsEquipment()
                || type == ObjectType.container
                || type == ObjectType.portal
                || mouseTarget.ProtoId == WellKnownProtos.GuestBook)
            {
                UiSystems.InGameSelect.Focus = mouseTarget;
            }
            else if (type == ObjectType.scenery)
            {
                var teleportTo = mouseTarget.GetInt32(obj_f.scenery_teleport_to);
                if (teleportTo != 0)
                {
                    UiSystems.InGameSelect.Focus = mouseTarget;
                }
            }

            if (type.IsCritter() && GameSystems.Party.IsInParty(mouseTarget))
            {
                UiSystems.Party.ForceHovered = mouseTarget;
            }
        }
    }

    [TempleDllLocation(0x10112fc0)]
    public ActionCursor? GetCursor()
    {
        var cursor = UiSystems.Combat.Initiative.GetCursor();
        if (cursor.HasValue)
        {
            return cursor;
        }

        cursor = UiSystems.Party.GetCursor();
        if (cursor.HasValue)
        {
            return cursor;
        }

        cursor = UiSystems.CharSheet.Looting.GetCursor();
        if (cursor.HasValue)
        {
            return cursor;
        }

        cursor = UiSystems.RadialMenu.GetCursor();
        if (cursor.HasValue)
        {
            return cursor;
        }

        cursor = UiSystems.HelpManager.GetCursor();
        if (cursor.HasValue)
        {
            return cursor;
        }

        return null;
    }

    public void AddEventListeners<T>(T viewport) where T : WidgetBase, IGameViewport
    {
        viewport.OnMouseMove += e =>
        {
            if (!GameSystems.Combat.IsCombatActive() && e.IsLeftButtonHeld)
            {
                HandleNormalLeftMouseDragHandler(viewport, e);
                e.StopImmediatePropagation();
                return;
            }
            
            HandleMouseMove(viewport, e);
        };
        viewport.OnMouseDown += e =>
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                if (e.Button == MouseButton.Left)
                {
                    HandleNormalLeftMouseDragHandler(viewport, e);
                    e.StopImmediatePropagation();
                }
                else if (e.Button == MouseButton.Right)
                {
                    HandleNormalRightMouseButton(viewport, e);
                }
            }
        };
        viewport.OnMouseUp += e =>
        {
            if (!GameSystems.Combat.IsCombatActive() && e.Button == MouseButton.Left)
            {
                e.StopImmediatePropagation();
                HandleNormalLeftMouseReleased(viewport, e);
            }
        };
    }
}
