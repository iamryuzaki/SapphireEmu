using System;
using System.Collections.Generic;
using SapphireEmu.Data.Base.GObject;

namespace SapphireEmu.Data.Base
{
    public class ItemInformation
    {
        public Int32 ItemID;
        public string Shortname;
        public Int32 MaxStack;
        public float MaxCondition;

        public ItemHeldType HeldType = ItemHeldType.None;
        public ItemHeldEntity HeldEntity = null;
        public ItemBaseProjectileInfo BaseProjectile = null;

        public bool IsHoldable() => HeldEntity != null;

        public class ItemBaseProjectileInfo
        {
            public Single Damage;
            public ProjectileAmmoTypes AmmoTypes;

            [Flags]
            public enum ProjectileAmmoTypes
            {
                PISTOL_9MM = 1,
                RIFLE_556MM = 2,
                SHOTGUN_12GUAGE = 4,
                BOW_ARROW = 8,
                HANDMADE_SHELL = 16,
                ROCKET = 32,
                NAILS = 64
            }
        }


        public class ItemHeldEntity
        {
            public UInt32 PrefabID;
        }

        public enum ItemHeldType
        {
            None = 0,
            HeldEntity = 1,
            AttackEntity = 2,
            BaseProjectile = 3,
            BaseMelee = 4
        }
    }
}