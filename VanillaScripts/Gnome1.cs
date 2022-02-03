
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
    [ObjectScript(78)]
    public class Gnome1 : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(55)))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 100);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }


    }
}
