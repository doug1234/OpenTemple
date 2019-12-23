
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

    public class RapidMetamagic
    {
        public static void RapidMMActionCostMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjActionCost();
            if (dispIo.d20a.d20ActType != D20ActionType.CAST_SPELL)
            {
                return;
            }

            if (dispIo.acpOrig.hourglassCost <= ActionCostType.Standard) // original is already less than full round
            {
                return;
            }

            if (dispIo.acpCur.hourglassCost <= 0) // adjusted amount is already free action
            {
                return;
            }

            // check if the original spell is standard action or less - if so reduce action cost to standard action
            var spData = dispIo.d20a.d20SpellData;
            var spEntry = GameSystems.Spell.GetSpellEntry(spData.SpellEnum);
            if (spEntry.spellEnum == 0)
            {
                return;
            }

            var castingTimeType = spEntry.castingTimeType;
            var mmData = spData.metaMagicData;
            if (mmData.IsQuicken && (dispIo.tbStat.tbsFlags & TurnBasedStatusFlags.FreeActionSpellPerformed) == 0)
            {
                dispIo.acpCur.hourglassCost = 0;
                dispIo.tbStat.tbsFlags |= TurnBasedStatusFlags.FreeActionSpellPerformed;
                // print "reducing cost to 0"
                return;
            }
        }

        // args are just-in-case placeholders
        [FeatCondition("Rapid Metamagic")]
        [AutoRegister] public static readonly ConditionSpec Condition = ConditionSpec.Create("Rapid Metamagic Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ActionCostMod, RapidMMActionCostMod)
            .Build();
    }
}