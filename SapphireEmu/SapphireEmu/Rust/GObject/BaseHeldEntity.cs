using System.Collections.Generic;
using Facepunch;
using Network;
using SapphireEmu.Data.Base.GObject;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseHeldEntity : BaseEntity
    {
        public Item ItemOwner { get; set; }
        public BasePlayer PlayerOwner => (BasePlayer)this.ItemOwner?.Container?.EntityOwner;

        public const uint HandBone = 3354652700u;
        
        public override void OnAwake()
        {
            base.IsComponent = true;
        }
        

        public void Initialization()
        {
            this.ItemOwner.OnParentChanged += OnItemParentChanged;
        }
        
        private void OnItemParentChanged(Item item)
        {
            if (this.ParentToPlayer(item))
            {
                return;
            }
            this.SetParent(null, 0);
            this.SetFlag(E_EntityFlags.Disabled, true);
            this.SendNetworkUpdate();
        }
        
        private bool ParentToPlayer(Item item)
        {
            BasePlayer ownerPlayer = item?.Container?.EntityOwner as BasePlayer;
            if (ownerPlayer == null)
            {
                this.ClearOwnerPlayer();
                return true;
            }
            
            this.SetOwnerPlayer(ownerPlayer);
            this.SendNetworkUpdate();
            return true;
        }
        
        public override void GetEntityProtobuf(ProtoBuf.Entity entity)
        {
            base.GetEntityProtobuf(entity);
            
            entity.heldEntity = new ProtoBuf.HeldEntity
            {
                itemUID = this.ItemOwner?.UID ?? 0
            };
        }
        
        public virtual void ClearOwnerPlayer()
        {
            ConsoleSystem.Log("Clear");
            base.SetParent(null, 0);
            this.SetHeld(false);
        }
        public virtual void SetOwnerPlayer(BasePlayer player)
        {
            ConsoleSystem.Log("Set");
            base.SetParent(player, HandBone);
            this.SetHeld(false);
        }
        
        public void SetHeld(bool bHeld)
        {
            base.SetFlag(E_EntityFlags.Reserved4, bHeld);
            base.SetFlag(E_EntityFlags.Disabled, !bHeld);
        }

        public override List<BasePlayer> GetSubscribers()
        {
            return this.PlayerOwner?.ListViewToMe ?? Pool.GetList<BasePlayer>();
        }

        // Override because we need get ListViewToMe from owner player, not our ListViewToMe
        public override void SendNetworkUpdate(ProtoBuf.Entity _entity = null)
        {
            if (this.PlayerOwner != null && this.PlayerOwner.IsConnected)
                this.SendNetworkUpdate(new SendInfo(this.PlayerOwner.Network.NetConnection), _entity);

            if ((this.PlayerOwner?.ListViewToMe.Count ?? 0) != 0)
                this.SendNetworkUpdate(new SendInfo(this.PlayerOwner.ListViewToMe.ToConnectionsList()), _entity);
        }
    }
}