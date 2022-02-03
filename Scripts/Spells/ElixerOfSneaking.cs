
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

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
