
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
    [SpellScript(49)]
    public class CatsGrace : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cat's Grace OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Cat's Grace OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;

            var target_item = spell.Targets[0];

            var dex_amount = 4;

            if (target_item.Object.IsFriendly(spell.caster))
            {
                target_item.Object.AddCondition("sp-Cats Grace", spell.spellId, spell.duration, dex_amount);
                target_item.ParticleSystem = AttachParticles("sp-Cats Grace", target_item.Object);

            }
            else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.AddCondition("sp-Cats Grace", spell.spellId, spell.duration, dex_amount);
                target_item.ParticleSystem = AttachParticles("sp-Cats Grace", target_item.Object);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Cat's Grace OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cat's Grace OnEndSpellCast");
        }


    }
}
