using System;
using System.Collections.Generic;
using SapphireEmu.Data.Base.GObject;

namespace SapphireEmu.Data.Base
{
    public class ItemInformation
    {
        public static Dictionary<ItemID, ItemInformation> ListPrefabsFromItemIds = new Dictionary<ItemID, ItemInformation>
        {
            { ItemID.Rock, new ItemInformation { Damage = 5, CanHeldEntity = true, PrefabUID = PrefabID.Rock, ItemID = ItemID.Rock}},
            { ItemID.Torch, new ItemInformation { Damage = 1, CanHeldEntity = true, PrefabUID = PrefabID.Torch, ItemID = ItemID.Torch}},
            { ItemID.RifleAk, new ItemInformation() {Damage = 40, CanHeldEntity = true, PrefabUID = PrefabID.RifleAk, ItemID = ItemID.RifleAk}},
            { ItemID.BoltRifle, new ItemInformation() {Damage = 100, CanHeldEntity = true, PrefabUID = PrefabID.BoltRifle, ItemID = ItemID.BoltRifle}},
        };

        public ItemID ItemID { get; private set; }
        public PrefabID PrefabUID { get; private set; }
        public float MaxCondition { get; private set; } = 100;
        public float MaxDamageRange { get; private set; } = 1.5f;
        public float Damage { get; private set; } = 0;
        public E_DamageType DamageType { get; private set; } = E_DamageType.Blunt;
        public UInt32 MaxStack { get; private set; } = 1;
        public Boolean CanHeldEntity { get; private set; } = false;
    }
}