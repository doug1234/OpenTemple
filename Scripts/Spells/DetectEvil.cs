
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

[SpellScript(111)]
public class DetectEvil : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Detect Evil OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-divination-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Detect Evil OnSpellEffect");
        spell.duration = 100 * spell.casterLevel;
        var target = spell.Targets[0];
        target.Object.AddCondition("sp-Detect Evil", spell.spellId, spell.duration, 0);
        target.ParticleSystem = AttachParticles("sp-Detect Alignment", target.Object);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Detect Evil OnBeginRound");
        // get all targets in a 90' cone, apply magical_aura particle systems
        foreach (var obj in ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, spell.spellRange, -45, 90))
        {
            if ((obj.GetAlignment().IsEvil()))
            {
                // HTN - WIP! check "power"
                AttachParticles("sp-Detect Alignment Evil", obj);
            }

        }

    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Detect Evil OnEndSpellCast");
    }

}