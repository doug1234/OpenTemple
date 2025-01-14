
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

namespace Scripts.Spells;

[SpellScript(515)]
public class VampiricTouch : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Vampiric Touch OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Vampiric Touch OnSpellEffect");
        var dice = Dice.D6;
        dice = dice.WithCount(Math.Min(10, (spell.casterLevel) / 2));
        spell.duration = 600;
        var target = spell.Targets[0];
        if (!(target.Object == spell.caster))
        {
            var attack_successful = spell.caster.PerformTouchAttack(target.Object);
            if ((attack_successful & D20CAF.HIT) != D20CAF.NONE)
            {
                var old_hp = target.Object.GetStat(Stat.hp_current);
                target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, attack_successful, 0);
                var new_hp = target.Object.GetStat(Stat.hp_current);
                var damage = old_hp - new_hp;
                if (damage > (old_hp + 10))
                {
                    damage = old_hp + 10;
                }

                // spell.caster.condition_add_with_args( 'Temporary_Hit_Points', spell.id, spell.duration, damage )
                spell.caster.AddCondition("sp-Vampiric Touch", spell.spellId, spell.duration, damage);
                spell.caster.FloatMesFileLine("mes/spell.mes", 20005, TextFloaterColor.White);
            }
            else
            {
                // target.obj.float_mesfile_line( 'mes\\spell.mes', 30021 )
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
                AttachParticles("sp-Vampiric Touch", spell.caster);
            }

        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Vampiric Touch OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Vampiric Touch OnEndSpellCast");
    }

}