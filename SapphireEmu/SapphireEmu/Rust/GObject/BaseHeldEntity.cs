﻿using System;
using System.Linq;
using Network;
using ProtoBuf;
using SapphireEmu.Data.Base;
using SapphireEmu.Data.Base.GObject;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseHeldEntity : BaseEntity
    {
        public SapphireEmu.Rust.GObject.Component.Item ItemOwner { get; set; }
        public BasePlayer PlayerOwner => (BasePlayer)this.ItemOwner.Container?.EntityOwner;

        public override void OnAwake()
        {
            base.IsComponent = true;
        }
        
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
                    pos = this.PlayerOwner.Position,
                    rot = this.Rotation
                },
                heldEntity = new HeldEntity
                {
                    itemUID = this.ItemOwner.UID
                },
                parent = new ParentInfo
                {
                    uid = this.PlayerOwner.UID,
                    bone = 3354652700
                }
            };
        }
        
        
        public void SetHeld(bool bHeld)
        {
            base.SetFlag(E_EntityFlags.Reserved4, bHeld);
            base.SetFlag(E_EntityFlags.Disabled, !bHeld);
        }



        public override void SendNetworkUpdate(Entity _entity = null)
        {
            if (this.PlayerOwner != null && this.PlayerOwner.IsConnected)
                this.SendNetworkUpdate(new SendInfo(this.PlayerOwner.Network.NetConnection), _entity);
            
            if (this.ListViewToMe.Count != 0)
                this.SendNetworkUpdate(new SendInfo(this.PlayerOwner.ListViewToMe.ToConnectionsList()), _entity);
        }
    }
}