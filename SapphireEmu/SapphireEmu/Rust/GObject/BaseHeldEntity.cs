using System.Linq;
using Network;
using ProtoBuf;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseHeldEntity : BaseEntity
    {
        public SapphireEmu.Rust.GObject.Component.Item ItemOwner { get; set; }
        public BaseEntity EntityOwner => this.ItemOwner.Container?.EntityOwner;

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
                    flags = (int) this.EntityFlags,
                    pos = this.Position,
                    rot = this.Rotation
                },
                heldEntity = new HeldEntity
                {
                    itemUID = this.ItemOwner.UID
                },
                parent = new ParentInfo
                {
                    uid = this.EntityOwner?.UID ?? 0
                }
            };
        }
    }
}