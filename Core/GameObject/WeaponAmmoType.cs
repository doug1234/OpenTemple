namespace SpicyTemple.Core.GameObject
{
    public enum WeaponAmmoType
    {
        arrow = 0,
        bolt = 1,
        bullet = 2,
        magic_missile = 3,
        dagger = 4,
        club = 5,
        shortspear = 6,
        spear = 7,
        dart = 8,
        javelin = 9,
        throwing_axe = 10,
        light_hammer = 11,
        trident = 12,
        halfling_sai = 13,
        sai = 14,
        shuriken = 15,
        ball_of_fire = 16,
        bottle = 17,
        no_ammo = 10000
    }

    public static class WeaponAmmoTypeExtensions
    {
        [TempleDllLocation(0x10065bf0)]
        public static WeaponAmmoType GetWeaponAmmoType(this GameObjectBody weapon)
        {
            if (weapon.type == ObjectType.weapon)
            {
                // Melee weapons will have no_ammo by default.
                return (WeaponAmmoType) weapon.GetInt32(obj_f.weapon_ammo_type);
            }

            return WeaponAmmoType.no_ammo;
        }

        /// <summary>
        /// Gets the proto id of an item that can infinitely supply the given ammo type.
        /// </summary>
        public static bool TryGetInfiniteSupplyProtoId(this WeaponAmmoType type, out int protoId)
        {
            switch (type)
            {
                case WeaponAmmoType.arrow:
                    protoId = 5004;
                    return true;
                case WeaponAmmoType.bolt:
                    protoId = 5005;
                    return true;
                case WeaponAmmoType.bullet:
                    protoId = 5007;
                    return true;
                default:
                    protoId = -1;
                    return false;
            }
        }

        public static bool IsThrown(this WeaponAmmoType type)
        {
            return type >= WeaponAmmoType.dagger && type <= WeaponAmmoType.bottle;
        }
    }
}