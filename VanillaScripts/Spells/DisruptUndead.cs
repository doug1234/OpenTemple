
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(135)]
    public class DisruptUndead : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Disrupt Undead OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Disrupt Undead-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Disrupt Undead OnEndProjectile");
            spell.duration = 0;

            EndProjectileParticles(projectile);
            var target = spell.Targets[0];

            if (target.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                var attack_successful = spell.caster.PerformTouchAttack(target.Object);

                if (attack_successful == D20CAF.HIT)
                {
                    var damage_dice = Dice.D6;

                    target.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    target.ParticleSystem = AttachParticles("sp-Disrupt Undead-hit", target.Object);

                }
                else if (attack_successful == D20CAF.CRITICAL)
                {
                    var damage_dice = Dice.Parse("2d6");

                    target.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    target.ParticleSystem = AttachParticles("sp-Disrupt Undead-hit", target.Object);

                }
                else
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30007);
                    AttachParticles("Fizzle", target.Object);
                }

            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 31008);
                AttachParticles("Fizzle", target.Object);
            }

            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnEndSpellCast");
        }


    }
}
