
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

[SpellScript(608)]
public class Gate : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Gate OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("gate OnSpellEffect");
        // this needs to be in every summon spell script!
        spell.duration = 500;
        // What Demon will it be?
        var dice1 = Dice.D100;
        var what_summoned = dice1.Roll();
        int monster_proto_id;
        int num_monsters;
        if (what_summoned > 99)
        {
            // set the  proto_id for Balor
            monster_proto_id = 14286;
            num_monsters = 1;
        }
        else if (what_summoned > 75)
        {
            // set the  proto_id for Glabrezu
            monster_proto_id = 14263;
            num_monsters = 1;
        }
        else if (what_summoned > 50)
        {
            // set the  proto_id for Hezrou
            monster_proto_id = 14259;

            num_monsters = Dice.D2.Roll();

        }
        else
        {
            // set the  proto_id for Vrock
            monster_proto_id = 14258;

            num_monsters = Dice.D3.Roll();

        }

        var i = 0;
        while (i < num_monsters)
        {
            // create monster, monster should be added to target_list
            spell.SummonMonsters(true, monster_proto_id);
            i = i + 1;
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Gate OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Gate OnEndSpellCast");
    }

}