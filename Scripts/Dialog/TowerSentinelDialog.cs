
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
    [DialogScript(121)]
    public class TowerSentinelDialog : TowerSentinel, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 8:
                    Trace.Assert(originalScript == "game.global_flags[91] == 1");
                    return GetGlobalFlag(91);
                case 3:
                case 9:
                    Trace.Assert(originalScript == "game.global_flags[92] == 1");
                    return GetGlobalFlag(92);
                case 12:
                case 73:
                    Trace.Assert(originalScript == "pc != game.party[0] and game.party[0].distance_to(pc) <= 40 and not critter_is_unconscious(game.party[0]) and anyone(game.party[0].group_list(), \"has_wielded\", 3005)");
                    return pc != PartyLeader && PartyLeader.DistanceTo(pc) <= 40 && !Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 66:
                case 67:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 68:
                case 69:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 82:
                case 83:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 11;
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
                case 63:
                case 64:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 12:
                    Trace.Assert(originalScript == "call_leader(npc, pc)");
                    call_leader(npc, pc);
                    break;
                case 33:
                case 34:
                    Trace.Assert(originalScript == "game.global_flags[287] = 1; game.fade_and_teleport( 1800, 0, 0, 5067, 424, 489 ); run_off( npc, pc )");
                    SetGlobalFlag(287, true);
                    FadeAndTeleport(1800, 0, 0, 5067, 424, 489);
                    run_off(npc, pc);
                    ;
                    break;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.global_flags[287] = 1; game.fade_and_teleport( 1800, 0, 0, 5066, 445, 446 ); run_off( npc, pc )");
                    SetGlobalFlag(287, true);
                    FadeAndTeleport(1800, 0, 0, 5066, 445, 446);
                    run_off(npc, pc);
                    ;
                    break;
                case 73:
                    Trace.Assert(originalScript == "call_leaderplease(npc, pc)");
                    call_leaderplease(npc, pc);
                    break;
                case 111:
                    Trace.Assert(originalScript == "talk_lareth(npc,pc,310)");
                    talk_lareth(npc, pc, 310);
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
                case 66:
                case 67:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 68:
                case 69:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 82:
                case 83:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 11);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
