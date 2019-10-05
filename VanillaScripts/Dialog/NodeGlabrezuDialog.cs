
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
    [DialogScript(203)]
    public class NodeGlabrezuDialog : NodeGlabrezu, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 13:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_dwarf or pc.stat_level_get(stat_race) == race_halfling or pc.stat_level_get(stat_race) == race_gnome");
                    return pc.GetRace() == RaceId.derro || pc.GetRace() == RaceId.tallfellow || pc.GetRace() == RaceId.svirfneblin;
                case 12:
                case 14:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) != race_dwarf and pc.stat_level_get(stat_race) != race_halfling and pc.stat_level_get(stat_race) != race_gnome");
                    return pc.GetRace() != RaceId.derro && pc.GetRace() != RaceId.tallfellow && pc.GetRace() != RaceId.svirfneblin;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 51:
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}