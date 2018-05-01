﻿using Network;
using SapphireEmu.Data.Base.GObject;
using SapphireEmu.Extended;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseHeldEntity : BaseEntity
    {
        public Item ItemOwner { get; set; }
        public BasePlayer PlayerOwner => (BasePlayer)this.ItemOwner.Container?.EntityOwner;

        public override void OnAwake()
        {
            base.IsComponent = true;
        }
        
        public override ProtoBuf.Entity GetEntityProtobuf()
        {
            return new ProtoBuf.Entity
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
                    rot = Vector3.zero,
                    skinid = 0,
                    time = 1f
                },
                heldEntity = new ProtoBuf.HeldEntity
                {
                    itemUID = this.ItemOwner.UID
                },
                parent = new ProtoBuf.ParentInfo
                {
                    uid = this.PlayerOwner?.UID ?? 0,
                    bone =this.PlayerOwner?.UID > 0 ?  3354652700 : 0
                }
            };
        }
        
        
        public void SetHeld(bool bHeld)
        {
            base.SetFlag(E_EntityFlags.Reserved4, bHeld);
            base.SetFlag(E_EntityFlags.Disabled, !bHeld);
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