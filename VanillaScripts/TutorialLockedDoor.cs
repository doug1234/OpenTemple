
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

namespace VanillaScripts;

[ObjectScript(252)]
public class TutorialLockedDoor : BaseObjectScript
{

    public override bool OnUnlockAttempt(GameObject attachee, GameObject triggerer)
    {
        if (!UiSystems.HelpManager.IsTutorialActive)
        {
            UiSystems.HelpManager.ToggleTutorial();
        }

        UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.LockedDoorReminder);
        return SkipDefault;
    }


}