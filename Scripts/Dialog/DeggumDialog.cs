
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
    [DialogScript(148)]
    public class DeggumDialog : Deggum, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                case 16:
                case 24:
                case 25:
                    Trace.Assert(originalScript == "game.quests[43].state >= qs_accepted or game.quests[46].state >= qs_accepted or game.quests[49].state >= qs_accepted or game.quests[52].state >= qs_accepted");
                    return GetQuestState(43) >= QuestState.Accepted || GetQuestState(46) >= QuestState.Accepted || GetQuestState(49) >= QuestState.Accepted || GetQuestState(52) >= QuestState.Accepted;
                case 42:
                case 43:
                case 63:
                case 64:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 91:
                case 92:
                case 93:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_elf or pc.stat_level_get(stat_race) == race_halfelf");
                    return pc.GetRace() == RaceId.aquatic_elf || pc.GetRace() == RaceId.halfelf;
                case 94:
                case 95:
                case 96:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) != race_elf and pc.stat_level_get(stat_race) != race_halfelf");
                    return pc.GetRace() != RaceId.aquatic_elf && pc.GetRace() != RaceId.halfelf;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 105:
                case 106:
                case 133:
                case 134:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 173:
                case 174:
                case 203:
                case 204:
                    Trace.Assert(originalScript == "game.global_flags[159] == 0");
                    return !GetGlobalFlag(159);
                case 181:
                case 182:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 221:
                case 222:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL_EVIL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 17:
                case 18:
                    Trace.Assert(originalScript == "banter(npc, pc, 20)");
                    banter(npc, pc, 20);
                    break;
                case 6:
                case 7:
                case 11:
                case 12:
                case 41:
                case 71:
                case 111:
                case 112:
                case 153:
                case 154:
                case 161:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[165] = 1; game.map_flags( 5080, 0, 1 )");
                    SetGlobalFlag(165, true);
                    // FIXME: map_flags;
                    ;
                    break;
                case 42:
                case 43:
                case 63:
                case 64:
                case 151:
                case 152:
                    Trace.Assert(originalScript == "banter2(npc,pc,230)");
                    banter2(npc, pc, 230);
                    break;
                case 50:
                case 80:
                case 120:
                case 131:
                case 132:
                case 162:
                case 163:
                case 183:
                case 184:
                case 223:
                case 224:
                    Trace.Assert(originalScript == "game.global_flags[165] = 1");
                    SetGlobalFlag(165, true);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.global_flags[166] = 1");
                    SetGlobalFlag(166, true);
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
                case 42:
                case 43:
                case 63:
                case 64:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
