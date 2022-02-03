namespace OpenTemple.Core.Systems.D20;

public enum D20DispatcherKey
{
    NONE = 0,
    STAT_STRENGTH = 1,
    STAT_DEXTERITY = 2,
    STAT_CONSTITUTION = 3,
    STAT_INTELLIGENCE = 4,
    STAT_WISDOM = 5,
    STAT_CHARISMA = 6,
    SAVE_FORTITUDE = 7,
    SAVE_REFLEX = 8,
    SAVE_WILL = 9,
    IMMUNITY_SPELL = 10,
    IMMUNITY_11 = 11,

    IMMUNITY_12 =
        12, // used in AI Controlled, Blindness, and Dominate. Might be a bug, but it doesn't seem to be handled in the immunity handler anyway
    IMMUNITY_COURAGE = 13, // used in Aura of Courage
    IMMUNITY_RACIAL = 14, // actually just Undead and Ooze use this
    IMMUNITY_15 = 15,
    IMMUNITY_SPECIAL = 16,
    OnEnterAoE = 18,
    OnLeaveAoE = 19,
    SKILL_APPRAISE = 20,
    SKILL_BLUFF = 21,
    SKILL_CONCENTRATION = 22,
    SKILL_RIDE = 59,
    SKILL_SWIM = 60,
    SKILL_USE_ROPE = 61,

    CL_Level = 63, // used for queries that regard negative levels
    CL_Barbarian = 64,
    CL_Bard = 65,
    CL_Cleric = 66,
    CL_Druid = 67,
    CL_Fighter = 68,
    CL_Monk = 69,
    CL_Paladin = 70,
    CL_Ranger = 71,
    CL_Rogue = 72,
    CL_Sorcerer = 73,
    CL_Wizard = 74,

    D20A_UNSPECIFIED_MOVE = 75,
    D20A_UNSPECIFIED_ATTACK = 17,
    D20A_STANDARD_ATTACK = 23,
    D20A_FULL_ATTACK = 24,
    D20A_STANDARD_RANGED_ATTACK = 25,
    D20A_RELOAD = 26,
    D20A_5FOOTSTEP = 27,
    D20A_MOVE = 28,
    D20A_DOUBLE_MOVE = 29,
    D20A_RUN = 30,
    D20A_CAST_SPELL = 31,
    D20A_HEAL = 32,
    D20A_CLEAVE = 33,
    D20A_ATTACK_OF_OPPORTUNITY = 34,
    D20A_WHIRLWIND_ATTACK = 35,
    D20A_TOUCH_ATTACK = 36,
    D20A_TOTAL_DEFENSE = 37,
    D20A_CHARGE = 38,
    D20A_FALL_TO_PRONE = 39,
    D20A_STAND_UP = 40,
    D20A_TURN_UNDEAD = 41,
    D20A_DEATH_TOUCH = 42,
    D20A_PROTECTIVE_WARD = 43,
    D20A_FEAT_OF_STRENGTH = 44,
    D20A_BARDIC_MUSIC = 45,
    D20A_PICKUP_OBJECT = 46,
    D20A_COUP_DE_GRACE = 47,
    D20A_USE_ITEM = 48,
    D20A_BARBARIAN_RAGE = 49,
    D20A_STUNNING_FIST = 50,
    D20A_SMITE_EVIL = 51,
    D20A_LAY_ON_HANDS_SET = 52,
    D20A_DETECT_EVIL = 53,
    D20A_STOP_CONCENTRATION = 54,
    D20A_BREAK_FREE = 55,
    D20A_TRIP = 56,
    D20A_REMOVE_DISEASE = 57,
    D20A_ITEM_CREATION = 58,
    D20A_WHOLENESS_OF_BODY_SET = 62,
    D20A_USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL = 76,
    D20A_TRACK = 77,
    D20A_ACTIVATE_DEVICE_STANDARD = 78,
    D20A_SPELL_CALL_LIGHTNING = 79,
    D20A_AOO_MOVEMENT = 80,
    D20A_CLASS_ABILITY_SA = 81,
    D20A_ACTIVATE_DEVICE_FREE = 82,
    D20A_OPEN_INVENTORY = 83,
    D20A_ACTIVATE_DEVICE_SPELL = 84,
    D20A_DISABLE_DEVICE = 85,
    D20A_SEARCH = 86,
    D20A_SNEAK = 87,
    D20A_TALK = 88,
    D20A_OPEN_LOCK = 89,
    D20A_SLEIGHT_OF_HAND = 90,
    D20A_OPEN_CONTAINER = 91,
    D20A_THROW = 92,
    D20A_THROW_GRENADE = 93,
    D20A_FEINT = 94,
    D20A_READY_SPELL = 95,
    D20A_READY_COUNTERSPELL = 96,
    D20A_READY_ENTER = 97,
    D20A_READY_EXIT = 98,
    D20A_COPY_SCROLL = 99,
    D20A_READIED_INTERRUPT = 103,
    D20A_LAY_ON_HANDS_USE = 104,
    D20A_WHOLENESS_OF_BODY_USE = 105,
    D20A_DISMISS_SPELLS = 106,
    D20A_FLEE_COMBAT = 107,
    D20A_USE_POTION = 108,
    D20A_DIVINE_MIGHT = 144,
    D20A_EMPTY_BODY = 145,
    D20A_QUIVERING_PALM = 146,

    NEWDAY_REST = 145, // for successfully resting (is also triggered for an 8 hour uninterrupted rest period)

    NEWDAY_CALENDARICAL =
        146, // for starting a new calendarical day (or artificially adding a days period); I think it's only used for disease timers
    SIG_HP_Changed = 147,
    SIG_HealSkill = 148,
    SIG_Sequence = 149,
    SIG_Pre_Action_Sequence = 150,
    SIG_Action_Recipient = 151,
    SIG_BeginTurn = 152,
    SIG_EndTurn = 153,
    SIG_Dropped_Enemy = 154,
    SIG_Concentration_Broken = 155,
    SIG_Remove_Concentration = 156,
    SIG_BreakFree = 157,
    SIG_Spell_Cast = 158,
    SIG_Spell_End = 159,
    SIG_Spell_Grapple_Removed = 160,
    SIG_Killed = 161,
    SIG_AOOPerformed = 162,
    SIG_Aid_Another = 163,
    SIG_TouchAttackAdded = 164,
    SIG_TouchAttack = 165,
    SIG_Temporary_Hit_Points_Removed = 166,
    SIG_Standing_Up = 167,
    SIG_Bardic_Music_Completed = 168,
    SIG_Combat_End = 169,
    SIG_Initiative_Update = 170,
    SIG_RadialMenu_Clear_Checkbox_Group = 171,
    SIG_Combat_Critter_Moved = 172,
    SIG_Hide = 173,
    SIG_Show = 174,
    SIG_Feat_Remove_Slippery_Mind = 175,
    SIG_Broadcast_Action = 176,
    SIG_Remove_Disease = 177,
    SIG_Rogue_Skill_Mastery_Init = 178,
    SIG_Spell_Call_Lightning = 179,
    SIG_Magical_Item_Deactivate = 180,
    SIG_Spell_Mirror_Image_Struck = 181,
    SIG_Spell_Sanctuary_Attempt_Save = 182,
    SIG_Experience_Awarded = 183,
    SIG_Pack = 184,
    SIG_Unpack = 185,
    SIG_Teleport_Prepare = 186,
    SIG_Teleport_Reconnect = 187,
    SIG_Atone_Fallen_Paladin = 188,
    SIG_Summon_Creature = 189,
    SIG_Attack_Made = 190,
    SIG_Golden_Skull_Combine = 191,
    SIG_Inventory_Update = 192,
    SIG_Critter_Killed = 193,
    SIG_SetPowerAttack = 194,
    SIG_SetExpertise = 195,
    SIG_SetCastDefensively = 196,
    SIG_Resurrection = 197,
    SIG_Dismiss_Spells = 198,
    SIG_DealNormalDamage = 199,
    SIG_Update_Encumbrance = 200,
    SIG_Remove_AI_Controlled = 201,
    SIG_Verify_Obj_Conditions = 202,
    SIG_Web_Burning = 203,
    SIG_Anim_CastConjureEnd = 204,
    SIG_Item_Remove_Enhancement = 205,
    SIG_Disarmed_Weapon_Retrieve = 206, // NEW
    SIG_Disarm = 207, // NEW; resets the "took damage -> abort" flag
    SIG_AID_ANOTHER_WAKE_UP = 208,

    QUE_Helpless = 207,
    QUE_SneakAttack = 208,
    QUE_OpponentSneakAttack = 209,
    QUE_CoupDeGrace = 210,
    QUE_Mute = 211,
    QUE_CannotCast = 212,
    QUE_CannotUseIntSkill = 213,
    QUE_CannotUseChaSkill = 214,
    QUE_RapidShot = 215,
    QUE_Critter_Is_Concentrating = 216,
    QUE_Critter_Is_On_Consecrate_Ground = 217,
    QUE_Critter_Is_On_Desecrate_Ground = 218,
    QUE_Critter_Is_Held = 219,
    QUE_Critter_Is_Invisible = 220,
    QUE_Critter_Is_Afraid = 221,
    QUE_Critter_Is_Blinded = 222,
    QUE_Critter_Is_Charmed = 223,
    QUE_Critter_Is_Confused = 224,
    QUE_Critter_Is_AIControlled = 225,
    QUE_Critter_Is_Cursed = 226,
    QUE_Critter_Is_Deafened = 227,
    QUE_Critter_Is_Diseased = 228,
    QUE_Critter_Is_Poisoned = 229,
    QUE_Critter_Is_Stunned = 230,
    QUE_Critter_Is_Immune_Critical_Hits = 231,
    QUE_Critter_Is_Immune_Poison = 232,
    QUE_Critter_Has_Spell_Resistance = 233,
    QUE_Critter_Has_Condition = 234,
    QUE_Critter_Has_Freedom_of_Movement = 235,
    QUE_Critter_Has_Endure_Elements = 236,
    QUE_Critter_Has_Protection_From_Elements = 237,
    QUE_Critter_Has_Resist_Elements = 238,
    QUE_Critter_Has_True_Seeing = 239,
    QUE_Critter_Has_Spell_Active = 240,
    QUE_Critter_Can_Call_Lightning = 241,
    QUE_Critter_Can_See_Invisible = 242,
    QUE_Critter_Can_See_Darkvision = 243,
    QUE_Critter_Can_See_Ethereal = 244,
    QUE_Critter_Can_Discern_Lies = 245,
    QUE_Critter_Can_Detect_Chaos = 246,
    QUE_Critter_Can_Detect_Evil = 247,
    QUE_Critter_Can_Detect_Good = 248,
    QUE_Critter_Can_Detect_Law = 249,
    QUE_Critter_Can_Detect_Magic = 250,
    QUE_Critter_Can_Detect_Undead = 251,
    QUE_Critter_Can_Find_Traps = 252,
    QUE_Critter_Can_Dismiss_Spells = 253,
    QUE_Obj_Is_Blessed = 254,
    QUE_Unconscious = 255,
    QUE_Dying = 256,
    QUE_Dead = 257,
    QUE_AOOPossible = 258,
    QUE_AOOIncurs = 259,
    QUE_HoldingCharge = 260,
    QUE_Has_Temporary_Hit_Points = 261,
    QUE_SpellInterrupted = 262,
    QUE_ActionTriggersAOO = 263,
    QUE_ActionAllowed = 264,
    QUE_Prone = 265,
    QUE_RerollSavingThrow = 266,
    QUE_RerollAttack = 267,
    QUE_RerollCritical = 268,
    QUE_Commanded = 269,
    QUE_Turned = 270,
    QUE_Rebuked = 271,
    QUE_CanBeFlanked = 272,
    QUE_Critter_Is_Grappling = 273,
    QUE_Barbarian_Raged = 274,
    QUE_Barbarian_Fatigued = 275,
    QUE_NewRound_This_Turn = 276,
    QUE_Flatfooted = 277,
    QUE_Masterwork = 278,
    QUE_FailedDecipherToday = 279,
    QUE_Polymorphed = 280,
    QUE_IsActionInvalid_CheckAction = 281,
    QUE_CanBeAffected_PerformAction = 282,
    QUE_CanBeAffected_ActionFrame = 283,
    QUE_AOOWillTake = 284,
    QUE_Weapon_Is_Mighty_Cleaving = 285,
    QUE_Autoend_Turn = 286,
    QUE_ExperienceExempt = 287,
    QUE_FavoredClass = 288,
    QUE_IsFallenPaladin = 289,
    QUE_WieldedTwoHanded = 290,
    QUE_Critter_Is_Immune_Energy_Drain = 291,
    QUE_Critter_Is_Immune_Death_Touch = 292,
    QUE_Failed_Copy_Scroll = 293,
    QUE_Armor_Get_AC_Bonus = 294,
    QUE_Armor_Get_Max_DEX_Bonus = 295,
    QUE_Armor_Get_Max_Speed = 296,
    QUE_FightingDefensively = 297,
    QUE_Elemental_Gem_State = 298,
    QUE_Untripable = 299,
    QUE_Has_Thieves_Tools = 300,
    QUE_Critter_Is_Encumbered_Light = 301,
    QUE_Critter_Is_Encumbered_Medium = 302,
    QUE_Critter_Is_Encumbered_Heavy = 303,
    QUE_Critter_Is_Encumbered_Overburdened = 304,
    QUE_Has_Aura_Of_Courage = 305,
    QUE_BardicInstrument = 306,
    QUE_EnterCombat = 307,
    QUE_AI_Fireball_OK = 308,
    QUE_Critter_Cannot_Loot = 309,
    QUE_Critter_Cannot_Wield_Items = 310,
    QUE_Critter_Is_Spell_An_Ability = 311,
    QUE_Play_Critical_Hit_Anim = 312,
    QUE_Is_BreakFree_Possible = 313,
    QUE_Critter_Has_Mirror_Image = 314,
    QUE_Wearing_Ring_of_Change = 315,
    QUE_Critter_Has_No_Con_Score = 316,
    QUE_Item_Has_Enhancement_Bonus = 317,
    QUE_Item_Has_Keen_Bonus = 318,
    QUE_AI_Has_Spell_Override = 319,
    QUE_Weapon_Get_Keen_Bonus = 320,
    QUE_Disarmed = 321,
    SIG_Destruction_Domain_Smite = 322,
    QUE_Can_Perform_Disarm = 323,
    QUE_Craft_Wand_Spell_Level = 324,
    QUE_Is_Ethereal = 325,
    QUE_Empty_Body_Num_Rounds = 326, // returns number of rounds set for Monk's Empty Body
    QUE_Quivering_Palm_Can_Perform = 327,
    QUE_Trip_AOO = 328,
    QUE_Get_Arcane_Spell_Failure = 329, // returns additive spell failure chance

    QUE_Is_Preferring_One_Handed_Wield =
        330, // e.g. a character with a Buckler can opt to wield a sword one handed so as to not take the -1 to hit penalty

    QUE_Scribe_Scroll_Spell_Level = 331,
    QUE_Critter_Is_Immune_Paralysis = 332,

    LVL_Stats_Activate = 100,
    LVL_Stats_Check_Complete = 101,
    LVL_Stats_Finalize = 102,

    LVL_Features_Activate = 200,
    LVL_Features_Check_Complete = 201,
    LVL_Features_Finalize = 202,

    LVL_Skills_Activate = 300,
    LVL_Skills_Check_Complete = 301,
    LVL_Skills_Finalize = 302,



    LVL_Feats_Activate = 400,
    LVL_Feats_Check_Complete = 401,
    LVL_Feats_Finalize = 402,

    LVL_Spells_Activate = 500,
    LVL_Spells_Check_Complete = 501,
    LVL_Spells_Finalize = 502,

    SPELL_Base_Caster_Level = 4096
}