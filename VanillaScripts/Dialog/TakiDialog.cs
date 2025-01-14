
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

[DialogScript(170)]
public class TakiDialog : Taki, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 41:
            case 42:
                originalScript = "game.quests[52].state >= qs_mentioned";
                return GetQuestState(52) >= QuestState.Mentioned;
            case 65:
            case 66:
            case 81:
            case 82:
                originalScript = "game.global_flags[146] == 1";
                return GetGlobalFlag(146);
            case 71:
            case 72:
            case 91:
            case 92:
                originalScript = "(game.party_alignment != LAWFUL_EVIL) and (game.party_alignment != NEUTRAL_EVIL) and (game.party_alignment != CHAOTIC_EVIL) and anyone( pc.group_list(), \"has_follower\", 8040 )";
                return (PartyAlignment != Alignment.LAWFUL_EVIL) && (PartyAlignment != Alignment.NEUTRAL_EVIL) && (PartyAlignment != Alignment.CHAOTIC_EVIL) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8040));
            case 73:
            case 74:
            case 93:
            case 94:
            case 185:
            case 186:
            case 193:
            case 194:
                originalScript = "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
            case 75:
            case 76:
            case 95:
            case 96:
                originalScript = "(game.party_alignment != LAWFUL_EVIL) and (game.party_alignment != NEUTRAL_EVIL) and (game.party_alignment != CHAOTIC_EVIL) and not anyone( pc.group_list(), \"has_follower\", 8040 )";
                return (PartyAlignment != Alignment.LAWFUL_EVIL) && (PartyAlignment != Alignment.NEUTRAL_EVIL) && (PartyAlignment != Alignment.CHAOTIC_EVIL) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8040));
            case 111:
            case 112:
                originalScript = "anyone( pc.group_list(), \"has_follower\", 8040 )";
                return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8040));
            case 113:
            case 114:
                originalScript = "not anyone( pc.group_list(), \"has_follower\", 8040 )";
                return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8040));
            case 141:
            case 142:
                originalScript = "not pc.follower_atmax()";
                return !pc.HasMaxFollowers();
            case 143:
            case 144:
                originalScript = "pc.follower_atmax()";
                return pc.HasMaxFollowers();
            case 183:
            case 184:
            case 191:
            case 192:
                originalScript = "(game.party_alignment != LAWFUL_EVIL) and (game.party_alignment != NEUTRAL_EVIL) and (game.party_alignment != CHAOTIC_EVIL)";
                return (PartyAlignment != Alignment.LAWFUL_EVIL) && (PartyAlignment != Alignment.NEUTRAL_EVIL) && (PartyAlignment != Alignment.CHAOTIC_EVIL);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
                originalScript = "switch_to_ashrem(npc,pc,1,10)";
                switch_to_ashrem(npc, pc, 1, 10);
                break;
            case 111:
            case 112:
            case 113:
            case 114:
                originalScript = "npc.attack(pc)";
                npc.Attack(pc);
                break;
            case 150:
                originalScript = "pc.follower_add(npc)";
                pc.AddFollower(npc);
                break;
            case 171:
                originalScript = "switch_to_ashrem(npc,pc,10,180)";
                switch_to_ashrem(npc, pc, 10, 180);
                break;
            case 210:
                originalScript = "pc.follower_remove(npc)";
                pc.RemoveFollower(npc);
                break;
            case 231:
                originalScript = "switch_to_ashrem(npc,pc,100,240)";
                switch_to_ashrem(npc, pc, 100, 240);
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