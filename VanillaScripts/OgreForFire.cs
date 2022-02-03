
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

namespace VanillaScripts
{
    [ObjectScript(164)]
    public class OgreForFire : BaseObjectScript
    {

        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(14, GetGlobalVar(14) + 1);
            if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
            {
                SetGlobalVar(13, GetGlobalVar(13) - 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(14, GetGlobalVar(14) - 1);
            return RunDefault;
        }
        public override bool OnEndCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
            {
                SetGlobalVar(13, GetGlobalVar(13) + 1);
            }

            return RunDefault;
        }


    }
}
