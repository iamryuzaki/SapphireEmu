using System.Linq;
using ProtoBuf;
using SapphireEmu.Data.Base;
using SapphireEmu.Extended;
using SapphireEngine;
using Message = Network.Message;

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

        #region [Method] OnRPC_OnPlayerAttack
        // TODO: Move To BaseMelee
        [Data.Base.Network.RPCMethod(Data.Base.Network.RPCMethod.ERPCMethodType.PlayerAttack)]
        void OnRPC_OnPlayerAttack(Message packet)
        {
            BasePlayer player = packet.ToPlayer();
            using (PlayerAttack playerAttack = PlayerAttack.Deserialize(packet.read))
            {
                if (Find(playerAttack.attack.hitID, out BaseEntity hitEntity))
                {
                    if (hitEntity is BaseCombatEntity hitCombatEntity)
                    {
                        Component.Item ActiveItem = player.ActiveItem;
                        if (ActiveItem.Information.CanHeldEntity)
                        {
                            hitCombatEntity.Hurt(ActiveItem.Information.Damage);
                            ConsoleSystem.Log($"Damage: {ActiveItem.Information.Damage}, new hp {hitCombatEntity.Health}");
                        } else ConsoleSystem.LogError($"[{nameof(OnRPC_OnPlayerAttack)}][NOT HELD ENTITY] Trying hit from <{ActiveItem.Information.ItemID}>");
                    }
                }
            }
        }
        #endregion
    }
}