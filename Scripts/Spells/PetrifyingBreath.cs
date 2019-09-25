
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
    [SpellScript(736)]
    public class PetrifyingBreath : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Breath OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info(" spell.caster={0}", spell.caster);
            Logger.Info(" caster.level={0}", spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Breath OnSpellEffect");
            spell.dc = 19;
            spell.duration = 100;
            spell.casterLevel = 10;
            // dam = dice_new( '1d6' )
            // dam.number = min( 15, spell.caster_level )
            AttachParticles("sp-Cone of Cold", spell.caster);
            // get all targets in a 25ft + 5ft/2levels cone (60')
            // range = 25 + 5 * int(spell.caster_level/2)
            var range = 60;
            using var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 60);
            // print >> efile, "spell range= ", range, "\n"
            // print >> efile, "target list: ", target_list, "\n"
            foreach (var obj in target_list)
            {
                if (obj == spell.caster)
                {
                    continue;
                }
                if (obj.GetNameId() == 14309)
                {
                    SetGlobalFlag(811, false);
                }

                if (obj.SavingThrow(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster))
                {
                    // saving throw successful
                    obj.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    // saving throw unsuccessful
                    obj.FloatMesFileLine("mes/spell.mes", 30002);
                    // HTN - apply condition HALT (Petrifyed)
                    obj.AddCondition("sp-Command", spell.spellId, spell.duration, 4);
                    AttachParticles("sp-Bestow Curse", obj);
                }

            }

            spell.EndSpell();
        }
        // efile.close()

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Breath OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Breath OnEndSpellCast");
        }

    }
}