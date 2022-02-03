namespace OpenTemple.Core.Systems.D20.Actions;

public enum ActionErrorCode
{
    AEC_OK = 0,
    AEC_NOT_ENOUGH_TIME1 = 1,
    AEC_NOT_ENOUGH_TIME2 = 2,
    AEC_NOT_ENOUGH_TIME3 = 3,
    AEC_ALREADY_MOVED = 4,
    AEC_TARGET_OUT_OF_RANGE = 5,
    AEC_TARGET_TOO_CLOSE = 6,
    AEC_TARGET_BLOCKED = 7,
    AEC_TARGET_TOO_FAR = 8,
    AEC_TARGET_INVALID = 9,
    AEC_NO_LOS = 10,
    AEC_OUT_OF_AMMO = 11,
    AEC_NEED_MELEE_WEAPON = 12,
    AEC_CANT_WHILE_PRONE = 13,
    AEC_INVALID_ACTION = 14,
    AEC_CANNOT_CAST_SPELLS = 15,
    AEC_OUT_OF_CHARGES = 16,
    AEC_WRONG_WEAPON_TYPE = 17,
    AEC_CANNOT_CAST_OUT_OF_AVAILABLE_SPELLS = 18,
    AEC_CANNOT_CAST_NOT_ENOUGH_XP = 19,
    AEC_CANNOT_CAST_NOT_ENOUGH_GP = 20,
    AEC_OUT_OF_COMBAT_ONLY = 21,
    AEC_CANNOT_USE_MUST_USE_BEFORE_ATTACKING = 22,
    AEC_NEED_A_STRAIGHT_LINE = 23,
    AEC_NO_ACTIONS = 24,
    AEC_NOT_IN_COMBAT = 25,
    AEC_AREA_NOT_SAFE = 26
};