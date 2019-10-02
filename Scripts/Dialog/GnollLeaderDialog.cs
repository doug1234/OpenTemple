
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

namespace Scripts.Dialog
{
    [DialogScript(75)]
    public class GnollLeaderDialog : GnollLeader, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 107:
                case 207:
                    Trace.Assert(originalScript == "pc != game.party[0] and game.party[0].distance_to(pc) <= 40 and not critter_is_unconscious(game.party[0]) and anyone(game.party[0].group_list(), \"has_wielded\", 3005)");
                    return pc != PartyLeader && PartyLeader.DistanceTo(pc) <= 40 && !Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 13:
                case 14:
                case 25:
                case 26:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.global_flags[37] == 1");
                    return GetGlobalFlag(37);
                case 23:
                case 24:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 27:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 28:
                    Trace.Assert(originalScript == "pc.money_get() >= 20000");
                    return pc.GetMoney() >= 20000;
                case 41:
                case 43:
                    Trace.Assert(originalScript == "pc.money_get() >= 5000");
                    return pc.GetMoney() >= 5000;
                case 42:
                case 44:
                    Trace.Assert(originalScript == "pc.money_get() >= 15000");
                    return pc.GetMoney() >= 15000;
                case 101:
                case 102:
                case 103:
                case 104:
                case 111:
                case 112:
                case 113:
                case 114:
                case 121:
                case 122:
                    Trace.Assert(originalScript == "game.global_flags[50] == 1");
                    return GetGlobalFlag(50);
                case 105:
                case 106:
                case 115:
                case 116:
                case 123:
                case 124:
                    Trace.Assert(originalScript == "game.global_flags[50] == 0");
                    return !GetGlobalFlag(50);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                case 31:
                case 32:
                case 53:
                case 54:
                case 81:
                case 82:
                case 83:
                case 131:
                case 132:
                case 141:
                case 142:
                case 205:
                case 206:
                case 221:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 6:
                    Trace.Assert(originalScript == "call_leader(npc, pc)");
                    call_leader(npc, pc);
                    break;
                case 27:
                    Trace.Assert(originalScript == "pc.money_adj(-10000)");
                    pc.AdjustMoney(-10000);
                    break;
                case 28:
                    Trace.Assert(originalScript == "pc.money_adj(-20000)");
                    pc.AdjustMoney(-20000);
                    break;
                case 33:
                case 51:
                case 52:
                case 61:
                case 71:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 41:
                case 43:
                    Trace.Assert(originalScript == "pc.money_adj(-5000)");
                    pc.AdjustMoney(-5000);
                    break;
                case 42:
                case 44:
                    Trace.Assert(originalScript == "pc.money_adj(-15000)");
                    pc.AdjustMoney(-15000);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.map_flags( 5005, 0, 1 )");
                    // FIXME: map_flags;
                    break;
                case 107:
                    Trace.Assert(originalScript == "call_leaderplease(npc, pc)");
                    call_leaderplease(npc, pc);
                    break;
                case 200:
                    Trace.Assert(originalScript == "game.global_flags[857] = 0; give_item(npc)");
                    SetGlobalFlag(857, false);
                    give_item(npc);
                    ;
                    break;
                case 207:
                    Trace.Assert(originalScript == "call_leadersvp(npc, pc)");
                    call_leadersvp(npc, pc);
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
                case 13:
                case 14:
                case 25:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 23:
                case 24:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
