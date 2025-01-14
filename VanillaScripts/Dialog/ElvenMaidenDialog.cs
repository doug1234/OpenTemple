
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

namespace VanillaScripts.Dialog;

[DialogScript(184)]
public class ElvenMaidenDialog : ElvenMaiden, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
            case 12:
                originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)";
                return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
            case 13:
            case 14:
                originalScript = "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != NEUTRAL_GOOD) and (game.party_alignment != CHAOTIC_GOOD) and ((pc.stat_level_get(stat_alignment) == LAWFUL_GOOD) or (pc.stat_level_get(stat_alignment) == NEUTRAL_GOOD) or (pc.stat_level_get(stat_alignment) == CHAOTIC_GOOD))";
                return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.NEUTRAL_GOOD) && (PartyAlignment != Alignment.CHAOTIC_GOOD) && ((pc.GetAlignment() == Alignment.LAWFUL_GOOD) || (pc.GetAlignment() == Alignment.NEUTRAL_GOOD) || (pc.GetAlignment() == Alignment.CHAOTIC_GOOD));
            case 15:
            case 16:
                originalScript = "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != NEUTRAL_GOOD) and (game.party_alignment != CHAOTIC_GOOD) and (pc.stat_level_get(stat_alignment) != LAWFUL_GOOD) and (pc.stat_level_get(stat_alignment) != NEUTRAL_GOOD) and (pc.stat_level_get(stat_alignment) != CHAOTIC_GOOD)";
                return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.NEUTRAL_GOOD) && (PartyAlignment != Alignment.CHAOTIC_GOOD) && (pc.GetAlignment() != Alignment.LAWFUL_GOOD) && (pc.GetAlignment() != Alignment.NEUTRAL_GOOD) && (pc.GetAlignment() != Alignment.CHAOTIC_GOOD);
            case 31:
            case 32:
                originalScript = "(pc.stat_level_get(stat_alignment) == LAWFUL_GOOD) or (pc.stat_level_get(stat_alignment) == NEUTRAL_GOOD) or (pc.stat_level_get(stat_alignment) == CHAOTIC_GOOD)";
                return (pc.GetAlignment() == Alignment.LAWFUL_GOOD) || (pc.GetAlignment() == Alignment.NEUTRAL_GOOD) || (pc.GetAlignment() == Alignment.CHAOTIC_GOOD);
            case 33:
            case 34:
                originalScript = "(pc.stat_level_get(stat_alignment) != LAWFUL_GOOD) and (pc.stat_level_get(stat_alignment) != NEUTRAL_GOOD) and (pc.stat_level_get(stat_alignment) != CHAOTIC_GOOD)";
                return (pc.GetAlignment() != Alignment.LAWFUL_GOOD) && (pc.GetAlignment() != Alignment.NEUTRAL_GOOD) && (pc.GetAlignment() != Alignment.CHAOTIC_GOOD);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 10:
                originalScript = "money_handout(npc,pc)";
                money_handout(npc, pc);
                break;
            case 21:
                originalScript = "all_run_off(npc,pc)";
                all_run_off(npc, pc);
                break;
            case 30:
                originalScript = "npc.item_transfer_to_by_proto( pc, 6125)";
                npc.TransferItemByProtoTo(pc, 6125);
                break;
            case 40:
                originalScript = "npc.item_transfer_to_by_proto( pc, 6126)";
                npc.TransferItemByProtoTo(pc, 6126);
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
            default:
                skillChecks = default;
                return false;
        }
    }
}