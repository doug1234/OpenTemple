using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Inventory;

public class CharSheetInventoryUi : IDisposable
{
    [TempleDllLocation(0x101552C0)]
    [TempleDllLocation(0x10BEECC8)]
    public int BagIndex { get; set; }

    [TempleDllLocation(0x101552a0)]
    [TempleDllLocation(0x10155290)]
    [TempleDllLocation(0x10BEECB8)]
    public GameObject Container { get; set; }

    public WidgetBase Widget { get; }

    private WidgetButton _totalWeightLabel;
    private WidgetButton _totalWeightValue;
    private string _totalWeightLabelDefaultStyle;

    [TempleDllLocation(0x10BEECC0)]
    [TempleDllLocation(0x10155160)]
    [TempleDllLocation(0x10155170)]
    public GameObject? DraggedObject
    {
        get => Globals.UiManager.DraggedObject is DraggedItem draggedItem ? draggedItem.Item : null;
        set => Globals.UiManager.DraggedObject = value != null ? new DraggedItem(value) : null;
    }

    private static readonly Size SlotSize = new(65, 65);

    private readonly List<InventorySlotWidget> _slots = new();
    
    [TempleDllLocation(0x10BEECD0)]
    public bool IsVisible { get; private set; }

    [TempleDllLocation(0x101551a0)]
    public WidgetBase UseItemWidget { get; }

    [TempleDllLocation(0x101551b0)]
    [TempleDllLocation(0x102FB278)]
    public WidgetBase DropItemWidget { get; }
    
    [TempleDllLocation(0x10159530)]
    public CharSheetInventoryUi()
    {
        var widgetDoc = WidgetDoc.Load("ui/char_inventory.json");
        Widget = widgetDoc.GetRootContainer();

        UseItemWidget = widgetDoc.GetWidget("useItemButton");
        DropItemWidget = widgetDoc.GetWidget("dropItemButton");

        SetupTotalWeightWidgets(widgetDoc);

        var slotContainer = widgetDoc.GetContainer("slotsContainer");
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                var inventoryIdx = row * 6 + col;
                var slot = new InventorySlotWidget(inventoryIdx)
                {
                    X = 1 + col * (SlotSize.Width + 2),
                    Y = 1 + row * (SlotSize.Height + 2),
                    PixelSize = SlotSize
                };
                slotContainer.Add(slot);
                new ItemSlotBehavior(slot,
                    () => slot.CurrentItem,
                    () => UiSystems.CharSheet.CurrentCritter);
                _slots.Add(slot);
            }
        }
    }

    private void SetupTotalWeightWidgets(WidgetDoc widgetDoc)
    {
        _totalWeightLabel = widgetDoc.GetButton("totalWeightLabel");
        _totalWeightLabel.AddClickListener(ShowTotalWeightHelp);

        _totalWeightValue = widgetDoc.GetButton("totalWeightValue");
        _totalWeightLabelDefaultStyle = _totalWeightValue.GetStyle().Id;
        _totalWeightValue.AddClickListener(ShowTotalWeightHelp);
        _totalWeightValue.OnBeforeRender += UpdateTotalWeight;
    }

    private static void ShowTotalWeightHelp()
    {
        GameSystems.Help.ShowTopic("TAG_ADVENTURING_ENCUMBRANCE");
    }

    private void UpdateTotalWeight()
    {
        var critter = UiSystems.CharSheet.CurrentCritter;
        if (critter == null)
        {
            return;
        }

        // Tooltip strings
        var textNotEncumbered = UiSystems.Tooltip.GetString(124);
        var textEncumbered = UiSystems.Tooltip.GetString(125);
        var textLight = UiSystems.Tooltip.GetString(126);
        var textMedium = UiSystems.Tooltip.GetString(127);
        var textHeavy = UiSystems.Tooltip.GetString(128);
        var textMax = UiSystems.Tooltip.GetString(129);
        var textMin = UiSystems.Tooltip.GetString(134);
        var textOverburdened = UiSystems.Tooltip.GetString(135);

        var styleId = _totalWeightLabelDefaultStyle;
        string tooltipText = null;
        int weightLimit;
        if ((weightLimit = GameSystems.D20.D20QueryInt(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Light)) !=
            0)
        {
            tooltipText = $"@0({textNotEncumbered})@0\n\n";
            tooltipText += $"{textLight} ({textMin}/{textMax}): (0/{weightLimit})";
        }
        else if ((weightLimit =
                     GameSystems.D20.D20QueryInt(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium)) != 0)
        {
            styleId += "MediumLoad";

            var strength = critter.GetStat(Stat.strength);
            var capacity = GameSystems.Stat.GetCarryingCapacityByLoad(strength, EncumbranceType.LightLoad);
            tooltipText =
                $"{textEncumbered} @1({textMedium})@0\n\n{textMedium} ({textMin}/{textMax}): ({capacity}/{weightLimit})";
        }
        else if ((weightLimit =
                     GameSystems.D20.D20QueryInt(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy)) != 0)
        {
            styleId += "HeavyLoad";

            var strength = critter.GetStat(Stat.strength);
            var capacity = GameSystems.Stat.GetCarryingCapacityByLoad(strength, EncumbranceType.MediumLoad);
            tooltipText =
                $"{textEncumbered} @2({textHeavy})@0\n\n{textHeavy} ({textMin}/{textMax}): ({capacity}/{weightLimit})";
        }
        else if ((weightLimit =
                     GameSystems.D20.D20QueryInt(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened)) !=
                 0)
        {
            styleId += "Overburdened";

            var strength = critter.GetStat(Stat.strength);
            var capacity = GameSystems.Stat.GetCarryingCapacityByLoad(strength, EncumbranceType.HeavyLoad);
            tooltipText =
                $"{textEncumbered} @2({textOverburdened})@0\n\n{textHeavy} ({textMin}/{textMax}): ({capacity}/{weightLimit})";
        }

        if (_totalWeightValue.GetStyle().Id != styleId)
        {
            _totalWeightValue.SetStyle(Globals.WidgetButtonStyles.GetStyle(styleId));
        }

        var totalWeight = GameSystems.Item.GetTotalCarriedWeight(critter);
        _totalWeightValue.Text = totalWeight.ToString(CultureInfo.InvariantCulture);

        // Update the tooltip also
        _totalWeightLabel.TooltipText = tooltipText;
        _totalWeightValue.TooltipText = tooltipText;
    }

    [TempleDllLocation(0x10156e60)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x10155040)]
    public void Show(GameObject critter)
    {
        IsVisible = true;
        Widget.Visible = true;
        Stub.TODO();

        foreach (var slotWidget in _slots)
        {
            slotWidget.Inventory = critter;
        }
    }

    [TempleDllLocation(0x10156f00)]
    public void Hide()
    {
        IsVisible = false;
        Widget.Visible = false;
        Stub.TODO();
    }

    [TempleDllLocation(0x10156e50)]
    public void Reset()
    {
        BagIndex = 0;

        if (DraggedObject != null)
        {
            DraggedObject = null;
            Tig.Mouse.ClearDraggedIcon();
        }
    }

    [TempleDllLocation(0x10155170)]
    public void SetContainer(GameObject container)
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x10157030)]
    [TempleDllLocation(0x102FB27C)]
    public bool TryGetInventoryIdxForWidget(WidgetBase widget, out int invIdx)
    {
        if (widget is InventorySlotWidget inventorySlotWidget)
        {
            invIdx = inventorySlotWidget.InventoryIndex;
            return true;
        }

        invIdx = -1;
        return false;
    }

    [TempleDllLocation(0x10156df0)]
    public bool EquippingIsAction(GameObject obj)
    {
        if (obj.type == ObjectType.armor)
        {
            if (!obj.GetArmorFlags().IsShield())
            {
                if (!obj.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                {
                    return false;
                }
            }
        }

        return true;
    }

    [TempleDllLocation(0x10155260)]
    public bool IsCloseEnoughToTransferItem(GameObject from, GameObject to)
    {
        return from.DistanceToObjInFeet(to) < 40;
    }
}