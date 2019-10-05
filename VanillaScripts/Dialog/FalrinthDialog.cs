
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(155)]
    public class FalrinthDialog : Falrinth, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 51:
                case 52:
                case 131:
                case 132:
                    Trace.Assert(originalScript == "game.quests[54].state == qs_accepted");
                    return GetQuestState(54) == QuestState.Accepted;
                case 21:
                case 22:
                case 61:
                case 62:
                case 91:
                case 92:
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 23:
                case 24:
                case 53:
                case 54:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 73:
                case 74:
                case 83:
                case 84:
                case 171:
                case 172:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 5808 ) or anyone( pc.group_list(), \"has_item\", 5809 ) or anyone( pc.group_list(), \"has_item\", 5810 ) or anyone( pc.group_list(), \"has_item\", 5811 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5808)) || pc.GetPartyMembers().Any(o => o.HasItemByName(5809)) || pc.GetPartyMembers().Any(o => o.HasItemByName(5810)) || pc.GetPartyMembers().Any(o => o.HasItemByName(5811));
                case 151:
                case 152:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "game.global_flags[167] = 1");
                    SetGlobalFlag(167, true);
                    break;
                case 41:
                    Trace.Assert(originalScript == "falrinth_escape(npc,pc)");
                    falrinth_escape(npc, pc);
                    break;
                case 101:
                case 102:
                case 115:
                case 116:
                case 121:
                case 193:
                case 194:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                case 61:
                case 62:
                case 91:
                case 92:
                case 111:
                case 112:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 23:
                case 24:
                case 53:
                case 54:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                case 151:
                case 152:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}