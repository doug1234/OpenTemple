
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

namespace Scripts
{
    [ObjectScript(250)]
    public class Zombiedeath : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(0, GetGlobalVar(0) + 1);
            Logger.Info("Zombies dead={0}", GetGlobalVar(0));
            if (GetGlobalVar(0) == 3)
            {
                if (UiSystems.HelpManager.IsTutorialActive)
                {
                    UiSystems.HelpManager.ToggleTutorial();
                }

                UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.LootReminder);
                DetachScript();
            }

            return RunDefault;
        }

    }
}