
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

[SpellScript(451)]
public class SpellResistance : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Spell Resistance OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Spell Resistance OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;

        var target_item = spell.Targets[0];

        if (target_item.Object.IsFriendly(spell.caster))
        {
            target_item.Object.AddCondition("sp-Spell Resistance", spell.spellId, spell.duration, 12 + spell.casterLevel);
            target_item.ParticleSystem = AttachParticles("sp-Spell Resistance", target_item.Object);

        }
        else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            target_item.Object.AddCondition("sp-Spell Resistance", spell.spellId, spell.duration, 12 + spell.casterLevel);
            target_item.ParticleSystem = AttachParticles("sp-Spell Resistance", target_item.Object);

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
        Logger.Info("Spell Resistance OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Spell Resistance OnEndSpellCast");
    }


}