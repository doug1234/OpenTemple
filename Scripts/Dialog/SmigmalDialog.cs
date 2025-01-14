
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Scripts.Dialog;

[DialogScript(141)]
public class SmigmalDialog : Smigmal, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 5:
            case 6:
            case 45:
            case 46:
                originalScript = "game.party_alignment & ALIGNMENT_EVIL != 0 and pc.stat_level_get(stat_alignment) & ALIGNMENT_EVIL != 0";
                return PartyAlignment.IsEvil() && pc.GetAlignment().IsEvil();
            case 24:
            case 25:
                originalScript = "game.global_flags[425] == 1";
                return GetGlobalFlag(425);
            case 41:
            case 42:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
            case 51:
                originalScript = "game.global_flags[7] == 0";
                return !GetGlobalFlag(7);
            case 52:
                originalScript = "game.global_flags[7] == 1";
                return GetGlobalFlag(7);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 60:
                originalScript = "smigmal_well( npc,pc )";
                smigmal_well(npc, pc);
                break;
            case 61:
                originalScript = "game.particles( 'sp-invisibility', npc ); game.sound( 4031 ); smigmal_escape(npc,pc)";
                AttachParticles("sp-invisibility", npc);
                Sound(4031);
                smigmal_escape(npc, pc);
                ;
                break;
            case 110:
                originalScript = "smig_backup(npc,pc); game.global_flags[996] = 1";
                smig_backup(npc, pc);
                SetGlobalFlag(996, true);
                ;
                break;
            case 111:
            case 121:
            case 131:
                originalScript = "npc.attack(pc)";
                npc.Attack(pc);
                break;
            case 120:
                originalScript = "smig_backup_2(npc,pc)";
                smig_backup_2(npc, pc);
                break;
            case 130:
                originalScript = "smig_backup(npc,pc)";
                smig_backup(npc, pc);
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            case 41:
            case 42:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}