using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.CharSheet.Portrait;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.PartyCreation.Systems;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.PartyCreation;

public class PCCreationUi : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    // TODO: Provide a way to customize. Co8 uses starting map 5107 for all alignments
    private static readonly Dictionary<Alignment, int> StartMaps = new()
    {
        {Alignment.TRUE_NEUTRAL, 5100},
        {Alignment.LAWFUL_NEUTRAL, 5103},
        {Alignment.CHAOTIC_NEUTRAL, 5104},
        {Alignment.NEUTRAL_GOOD, 5101},
        {Alignment.LAWFUL_GOOD, 5096},
        {Alignment.CHAOTIC_GOOD, 5097},
        {Alignment.NEUTRAL_EVIL, 5102},
        {Alignment.LAWFUL_EVIL, 5098},
        {Alignment.CHAOTIC_EVIL, 5099},
    };

    [TempleDllLocation(0x102f7d68)]
    private ChargenStage uiPcCreationActiveStageIdx;

    [TempleDllLocation(0x10bdb8e4)]
    private int dword_10BDB8E4 = 1000;

    [TempleDllLocation(0x102f7938)]
    private readonly List<IChargenSystem> chargenSystems = new();

    [TempleDllLocation(0x11e72f00)]
    private readonly CharEditorSelectionPacket charEdSelPkt = new();

    [TempleDllLocation(0x10bddd18)]
    private readonly WidgetContainer _mainWindow;

    [TempleDllLocation(0x11e741b4)]
    private readonly ScrollBox uiPcCreationScrollBox;

    private readonly StatBlockWidget _statBlockWidget;

    private readonly WidgetContent _activeButtonBorder;

    [TempleDllLocation(0x10BDB100)]
    private readonly WidgetText _descriptionLabel;

    private readonly MiniatureWidget _modelPreview;

    [TempleDllLocation(0x10bdd5d4)]
    private ChargenStage uiPcCreationStagesCompleted;

    [TempleDllLocation(0x102f7bf0)]
    public bool uiPcCreationIsHidden = true;

    [TempleDllLocation(0x1011b750)]
    public bool IsVisible => !uiPcCreationIsHidden;

    [TempleDllLocation(0x10bddd1c)]
    public int defaultPartyNum { get; set; }

    [TempleDllLocation(0x10bd3a48)]
    [TempleDllLocation(0x10111120)]
    public int startMap { get; set; }

    [TempleDllLocation(0x1011b730)]
    public bool IsPointBuy =>
        charEdSelPkt.isPointbuy
        && uiPcCreationActiveStageIdx == ChargenStage.Stats;

    [TempleDllLocation(0x11e741a0)]
    private GameObject? charEditorObjHnd;

    private readonly PartyAlignmentUi _partyAlignmentUi = new();

    private readonly IComparer<SpellStoreData> _spellPriorityComparer;

    [TempleDllLocation(0x10bddd20)]
    private bool ironmanSaveNamePopupActive;

    public GameObject EditedChar => charEditorObjHnd;

    [TempleDllLocation(0x10120420)]
    public PCCreationUi()
    {
        Stub.TODO();

        /*
         * TODO
         * uiPromptType3.idx = 3;
                  uiPromptType3.styleIdx = 0;
                  uiPromptType3.bodyText = dword_10BDB8D4;
                  uiPromptType3.textRect.x = 23;
                  uiPromptType3.textRect.y = 19;
                  uiPromptType3.textRect.width = 297;
                  uiPromptType3.textRect.height = 122;
                  UI_art_manager__get_image(3, 2, (ImgFile **)&uiPromptType3.image);
                  uiPromptType3.wndRect.x = (conf->width - 341) / 2;
                  uiPromptType3.wndRect.y = (conf->height - 158) / 2;
                  uiPromptType3.wndRect.width = 341;
                  uiPromptType3.wndRect.height = 158;
         */

        var doc = WidgetDoc.Load("ui/pc_creation/pc_creation_ui.json");
        _mainWindow = doc.GetRootContainer();
        _mainWindow.OnBeforeRender += BeforeRenderMainWindow;
        uiPcCreationScrollBox = new ScrollBox(new Rectangle(219, 295, 433, 148), new ScrollBoxSettings
        {
            TextArea = new Rectangle(3, 3, 415, 142),
            DontAutoScroll = true,
            Indent = 15,
            ScrollBarHeight = 148,
            ScrollBarPos = new Point(420, 0),
            Font = PredefinedFont.ARIAL_10
        });
        _mainWindow.Add(uiPcCreationScrollBox);

        _descriptionLabel = doc.GetTextContent("playerDescriptionLine");

        _activeButtonBorder = doc.GetContent("activeButtonBorder");
        _activeButtonBorder.Visible = false;

        // TODO: 0x1011c1c0 contains logic to animate / rotate the char model

        _partyAlignmentUi.OnCancel += Cancel;
        _partyAlignmentUi.OnConfirm += AlignmentSelected;

        _spellPriorityComparer = new SpellPriorityComparer(LoadSpellPriorities());

        chargenSystems.Add(new AbilityScoreSystem());
        chargenSystems.Add(new RaceSystem());
        chargenSystems.Add(new GenderSystem());
        chargenSystems.Add(new Systems.HeightSystem());
        chargenSystems.Add(new HairSystem());
        chargenSystems.Add(new ClassSystem());
        chargenSystems.Add(new AlignmentSystem());
        chargenSystems.Add(new Systems.DeitySystem());
        chargenSystems.Add(new ClassFeaturesSystem());
        chargenSystems.Add(new FeatsSystem());
        chargenSystems.Add(new SkillsSystem());
        chargenSystems.Add(new SpellsSystem());
        chargenSystems.Add(new Systems.PortraitSystem());
        chargenSystems.Add(new VoiceSystem());

        var stateButtonsContainer = doc.GetContainer("stateButtons");
        var y = 0f;
        foreach (var system in chargenSystems)
        {
            system.Container.Visible = false;
            _mainWindow.Add(system.Container);

            var stageButton = CreateStageButton(system);
            stageButton.Pos = new PointF(0, y);
            y += stageButton.ComputePreferredBorderAreaSize().Height;
            stateButtonsContainer.Add(stageButton);
        }

        _statBlockWidget = new StatBlockWidget();
        doc.GetContainer("statBlock").Add(_statBlockWidget.Container);

        var modelPreviewContainer = doc.GetContainer("modelPreview");
        _modelPreview = new MiniatureWidget();
        _modelPreview.Anchors.FillParent();
        modelPreviewContainer.Add(_modelPreview);
    }

    private WidgetButton CreateStageButton(IChargenSystem system)
    {
        var stageButton = new WidgetButton();
        stageButton.SetStyle("chargen-active-button");
        stageButton.Text = system.ButtonLabel;
        stageButton.OnBeforeRender += () =>
        {
            stageButton.SetActive(uiPcCreationActiveStageIdx == system.Stage);
            stageButton.Disabled = uiPcCreationStagesCompleted < system.Stage;

            // Render the blue outline for the active stage
            if (stageButton.IsActive())
            {
                var contentArea = stageButton.GetViewportBorderArea();
                _activeButtonBorder.SetBounds(new RectangleF(PointF.Empty, contentArea.Size));
                _activeButtonBorder.Render(contentArea.Location);
            }
        };
        stageButton.AddClickListener(() => ShowStage(system.Stage));
        stageButton.OnMouseEnter += msg => { ShowHelpTopic(system.HelpTopic); };
        stageButton.OnMouseLeave += msg => { system.ButtonExited(); };
        return stageButton;
    }

    [TempleDllLocation(0x1011ed80)]
    [TemplePlusLocation("ui_pc_creation.cpp")]
    private void BeforeRenderMainWindow()
    {
        var nextStage = uiPcCreationStagesCompleted + 1;
        // TODO TEMPORARY
        if ((int) nextStage >= chargenSystems.Count)
        {
            nextStage = (ChargenStage) (chargenSystems.Count - 1);
        }
        // TODO END TEMPORARY

        var stage = ChargenStages.First;
        for (; stage < nextStage; stage++)
        {
            if (!chargenSystems[(int) stage].CheckComplete())
            {
                break;
            }
        }

        if (stage != uiPcCreationStagesCompleted)
        {
            uiPcCreationStagesCompleted = stage;
            if (uiPcCreationActiveStageIdx > stage)
            {
                uiPcCreationActiveStageIdx = stage;
            }

            // reset the next stages
            ResetSystemsAfter(stage);
            UpdatePlayerDescription();
        }

        // TODO var wnd = uiManager->GetWindow(id);
        // TODO RenderHooks::RenderImgFile(temple::GetRef<ImgFile*>(0x10BDAFE0), wnd->x, wnd->y);
        UpdateStatBlock();
        // TODO UiRenderer::PushFont(PredefinedFont::PRIORY_12);

        // TODO UiRenderer::DrawTextInWidget(id,
        // TODO temple::GetRef<char[] >(0x10BDB100),
        // TODO temple::GetRef<TigRect>(0x10BDB004),
        // TODO temple::GetRef<TigTextStyle>(0x10BDDCC8));
        // TODO UiRenderer::PopFont();

        // TODO var renderCharModel = temple::GetRef<void(__cdecl)(int)>(0x1011C320);
        // TODO renderCharModel(id);
    }

    [TempleDllLocation(0x1011e740)]
    private void UpdateStatBlock()
    {
        _statBlockWidget.Update(charEdSelPkt, charEditorObjHnd, uiPcCreationStagesCompleted);
    }

    [TempleDllLocation(0x1011e160)]
    private static Dictionary<int, int> LoadSpellPriorities()
    {
        // Load spell var-memorization rules
        var spellPriorityRules = Tig.FS.ReadMesFile("rules/spell_priority.mes");
        var priorities = new Dictionary<int, int>(spellPriorityRules.Count);
        foreach (var (priority, spellName) in spellPriorityRules)
        {
            if (!GameSystems.Spell.GetSpellEnumByEnglishName(spellName, out var spellEnum))
            {
                Logger.Warn("Unknown spell name in spell priority list: '{0}'", spellName);
                continue;
            }

            priorities[spellEnum] = priority;
        }

        return priorities;
    }

    private void AlignmentSelected(Alignment alignment)
    {
        GameSystems.Party.PartyAlignment = alignment;
        UiSystems.PartyPool.Show(false);
    }

    [TempleDllLocation(0x1011ebc0)]
    public void Dispose()
    {
        _partyAlignmentUi.Dispose();

        Stub.TODO();
    }

    [TempleDllLocation(0x1011fdc0)]
    public void Start()
    {
        if (defaultPartyNum > 0)
        {
            for (var i = 0; i < defaultPartyNum; i++)
            {
                var protoId = 13100 + i;
                charEditorObjHnd = GameSystems.MapObject.CreateObject(protoId, new locXY(640, 480));
                GameSystems.Critter.GenerateHp(charEditorObjHnd);
                GameSystems.Party.AddToPCGroup(charEditorObjHnd);
                GameSystems.Item.GiveStartingEquipment(charEditorObjHnd);

                var model = charEditorObjHnd.GetOrCreateAnimHandle();
                charEditorObjHnd.UpdateRenderHeight(model);
                charEditorObjHnd.UpdateRadius(model);
            }

            UiChargenFinalize();
        }
        else
        {
            _partyAlignmentUi.Reset();
            StartNewParty();
        }
    }

    [TempleDllLocation(0x1011e200)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:216")]
    private void StartNewParty()
    {
        uiPcCreationIsHidden = false;
        UiPcCreationSystemsResetAll();

//           TODO uiPromptType3/*0x10bdd520*/.bodyText = (string )uiPcCreationText_SelectAPartyAlignment/*0x10bdb018*/;
//
//          TODO  WidgetSetHidden/*0x101f9100*/(uiPcCreationMainWndId/*0x10bdd690*/, 0);
//          TODO  WidgetBringToFront/*0x101f8e40*/(uiPcCreationMainWndId/*0x10bdd690*/);

        _partyAlignmentUi.Show();
        UiSystems.UtilityBar.Hide();
    }

    [TempleDllLocation(0x1011ddd0)]
    private void UiPcCreationSystemsResetAll()
    {
//          TODO  WidgetSetHidden/*0x101f9100*/(uiPcCreationWndId/*0x10bddd18*/, 1);

// TODO
//            v0 = &chargenSystems/*0x102f7938*/[0].hide;
//            do
//            {
//                if ( *v0 )
//                {
//                    (*v0)();
//                }
//                v0 += 11;
//            }
//            while ( (int)v0 < (int)&dword_102F7BB8/*0x102f7bb8*/ );
    }

    [TempleDllLocation(0x1011fc30)]
    public void UiChargenFinalize()
    {
        if (CheckPartySpareSpellSlotsForLevel1Caster())
        {
            if (defaultPartyNum == 0)
            {
                PcCreationAutoAddSpellsMemorized();
            }
        }

        if (!Globals.GameLib.IsIronmanGame || Globals.GameLib.IronmanSaveName != null)
        {
            UiSystems.PartyPool.UiPartypoolClose(false);
            UiSystems.PartyPool.ClearAvailable();

            MoveToStartMap();
            UiSystems.Party.UpdateAndShowMaybe();

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                GameSystems.Item.GiveStartingEquipment(partyMember);
                GameSystems.Spell.PendingSpellsToMemorized(partyMember);
                partyMember.ClearArray(obj_f.critter_spells_cast_idx);

                var model = partyMember.GetOrCreateAnimHandle();
                partyMember.UpdateRenderHeight(model);
                partyMember.UpdateRadius(model);
            }

            // Cancel all popups
            UiSystems.Popup.CloseAll();

            // TODO
//    hider = (void (**)(void))&chargenSystems/*0x102f7938*/[0].hide;
//    do
//    {
//      if ( *hider )
//      {
//        (*hider)();
//      }
//      hider += 11;
//    }
//    while ( (int)hider < (int)&dword_102F7BB8/*0x102f7bb8*/ );

            UiSystems.PCCreation.uiPcCreationIsHidden = true;
            UiSystems.UtilityBar.Show();
            UiSystems.MainMenu.Hide();
            UiSystems.Party.Update();
        }
        else if (!ironmanSaveNamePopupActive)
        {
            ironmanSaveNamePopupActive = true;
            UiSystems.Popup.RequestTextEntry("#{pc_creation:600}", "#{pc_creation:601}", ConfirmedIronmanSaveName);
        }
    }

    [TempleDllLocation(0x10111a80)]
    private void MoveToStartMap()
    {
        if (startMap != 0 && GameSystems.Map.IsValidMapId(startMap))
        {
            UiSystems.MainMenu.TransitionToMap(startMap);
        }
        else
        {
            var mapId = StartMaps[GameSystems.Party.PartyAlignment];
            UiSystems.MainMenu.TransitionToMap(mapId);
        }
    }

    [TempleDllLocation(0x1011bcb0)]
    private void ConfirmedIronmanSaveName(string name, bool sthg)
    {
        ironmanSaveNamePopupActive = false;
        if (!sthg && !string.IsNullOrEmpty(name))
        {
            Globals.GameLib.SetIronmanSaveName(name);
            UiChargenFinalize();
            GameSystems.Party.AddPartyMoney(0, 500, 0,
                0); // TODO: FISHY! Why does this not allow continously adding money to the party???
        }
        else
        {
            GameSystems.Party.AddPartyMoney(0, 500, 0,
                0); // TODO: FISHY! Why does this not allow continously adding money to the party???
        }
    }

    [TempleDllLocation(0x1011e8e0)]
    private bool CheckPartySpareSpellSlotsForLevel1Caster()
    {
        return GameSystems.Party.PlayerCharacters.Any(CheckSpareSpellSlotsForLevel1Caster);
    }

    [TempleDllLocation(0x1011e0b0)]
    private static bool CheckSpareSpellSlotsForLevel1Caster(GameObject player)
    {
        var bonusSpells = 0;
        var firstClass = (Stat) player.GetInt32(obj_f.critter_level_idx, 0);
        var numMemo = player.GetArrayLength(obj_f.critter_spells_memorized_idx);
        if (firstClass == Stat.level_cleric)
        {
            bonusSpells = 1;
        }
        else if (firstClass != Stat.level_druid && firstClass != Stat.level_wizard)
        {
            return false;
        }

        var numSpells = D20ClassSystem.GetNumSpellsFromClass(player, firstClass, 0, 1) + bonusSpells;
        if (numMemo >= numSpells + D20ClassSystem.GetNumSpellsFromClass(player, firstClass, 1, 1))
        {
            return false;
        }

        return true;
    }

    [TempleDllLocation(0x1011f290)]
    private void PcCreationAutoAddSpellsMemorized()
    {
        foreach (var player in GameSystems.Party.PlayerCharacters)
        {
            AutoAddSpellsMemorized(player);
        }
    }

    [TempleDllLocation(0x1011e920)]
    private void AutoAddSpellsMemorized(GameObject handle)
    {
        var firstClass = (Stat) handle.GetInt32(obj_f.critter_level_idx, 0);
        var spellClass = SpellSystem.GetSpellClass(firstClass);

        // Memorize 0th and 1st level spells
        for (var spellLevel = 0; spellLevel <= 1; spellLevel++)
        {
            // Build a prioritized list of cantrips that can be memorized
            var spellsKnown = handle.GetSpellArray(obj_f.critter_spells_known_idx)
                .Where(sp => sp.classCode == spellClass)
                .Where(sp => GameSystems.Spell.GetSpellLevelBySpellClass(sp.spellEnum, spellClass) == spellLevel)
                .ToList();
            spellsKnown.Sort(_spellPriorityComparer);

            // TODO: Needs rework by using the new slot based query system for spells memorized / castable
            var additionalMemos = 0;
            var initialMemoCount = handle.GetArrayLength(obj_f.critter_spells_memorized_idx);
            var canCastCount = D20ClassSystem.GetNumSpellsFromClass(handle, firstClass, spellLevel, 1);

            while (handle.GetArrayLength(obj_f.critter_spells_memorized_idx) < initialMemoCount + canCastCount)
            {
                var spellToMemo = spellsKnown[additionalMemos % spellsKnown.Count];
                var state = new SpellStoreState();
                state.spellStoreType = SpellStoreType.spellStoreMemorized;
                GameSystems.Spell.SpellMemorizedAdd(handle, spellToMemo.spellEnum, spellClass,
                    spellToMemo.spellLevel,
                    state);
                ++additionalMemos;
            }
        }

        // Handle domain spells for clerics
        if (firstClass == Stat.level_cleric)
        {
            var domain = (DomainId) handle.GetInt32(obj_f.critter_domain_1);
            var domainClassCode = SpellSystem.GetSpellClass(domain);

            var spellsKnown = handle.GetSpellArray(obj_f.critter_spells_known_idx)
                .Where(sp => sp.classCode == domainClassCode)
                .Where(sp => GameSystems.Spell.GetSpellLevelBySpellClass(sp.spellEnum, domainClassCode) != 0)
                .ToList();
            spellsKnown.Sort(_spellPriorityComparer);

            if (spellsKnown.Count > 0)
            {
                var spellToMemo = spellsKnown[0];
                var state = new SpellStoreState();
                state.spellStoreType = SpellStoreType.spellStoreMemorized;
                GameSystems.Spell.SpellMemorizedAdd(handle, spellToMemo.spellEnum, spellClass,
                    spellToMemo.spellLevel,
                    state);
            }
        }
    }

    [TempleDllLocation(0x1011e270)]
    public void Hide()
    {
        // TODO ScrollboxHideWindow/*0x1018cac0*/(uiPcCreationScrollBox/*0x11e741b4*/);
        // TODO UiPcCreationPortraitsMainHide/*0x10163030*/();
        UiSystems.Popup.CloseAll();
        // TODO WidgetSetHidden/*0x101f9100*/(uiPcCreationMainWndId/*0x10bdd690*/, 1);
        _partyAlignmentUi.Hide();
        UiPcCreationSystemsResetAll();
        uiPcCreationIsHidden = true;
        UiSystems.UtilityBar.Show();
    }

    private void Cancel()
    {
        ClearParty();
        Hide();
        UiSystems.MainMenu.Show(MainMenuPage.Difficulty);
    }

    [TempleDllLocation(0x1011b6f0)]
    public void ClearParty()
    {
        while (GameSystems.Party.PartySize > 0)
        {
            var player = GameSystems.Party.GetPartyGroupMemberN(0);
            GameSystems.Party.RemoveFromAllGroups(player);
        }

        // TODO PcPortraitsDeactivate/*0x10163060*/();
    }

    [TempleDllLocation(0x1011ec60)]
    public void Begin()
    {
        // TODO: This seems weird and kills encapsulation
        UiSystems.PartyPool.BeginAdventuringButton.Visible = true;
        UiSystems.PartyPool.BeginAdventuringButton.Disabled = false;

        StartNewParty();
        uiPcCreationActiveStageIdx = 0;
        charEditorObjHnd = null;
        dword_10BDB8E4 = 1000;

        foreach (var system in chargenSystems)
        {
            system.Reset(charEdSelPkt);
        }

        UiSystems.PCCreation._partyAlignmentUi.Hide();
        Globals.UiManager.AddWindow(_mainWindow);
        _mainWindow.BringToFront();

        ShowStage(ChargenStage.Stats);
        UpdatePlayerDescription();
    }

    [TempleDllLocation(0x1011c470)]
    public void UpdatePlayerDescription()
    {
        var desc = new ComplexInlineElement();

        // Alignment
        if (charEdSelPkt.alignment.HasValue)
        {
            var s = GameSystems.Stat.GetAlignmentName(charEdSelPkt.alignment.Value);
            desc.AppendContent(s);
            desc.AppendContent(" ");
        }

        // Gender
        if (charEdSelPkt.genderId.HasValue)
        {
            var s = GameSystems.Stat.GetGenderName(charEdSelPkt.genderId.Value);
            desc.AppendContent(s);
            desc.AppendContent(" ");
        }

        // Race
        if (charEdSelPkt.raceId.HasValue)
        {
            var s = GameSystems.Stat.GetRaceName(charEdSelPkt.raceId.Value);
            desc.AppendContent(s);
            desc.AppendContent(" ");
        }

        // Deity
        if (charEdSelPkt.deityId.HasValue)
        {
            // "Worships"
            desc.AppendTranslation("pc_creation:500").AddStyle(PartyCreationStyles.AccentColor);
            desc.AppendContent(GameSystems.Deity.GetName(charEdSelPkt.deityId.Value));
        }

        _descriptionLabel.Content = desc;
    }

    [TempleDllLocation(0x1011e3b0)]
    private void ShowStage(ChargenStage stage)
    {
        if (stage > uiPcCreationStagesCompleted)
        {
            return;
        }

        chargenSystems[(int) uiPcCreationActiveStageIdx].Hide();

        if (stage == uiPcCreationStagesCompleted && stage > ChargenStage.Stats)
        {
            for (var i = ChargenStage.Stats; i < stage; i++)
            {
                chargenSystems[(int) i].Finalize(charEdSelPkt, ref charEditorObjHnd);
            }
        }

        // This has to be set here because Finalize on the systems called above may replace the handle
        _modelPreview.Object = charEditorObjHnd;
        _modelPreview.Visible = charEditorObjHnd != null
                                && stage > ChargenStage.Gender
                                && (stage < ChargenStage.Portrait || charEdSelPkt.portraitId == 0);

        uiPcCreationActiveStageIdx = stage;

        if (stage <= ChargenStages.Last)
        {
            chargenSystems[(int) stage].Activate();
            // TODO: Probably no longer needed UiPcCreationStatTitleUpdateMeasures/*0x1011bd10*/(stage);
            var systemNameId = chargenSystems[(int) stage].HelpTopic;
            ShowHelpTopic(systemNameId);
            chargenSystems[(int) stage].Show();
        }
        else
        {
            UiSystems.PCCreation.UiPcCreationSystemsResetAll();
            if (dword_10BDB8E4 == 1000)
            {
                UiSystems.PartyPool.BeginAdventuringButton.Visible = false;
                UiSystems.PartyPool.Add(UiSystems.PCCreation.charEditorObjHnd);
                if (GameSystems.Map.GetCurrentMapId() == GameSystems.Map.GetMapIdByType(MapType.ShoppingMap))
                {
                    UiSystems.PartyPool.Show(false);
                }
                else
                {
                    UiSystems.PartyPool.Show(true);
                }

                _partyAlignmentUi.Hide();
            }
            else
            {
                // TODO UiSystems.Popup.OnClickButton(3, 0);
                // TODO UiSystems.Popup.UiPopupShow_Impl(&uiPromptType3 /*0x10bdd520*/, 3, 0);
                GameSystems.Party.AddToPCGroup(UiSystems.PCCreation.charEditorObjHnd);
                GameSystems.Item.GiveStartingEquipment(UiSystems.PCCreation.charEditorObjHnd);
                // TODO PcPortraitsButtonActivateNext /*0x10163090*/();
            }

            UiSystems.PCCreation.charEditorObjHnd = null;
            // TODO ScrollboxHideWindow /*0x1018cac0*/(uiPcCreationScrollBox /*0x11e741b4*/);
        }
    }

    [VisibleForScripting]
    public void SkipToStageForTesting(ChargenStage stage, Dictionary<string, object> props)
    {
        while (uiPcCreationStagesCompleted < stage &&
               chargenSystems[(int) uiPcCreationStagesCompleted].CompleteForTesting(props))
        {
            BeforeRenderMainWindow();
            ShowStage(uiPcCreationStagesCompleted);
        }
    }

    [TempleDllLocation(0x1011b890)]
    internal void ShowHelpTopic(string systemName)
    {
        if (GameSystems.Help.TryGetTopic(systemName, out var topic))
        {
            uiPcCreationScrollBox.DontAutoScroll = true;
            uiPcCreationScrollBox.Indent = 15;
            uiPcCreationScrollBox.SetEntries(new List<D20RollHistoryLine>
            {
                D20RollHistoryLine.Create(topic.Title),
                D20RollHistoryLine.Create("\n"),
                new(topic.Text, topic.Links)
            });
        }
        else
        {
            uiPcCreationScrollBox.ClearLines();
        }
    }

    [TempleDllLocation(0x1011bae0)]
    internal void ShowHelpText(string text)
    {
        uiPcCreationScrollBox.DontAutoScroll = true;
        uiPcCreationScrollBox.Indent = 15;
        uiPcCreationScrollBox.SetEntries(new List<D20RollHistoryLine>
        {
            D20RollHistoryLine.Create(text)
        });
    }

    [TempleDllLocation(0x1011bc70)]
    internal void ResetSystemsAfter(ChargenStage stage)
    {
        for (var i = (int) stage + 1; i < chargenSystems.Count; i++)
        {
            chargenSystems[i].Reset(charEdSelPkt);
        }
    }
}