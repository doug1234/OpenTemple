
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
    [DialogScript(70)]
    public class FurnokDialog : Furnok, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and game.quests[18].state == qs_accepted");
                    return !npc.HasMet(pc) && GetQuestState(18) == QuestState.Accepted;
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 22:
                    Trace.Assert(originalScript == "game.story_state > 0");
                    return StoryState > 0;
                case 23:
                    Trace.Assert(originalScript == "game.story_state >= 3");
                    return StoryState >= 3;
                case 61:
                case 65:
                case 71:
                case 73:
                case 81:
                case 83:
                case 241:
                case 242:
                case 361:
                case 371:
                case 373:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 63:
                case 67:
                case 245:
                case 246:
                case 363:
                case 364:
                    Trace.Assert(originalScript == "npc.item_find(4001) != OBJ_HANDLE_NULL");
                    return npc.FindItemByName(4001) != null;
                case 64:
                case 68:
                    Trace.Assert(originalScript == "npc.item_find(4001) == OBJ_HANDLE_NULL");
                    return npc.FindItemByName(4001) == null;
                case 69:
                case 75:
                case 76:
                case 85:
                case 86:
                case 247:
                case 248:
                case 365:
                case 366:
                case 375:
                case 376:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 111:
                case 112:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_accepted");
                    return GetQuestState(18) == QuestState.Accepted;
                case 113:
                case 114:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL");
                    return npc.GetLeader() == null;
                case 141:
                case 142:
                case 171:
                case 172:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000");
                    return pc.GetMoney() >= 1000;
                case 143:
                case 144:
                case 173:
                case 174:
                    Trace.Assert(originalScript == "pc.money_get() < 1000");
                    return pc.GetMoney() < 1000;
                case 151:
                case 152:
                    Trace.Assert(originalScript == "game.global_vars[7] > 60");
                    return GetGlobalVar(7) > 60;
                case 153:
                case 154:
                    Trace.Assert(originalScript == "game.global_vars[7] <= 60");
                    return GetGlobalVar(7) <= 60;
                case 163:
                case 164:
                case 193:
                case 194:
                    Trace.Assert(originalScript == "game.global_vars[8] >= (7 - (pc.skill_level_get(npc,skill_spot)/2))");
                    throw new NotSupportedException("Conversion failed.");
                case 181:
                case 182:
                    Trace.Assert(originalScript == "game.global_vars[7] > 75");
                    return GetGlobalVar(7) > 75;
                case 183:
                case 184:
                    Trace.Assert(originalScript == "game.global_vars[7] <= 75");
                    return GetGlobalVar(7) <= 75;
                case 301:
                case 302:
                    Trace.Assert(originalScript == "npc.area != 1 and npc.area != 3");
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 303:
                case 304:
                    Trace.Assert(originalScript == "(npc.area == 1 or npc.area == 3) and game.global_flags[51] == 0");
                    return (npc.GetArea() == 1 || npc.GetArea() == 3) && !GetGlobalFlag(51);
                case 305:
                case 306:
                    Trace.Assert(originalScript == "(npc.area == 1 or npc.area == 3) and game.global_flags[51] == 1");
                    return (npc.GetArea() == 1 || npc.GetArea() == 3) && GetGlobalFlag(51);
                case 413:
                case 414:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= (6+ ((npc.money_get() - 200000) / 50000))");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= (6 + ((npc.GetMoney() - 200000) / 50000));
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 81:
                case 83:
                case 371:
                case 373:
                    Trace.Assert(originalScript == "npc.item_transfer_to(pc,4001)");
                    npc.TransferItemByNameTo(pc, 4001);
                    break;
                case 90:
                case 250:
                case 361:
                    Trace.Assert(originalScript == "pc.follower_add(npc); game.global_flags[57] = 1");
                    pc.AddFollower(npc);
                    SetGlobalFlag(57, true);
                    ;
                    break;
                case 141:
                case 142:
                case 171:
                case 172:
                    Trace.Assert(originalScript == "pc.money_adj(-1000)");
                    pc.AdjustMoney(-1000);
                    break;
                case 150:
                case 180:
                    Trace.Assert(originalScript == "game.global_vars[7] = game.random_range( 1, 100 )");
                    SetGlobalVar(7, RandomRange(1, 100));
                    break;
                case 151:
                case 152:
                case 181:
                case 182:
                    Trace.Assert(originalScript == "pc.money_adj(2000)");
                    pc.AdjustMoney(2000);
                    break;
                case 153:
                case 154:
                case 183:
                case 184:
                    Trace.Assert(originalScript == "game.global_vars[8] = game.global_vars[8] + 1");
                    SetGlobalVar(8, GetGlobalVar(8) + 1);
                    break;
                case 200:
                    Trace.Assert(originalScript == "game.global_flags[51] = 1");
                    SetGlobalFlag(51, true);
                    break;
                case 320:
                case 333:
                case 334:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 411:
                case 412:
                    Trace.Assert(originalScript == "pc.follower_remove(npc); game.global_flags[61] = 1");
                    pc.RemoveFollower(npc);
                    SetGlobalFlag(61, true);
                    ;
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}