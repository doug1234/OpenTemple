
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

namespace Scripts;

[ObjectScript(337)]
public class Elysia : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            pc.AddCondition("fallen_paladin");
        }

        SetGlobalFlag(971, true);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(971, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(994)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }

        if ((GetGlobalFlag(994)))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
        }

        attachee.RunOff();
        return RunDefault;
    }

}