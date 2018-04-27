using ProtoBuf;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseProjectile : BaseHeldEntity
    {
        public int AmmoCount = 10;
        public int AmmoMax = 30;
        public int AmmoType = 815896488;
        
        public override Entity GetEntityProtobuf()
        {
            return new Entity
            {
                baseNetworkable = new ProtoBuf.BaseNetworkable
                {
                    @group = 0,
                    prefabID = this.PrefabID,
                    uid = this.UID
                },
                baseEntity = new ProtoBuf.BaseEntity
                {
                    flags = (int)this.EntityFlags,
                    pos = Vector3.zero,
                    rot = Vector3.zero
                },
                heldEntity = new HeldEntity
                {
                    itemUID = this.ItemOwner.UID
                },
                baseProjectile = new ProtoBuf.BaseProjectile()
                {
                    primaryMagazine = new Magazine()
                    {
                        ammoType = this.AmmoType,
                        capacity = this.AmmoMax,
                        contents = this.AmmoCount
                    }
                },
                parent = new ParentInfo
                {
                    uid = this.PlayerOwner.UID,
                    bone = 3354652700
                }
            };
        }
    }
}