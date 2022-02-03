
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

namespace VanillaScripts.Spells
{
    [SpellScript(703)]
    public class BootsOfSpeed : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Boots of Speed OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Boots of Speed OnSpellEffect");
            var remove_list = new List<GameObject>();

            spell.duration = 0;

            Logger.Info("spell.duration = {0}", spell.duration);
            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsFriendly(spell.caster))
                {
                    var return_val = target_item.Object.AddCondition("sp-Haste", spell.spellId, spell.duration, 1);

                    target_item.ParticleSystem = AttachParticles("sp-Haste", target_item.Object);

                    if (!return_val)
                    {
                        remove_list.Add(target_item.Object);
                    }

                }
                else
                {
                    if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        var return_val = target_item.Object.AddCondition("sp-Boots of Speed", spell.spellId, spell.duration, 1);

                        target_item.ParticleSystem = AttachParticles("sp-Haste", target_item.Object);

                        if (!return_val)
                        {
                            remove_list.Add(target_item.Object);
                        }

                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                        remove_list.Add(target_item.Object);
                    }

                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Boots of Speed OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Boots of Speed OnEndSpellCast");
        }


    }
}
