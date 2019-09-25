
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
    [SpellScript(167)]
    public class Feeblemind : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Feeblemind OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Feeblemind OnSpellEffect");
            var target = spell.Targets[0];
            if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
            {
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target.Object);
                    Logger.Info("obj={0}", target.Object);
                    spell.RemoveTarget(target.Object);
                }
                else
                {
                    // saving throw unsuccessful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target.Object.FloatMesFileLine("mes/spell.mes", 20022);
                    target.Object.AddCondition("sp-Feeblemind", spell.spellId, 0, 0);
                    target.ParticleSystem = AttachParticles("sp-Feeblemind", target.Object);
                }

            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target.Object.FloatMesFileLine("mes/spell.mes", 31001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Feeblemind OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Feeblemind OnEndSpellCast");
        }

    }
}