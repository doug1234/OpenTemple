
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

[DialogScript(7)]
public class CarpenterBrotherDialog : CarpenterBrother, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
            case 121:
            case 122:
                originalScript = "not npc.has_met( pc )";
                return !npc.HasMet(pc);
            case 4:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 21:
            case 23:
                originalScript = "game.quests[5].state == qs_mentioned";
                return GetQuestState(5) == QuestState.Mentioned;
            case 22:
            case 24:
                originalScript = "game.quests[5].state == qs_completed and game.global_flags[32] == 1";
                return GetQuestState(5) == QuestState.Completed && GetGlobalFlag(32);
            case 27:
            case 28:
                originalScript = "game.quests[5].state == qs_completed and game.global_flags[32] == 0";
                return GetQuestState(5) == QuestState.Completed && !GetGlobalFlag(32);
            case 29:
            case 30:
                originalScript = "game.quests[5].state == qs_accepted and game.global_flags[17] == 1";
                return GetQuestState(5) == QuestState.Accepted && GetGlobalFlag(17);
            case 41:
            case 42:
                originalScript = "game.global_flags[1] == 0";
                return !GetGlobalFlag(1);
            case 43:
            case 44:
                originalScript = "game.global_flags[1] == 1";
                return GetGlobalFlag(1);
            case 173:
            case 174:
            case 183:
            case 184:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 5";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 40:
                originalScript = "game.quests[5].state = qs_mentioned";
                SetQuestState(5, QuestState.Mentioned);
                break;
            case 71:
                originalScript = "game.quests[5].state = qs_accepted; npc.reaction_adj( pc,+10)";
                SetQuestState(5, QuestState.Accepted);
                npc.AdjustReaction(pc, +10);
                ;
                break;
            case 72:
            case 74:
                originalScript = "game.quests[5].state = qs_accepted; npc.reaction_adj( pc,-10); buttin(npc,pc,210)";
                SetQuestState(5, QuestState.Accepted);
                npc.AdjustReaction(pc, -10);
                buttin(npc, pc, 210);
                ;
                break;
            case 73:
            case 81:
            case 82:
            case 151:
            case 152:
                originalScript = "game.quests[5].state = qs_accepted";
                SetQuestState(5, QuestState.Accepted);
                break;
            case 90:
            case 120:
                originalScript = "npc.reaction_adj( pc,+10)";
                npc.AdjustReaction(pc, +10);
                break;
            case 100:
                originalScript = "game.global_flags[32] = 1";
                SetGlobalFlag(32, true);
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
            case 173:
            case 174:
            case 183:
            case 184:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}