
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog;

[DialogScript(66)]
public class Courier1Dialog : Courier1, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 6:
            case 7:
                originalScript = "game.global_flags[40] == 1";
                return GetGlobalFlag(40);
            case 13:
            case 14:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 7 and game.quests[17].state <= qs_accepted";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7 && GetQuestState(17) <= QuestState.Accepted;
            case 15:
            case 16:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 8 and game.quests[17].state <= qs_accepted";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8 && GetQuestState(17) <= QuestState.Accepted;
            case 17:
            case 18:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 8 and game.quests[17].state <= qs_accepted";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 8 && GetQuestState(17) <= QuestState.Accepted;
            case 21:
            case 22:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 6 and game.quests[17].state <= qs_accepted";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6 && GetQuestState(17) <= QuestState.Accepted;
            case 23:
            case 24:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 2 and game.quests[17].state <= qs_accepted";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 2 && GetQuestState(17) <= QuestState.Accepted;
            case 27:
            case 28:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 7 and game.quests[17].state <= qs_accepted";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 7 && GetQuestState(17) <= QuestState.Accepted;
            case 33:
            case 34:
            case 53:
            case 54:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 3";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 3;
            case 41:
            case 42:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 2";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 2;
            case 63:
            case 64:
                originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 10:
            case 20:
                originalScript = "game.quests[17].state = qs_mentioned";
                SetQuestState(17, QuestState.Mentioned);
                break;
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
            case 21:
            case 22:
            case 23:
            case 24:
            case 27:
            case 28:
                originalScript = "game.quests[17].state = qs_accepted";
                SetQuestState(17, QuestState.Accepted);
                break;
            case 60:
            case 75:
            case 80:
            case 90:
                originalScript = "game.quests[17].state = qs_completed";
                SetQuestState(17, QuestState.Completed);
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            case 13:
            case 14:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                return true;
            case 15:
            case 16:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                return true;
            case 17:
            case 18:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                return true;
            case 21:
            case 22:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                return true;
            case 23:
            case 24:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 2);
                return true;
            case 27:
            case 28:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                return true;
            case 33:
            case 34:
            case 53:
            case 54:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 3);
                return true;
            case 41:
            case 42:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 2);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}