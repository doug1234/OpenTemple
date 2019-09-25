
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
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(731)]
    public class ElixerOfSneaking : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Elixir of sneaking OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Elixir of sneaking OnSpellEffect");
            var target = spell.Targets[0];
            target.Object.AddCondition("Elixer Timed Skill Bonus", 1, 0, 0);
        }
        // spell.target_list.remove_target( target.obj )
        // spell.spell_end( spell.id )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Elixir of sneaking OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Elixir of sneaking OnEndSpellCast");
        }

    }
}