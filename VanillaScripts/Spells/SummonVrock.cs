
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
    [SpellScript(729)]
    public class SummonVrock : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnSpellEffect");
            spell.duration = 5;

            var monster_proto_id = 14258;

            var monster_obj = GameSystems.MapObject.CreateObject(monster_proto_id, spell.aoeCenter);

            AttachParticles("Orb-Summon-Vrock", monster_obj);
            spell.caster.AddAIFollower(monster_obj);
            var caster_init_value = spell.caster.GetInitiative();

            monster_obj.AddToInitiative();
            monster_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            monster_obj.AddCondition("sp-Summoned", spell.spellId, spell.duration, 0);

            spell.ClearTargets();
            spell.AddTarget(monster_obj);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnEndSpellCast");
        }


    }
}