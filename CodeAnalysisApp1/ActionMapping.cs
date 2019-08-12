﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisApp1
{
    public static class ActionMapping
    {

        public static Dictionary<int, string> Mapping = new Dictionary<int, string>
        {
            {0, "UNSPECIFIED_MOVE"},
{1, "UNSPECIFIED_ATTACK"},
{2, "STANDARD_ATTACK"},
{3, "FULL_ATTACK"},
{4, "STANDARD_RANGED_ATTACK"},
{5, "RELOAD"},
{6, "FIVEFOOTSTEP"},
{7, "MOVE"},
{8, "DOUBLE_MOVE"},
{9, "RUN"},
{10, "CAST_SPELL"},
{11, "HEAL"},
{12, "CLEAVE"},
{13, "ATTACK_OF_OPPORTUNITY"},
{14, "WHIRLWIND_ATTACK"},
{15, "TOUCH_ATTACK"},
{16, "TOTAL_DEFENSE"},
{17, "CHARGE"},
{18, "FALL_TO_PRONE"},
{19, "STAND_UP"},
{20, "TURN_UNDEAD"},
{21, "DEATH_TOUCH"},
{22, "PROTECTIVE_WARD"},
{23, "FEAT_OF_STRENGTH"},
{24, "BARDIC_MUSIC"},
{25, "PICKUP_OBJECT"},
{26, "COUP_DE_GRACE"},
{27, "USE_ITEM"},
{28, "BARBARIAN_RAGE"},
{29, "STUNNING_FIST"},
{30, "SMITE_EVIL"},
{31, "LAY_ON_HANDS_SET"},
{32, "DETECT_EVIL"},
{33, "STOP_CONCENTRATION"},
{34, "BREAK_FREE"},
{35, "TRIP"},
{36, "REMOVE_DISEASE"},
{37, "ITEM_CREATION"},
{38, "WHOLENESS_OF_BODY_SET"},
{39, "USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL"},
{40, "TRACK"},
{41, "ACTIVATE_DEVICE_STANDARD"},
{42, "SPELL_CALL_LIGHTNING"},
{43, "AOO_MOVEMENT"},
{44, "CLASS_ABILITY_SA"},
{45, "ACTIVATE_DEVICE_FREE"},
{46, "OPEN_INVENTORY"},
{47, "ACTIVATE_DEVICE_SPELL"},
{48, "DISABLE_DEVICE"},
{49, "SEARCH"},
{50, "SNEAK"},
{51, "TALK"},
{52, "OPEN_LOCK"},
{53, "SLEIGHT_OF_HAND"},
{54, "OPEN_CONTAINER"},
{55, "THROW"},
{56, "THROW_GRENADE"},
{57, "FEINT"},
{58, "READY_SPELL"},
{59, "READY_COUNTERSPELL"},
{60, "READY_ENTER"},
{61, "READY_EXIT"},
{62, "COPY_SCROLL"},
{63, "READIED_INTERRUPT"},
{64, "LAY_ON_HANDS_USE"},
{65, "WHOLENESS_OF_BODY_USE"},
{66, "DISMISS_SPELLS"},
{67, "FLEE_COMBAT"},
{68, "USE_POTION"},
{69, "DIVINE_MIGHT"},
{70, "DISARM"},
{71, "SUNDER"},
{72, "BULLRUSH"},
{73, "TRAMPLE"},
{74, "GRAPPLE"},
{75, "PIN"},
{76, "OVERRUN"},
{77, "SHIELD_BASH"},
{78, "DISARMED_WEAPON_RETRIEVE"},
{79, "AID_ANOTHER_WAKE_UP"},
{80, "EMPTY_BODY"},
{81, "QUIVERING_PALM"},
{82, "PYTHON_ACTION"},
{83, "NUMACTIONS"},
        };

    }
}