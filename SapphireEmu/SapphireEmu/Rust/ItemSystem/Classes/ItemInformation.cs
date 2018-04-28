using System;
using SapphireEmu.Rust.GObject;

namespace SapphireEmu.Rust
{
    public enum ItemHeldType
    {
        None = 0,
        HeldEntity = 1,
        AttackEntity = 2,
        BaseProjectile = 3,
        BaseMelee = 4
    }
    
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
#if RUST
        public static ItemInformation Load(Item item)
        {
            var iteminfo = new ItemInformation();
            iteminfo.ItemID = item.info.itemid;
            iteminfo.Shortname = item.info.shortname;
            iteminfo.MaxStack = item.info.stackable;
            iteminfo.MaxCondition = item.info.condition.max;

            var heldent = item.GetHeldEntity();
            if (heldent != null)
            {
                iteminfo.HeldEntity = ItemHeldEntity.Load(heldent);

                var heldtype = heldent.GetType();
                if (heldtype == typeof(HeldEntity)) iteminfo.HeldType = ItemHeldType.HeldEntity;
                else if (heldtype == typeof(AttackEntity)) iteminfo.HeldType = ItemHeldType.AttackEntity;
                else if (heldtype == typeof(BaseProjectile)) iteminfo.HeldType = ItemHeldType.BaseProjectile;
                else if (heldtype == typeof(BaseMelee)) iteminfo.HeldType = ItemHeldType.BaseMelee;

                iteminfo.HeldEntity = ItemHeldEntity.Load(heldent);
                iteminfo.BaseProjectile = ItemBaseProjectileInfo.Load(heldent);
            }

            return iteminfo;
        }
#endif
        
#if SAPPHIRE_EMU
            public static bool GetHeldType(ItemHeldType heldtype, out Type type)
            {
                switch (heldtype)
                {
                    case ItemHeldType.HeldEntity: type = typeof(BaseHeldEntity); return true;
                    case ItemHeldType.BaseProjectile:  type = typeof(BaseProjectile); return true;
                    case ItemHeldType.BaseMelee:  type = typeof(BaseMelee); return true;
                    default: 
                        type = typeof(BaseHeldEntity); return false;
                }
            }
#endif
    }
    
    public class ItemHeldEntity
    {
        public UInt32 PrefabID;

#if RUST
        // TODO: Add HolsterInfo
        public static ItemHeldEntity Load(BaseEntity heldent)
        {
            var ihInfo = new ItemHeldEntity();
            ihInfo.PrefabID = heldent.prefabID;
            return ihInfo;
        }
#endif
    }
    
    public class ItemBaseProjectileInfo
    {
        public Single Damage;
        public ProjectileAmmoTypes AmmoTypes;

#if RUST
        public static ItemBaseProjectileInfo Load(BaseEntity heldent)
        {
            var bProjectile = heldent as BaseProjectile;
            if (bProjectile == null || bProjectile.primaryMagazine == null) return null;

            var ibpInfo = new ItemBaseProjectileInfo();
            var imProjectile = bProjectile.primaryMagazine.ammoType.GetComponent<ItemModProjectile>();
            var projectile = imProjectile.projectileObject.Get().GetComponent<Projectile>();
            ibpInfo.Damage = projectile?.damageTypes.Sum(p => p.amount) ?? 200;
            ibpInfo.AmmoTypes = (ProjectileAmmoTypes) (int) bProjectile.primaryMagazine.definition.ammoTypes;
            return ibpInfo;
        }
#endif

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
}