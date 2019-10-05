
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
    [DialogScript(173)]
    public class StcuthbertDialog : Stcuthbert, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
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
                    Trace.Assert(originalScript == "unshit(npc,pc); game.global_flags[544] = 1; game.global_flags[328] = 1");
                    unshit(npc, pc);
                    SetGlobalFlag(544, true);
                    SetGlobalFlag(328, true);
                    ;
                    break;
                case 2:
                    Trace.Assert(originalScript == "switch_to_iuz(npc,pc,200)");
                    switch_to_iuz(npc, pc, 200);
                    break;
                case 10:
                case 20:
                case 30:
                    Trace.Assert(originalScript == "unshit(npc,pc)");
                    unshit(npc, pc);
                    break;
                case 11:
                    Trace.Assert(originalScript == "switch_to_iuz(npc,pc,210)");
                    switch_to_iuz(npc, pc, 210);
                    break;
                case 21:
                    Trace.Assert(originalScript == "switch_to_iuz(npc,pc,220)");
                    switch_to_iuz(npc, pc, 220);
                    break;
                case 31:
                    Trace.Assert(originalScript == "cuthbert_raise_good(npc,pc); turn_off_gods(npc,pc); game.global_flags[544] = 0");
                    cuthbert_raise_good(npc, pc);
                    turn_off_gods(npc, pc);
                    SetGlobalFlag(544, false);
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