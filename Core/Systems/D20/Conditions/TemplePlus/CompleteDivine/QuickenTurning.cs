using System;
using System.Collections.Generic;
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
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Quicken Turning:  Complete Divine, p. 84
    public class QuickenTurning
    {
        private static void QuickenTurningBeginRound(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0); // reset turn undead used
        }

        private static void TurnUndeadDisabled(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg1(); // Returns 1 if turn undead has already been used
        }

        private static void TurnUndeadPerform(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(1); // Set the flag that turn undead has been used this round
        }

        public static void QuickenTurningCostMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjActionCost();
            if (dispIo.d20a.d20ActType == D20ActionType.TURN_UNDEAD)
            {
                // Always a free action with the feat
                dispIo.acpCur.hourglassCost = ActionCostType.Null;
            }
        }

        // First argument is whether or not turn undead is used, second is extra
        [FeatCondition("Quicken Turning")]
        [AutoRegister] public static readonly ConditionSpec Condition = ConditionSpec.Create("Quicken Turning Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.BeginRound, QuickenTurningBeginRound)
            .AddQueryHandler("Turn Undead Disabled", TurnUndeadDisabled)
            .AddSignalHandler("Turn Undead Perform", TurnUndeadPerform)
            .AddHandler(DispatcherType.ActionCostMod, QuickenTurningCostMod)
            .Build();
    }
}