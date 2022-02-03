
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

[SpellScript(427)]
public class ShieldOfFaith : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Shield of Faith OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Shield of Faith OnSpellEffect");
        var bonus = 2 + Math.Min(3, spell.casterLevel / 6);
        spell.duration = 10 * spell.casterLevel;
        var target_item = spell.Targets[0];
        var npc = spell.caster; // added so NPC's can pre-buff
        if (npc.type != ObjectType.pc && npc.GetLeader() == null && !GameSystems.Combat.IsCombatActive())
        {
            spell.duration = 2000 * spell.casterLevel;
        }

        if (target_item.Object.IsFriendly(spell.caster))
        {
            if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
            {
                target_item.Object.AddCondition("sp-Shield of Faith", spell.spellId, spell.duration, bonus);
                target_item.ParticleSystem = AttachParticles("sp-Shield of Faith", target_item.Object);
            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31001);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

        }
        else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            // saving throw unsuccessful
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            target_item.Object.AddCondition("sp-Shield of Faith", spell.spellId, spell.duration, bonus);
            target_item.ParticleSystem = AttachParticles("sp-Shield of Faith", target_item.Object);
        }
        else
        {
            // saving throw successful
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target_item.Object);
            spell.RemoveTarget(target_item.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Shield of Faith OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Shield of Faith OnEndSpellCast");
    }

}