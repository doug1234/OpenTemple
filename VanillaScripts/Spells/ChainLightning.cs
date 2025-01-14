
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

[SpellScript(51)]
public class ChainLightning : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Chain Lightning OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Chain Lightning OnSpellEffect");
        var remove_list = new List<GameObject>();

        var primary_damage_dice = Dice.D6;

        primary_damage_dice = primary_damage_dice.WithCount(Math.Min(1 * spell.casterLevel, 20));
        var secondary_damage_dice = Dice.D6;

        secondary_damage_dice = secondary_damage_dice.WithCount(Math.Min(1 * (spell.casterLevel / 2), 20));
        SpawnParticles("sp-Chain Lightning", spell.aoeCenter);
        GameSystems.Vfx.ChainLightning(spell.caster, spell.Targets.Select(t => t.Object));
        var i = 0;

        foreach (var target_item in spell.Targets)
        {
            if ((i == 0))
            {
                i = 1;

                if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, primary_damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                }

            }
            else
            {
                if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, secondary_damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                }

            }

            remove_list.Add(target_item.Object);
        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Chain Lightning OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Chain Lightning OnEndSpellCast");
    }


}