using System;
using System.Collections.Generic;
using Network;
using ProtoBuf;
using SapphireEmu.Environment;
using SapphireEmu.Rust.GObject.Component;
using SapphireEngine;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public partial class BasePlayer : BaseCombatEntity
    {
        public static Dictionary<UInt64, BasePlayer> ListPlayers = new Dictionary<ulong, BasePlayer>();
        public static List<BasePlayer> ListOnlinePlayers = new List<BasePlayer>();

        public Boolean IsConnected => this.Network.NetConnection != null && this.Network.NetConnection.connected;
        public Boolean IsSleeping => this.HasPlayerFlag(E_PlayerFlags.Sleeping);
        
        public UInt64 SteamID = 0UL;
        public String Username = "Blaster :D";

        public BasePlayerInventory Inventory { get; private set; }
        public BasePlayerNetwork Network { get; private set; }
        public Item ActiveItem { get; set; } = null;

        public E_PlayerFlags PlayerFlags = E_PlayerFlags.Sleeping;
        public E_PlayerButton PlayerButtons = 0;
        public E_PlayerModelState PlayerModelState = E_PlayerModelState.Sleeping;

        #region [Method] Awake
        public override void OnAwake()
        {
            base.OnAwake();
            
            this.Inventory = new BasePlayerInventory(this);
            this.Network = new BasePlayerNetwork(this);
        }
        #endregion

        #region [Methods] Has and Set Player Flags
        public bool HasPlayerFlag(E_PlayerFlags _f)=> ((this.PlayerFlags & _f) == _f);

        public void SetPlayerFlag(E_PlayerFlags _f, bool _b)
        {
            if (_b)
            {
                if (!this.HasPlayerFlag(_f))
                    this.PlayerFlags |= _f;
            }
            else
            {
                if (this.HasPlayerFlag(_f))
                    this.PlayerFlags &= ~_f;
            }
        }
        #endregion
        
        #region [Method] Has Player Buttons
        public bool HasPlayerButton(E_PlayerButton _button)=> ((this.PlayerButtons & _button) == _button);
        #endregion

        #region [Methods] GetEntityProtobuf
        #region [Method] GetEntityProtobuf
        
        public override Entity GetEntityProtobuf()
        {
            for (var i = 0; i < this.Inventory.ContainerBelt.ListItems.Count; i++)
                if (this.Inventory.ContainerBelt.ListItems[i].HeldEntity != null)
                    this.Inventory.ContainerBelt.ListItems[i].HeldEntity.SendNetworkUpdate();
            
            return new Entity
            {
                baseNetworkable = new ProtoBuf.BaseNetworkable
                {
                    @group = 0,
                    prefabID = this.PrefabID,
                    uid = this.UID
                },
                baseCombat = new BaseCombat
                {
                    health = this.Health,
                    state = (int) this.LifeState,
                },
                baseEntity = new ProtoBuf.BaseEntity
                {
                    flags = (int) this.EntityFlags,
                    pos = this.Position,
                    rot = this.Rotation
                },
                basePlayer = new ProtoBuf.BasePlayer
                {
                    userid = this.SteamID,
                    name = this.Username,
                    playerFlags = (int) this.PlayerFlags,
                    modelState = new ModelState {flags = (int) this.PlayerModelState},
                    health = this.Health,
                    inventory = this.Inventory.GetProtobufObject(),
                    skinCol = -1,
                    skinMesh = -1,
                    skinTex = -1,
                    heldEntity = this.ActiveItem?.HeldEnityUID ?? 0
                }
            };
        }
        #endregion

        #region [Method] GetEntityProtobuf_PlayerFlags
        public Entity GetEntityProtobuf_PlayerFlags()
        {
            return new Entity
            {
                baseNetworkable = new ProtoBuf.BaseNetworkable
                {
                    @group = 0,
                    prefabID = this.PrefabID,
                    uid = this.UID
                },
                basePlayer = new ProtoBuf.BasePlayer
                {
                    userid = this.SteamID,
                    name = this.Username,
                    playerFlags = (int) this.PlayerFlags,
                    modelState = new ModelState {flags = (int) this.PlayerModelState},
                    heldEntity = this.ActiveItem?.HeldEnityUID ?? 0
                }
            };
        }
        #endregion
        #endregion
        
        #region [Method] SendNetworkUpdate_PlayerFlags
        public void SendNetworkUpdate_PlayerFlags() 
        {
            if (this.IsConnected)
                this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.Network.NetConnection));
            
            if (this.ListViewToMe.Count != 0)
                this.SendNetworkUpdate_PlayerFlags(new SendInfo(Extended.Rust.ToConnectionsList(ListViewToMe)));
        }
        public void SendNetworkUpdate_PlayerFlags(SendInfo sendInfo)
        {
            
            Entity entity = GetEntityProtobuf_PlayerFlags();

            this.SendNetworkUpdate(sendInfo, entity);
        }
        #endregion

        #region [Method] SendRPC_ForcePositionTo
        public void SendRPC_ForcePositionTo(Vector3 _vector3)
        {
            if (this.ListViewToMe.Count != 0)
            {
                this.SendRPC_ForcePositionTo(new SendInfo(Extended.Rust.ToConnectionsList(ListViewToMe)), _vector3);
            }
        }
        
        public void SendRPC_ForcePositionTo(SendInfo _sendInfo, Vector3 _vector3)
        {
            this.Position = _vector3;
            
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, true);
            this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.Network.NetConnection));
            
            this.ClientRPCEx(new SendInfo(this.Network.NetConnection), null, ERPCMethodType.StartLoading);
            
            this.ClientRPCEx<Vector3>(_sendInfo, null, ERPCMethodType.ForcePositionTo, _vector3);
            
            this.OnPositionChanged();
            
            this.ClientRPCEx(new SendInfo(this.Network.NetConnection), null, ERPCMethodType.FinishLoading);
            
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, false);
            this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.Network.NetConnection));
            
        }
        #endregion

        #region [Methods] Start and Stop Sleeping
        public void SendSleepingStart()
        {
            this.OnChangeActiveItem(null);
            this.SetPlayerFlag(E_PlayerFlags.Sleeping, true);
            this.PlayerModelState = E_PlayerModelState.Sleeping;
            this.SendNetworkUpdate_PlayerFlags();
        }

        public void SendSleepingStop()
        {
            this.SetPlayerFlag(E_PlayerFlags.Sleeping, false);
            this.SendNetworkUpdate_PlayerFlags();
        }
        #endregion

        #region [Method] Hurt
        public override void Hurt(float damage, E_DamageType type = E_DamageType.Generic, BaseCombatEntity initiator = null)
        {
            if (damage >= this.Health && this.HasPlayerFlag(E_PlayerFlags.Wounded) == false)
            {
                this.Health = 5f;
                this.SetPlayerFlag(E_PlayerFlags.Wounded, true);
                this.SendNetworkUpdate_PlayerFlags();
                return;
            }
            base.Hurt(damage, type, initiator);
            if (damage > 20)
                this.ClientRPCEx<Vector3, int>(new SendInfo(this.Network.NetConnection), null, ERPCMethodType.DirectionalDamage, this.Position, (int)type);
        }
        #endregion

        #region [Methods] OnRPC Methods

        [RPCMethod(ERPCMethodType.MoveItem)]
        void OnRPC_MoveItem(Message packet)
        {
            uint itemid = packet.read.UInt32();
            if (Item.ListItemsInWorld.TryGetValue(itemid, out Item itemTarget))
            {
                if (itemTarget.Container == null || itemTarget.Container.EntityOwner == this)
                {
                    uint newContainerUID = packet.read.UInt32();
                    if (ItemContainer.ListContainers.TryGetValue(newContainerUID, out ItemContainer containerTarget))
                    {
                        if (containerTarget.EntityOwner == this)
                        {
                            int slot = packet.read.Int8();
                            int amount = packet.read.UInt16();

                            // TODO: Need release split system
                            
                            if (itemTarget.Container != containerTarget)
                            {
                                if (itemTarget.Container != null)
                                {
                                    ItemContainer lastContainer = itemTarget.Container;
                                    int lastPositon = itemTarget.PositionInContainer;
                                    lastContainer.RemoveItemFromContainer(itemTarget);
                                    
                                    if (containerTarget.ListSlots.TryGetValue(slot, out Item itemInNewPosition))
                                    {
                                        containerTarget.RemoveItemFromContainer(itemInNewPosition);
                                        lastContainer.AddItemToContainer(itemInNewPosition, lastPositon);
                                    }
                                    lastContainer.OnItemConainerUpdate();
                                }
                               
                                if (containerTarget.AddItemToContainer(itemTarget, slot))
                                    containerTarget.OnItemConainerUpdate();
                            }
                            else if (itemTarget.Container.ChangeItemSlotFromContainer(itemTarget, slot))
                                itemTarget.Container.OnItemConainerUpdate();
                        }
                        else
                            ConsoleSystem.LogWarning("[BasePlayer.OnRPC_MoveItem]: Detected movie item to enemy container!");
                    }
                }
                else
                    ConsoleSystem.LogWarning("[BasePlayer.OnRPC_MoveItem]: Detected movie enemy item to container!");
            }
        }
        
        #region [Method] OnRPC_OnPlayerLanded
        [RPCMethod(ERPCMethodType.OnPlayerLanded)]
        void OnRPC_OnPlayerLanded(Message packet)
        {
            float f = packet.read.Float();
            if (!float.IsNaN(f) && !float.IsInfinity(f))
            {
                float num2 = Mathf.InverseLerp(-15f, -100f, f);
                if (num2 != 0f)
                {
                    //this.metabolism.bleeding.Add(num2 * 0.5f);
                    float amount = num2 * 500f;
                    this.Hurt(amount, E_DamageType.Fall);
                    if (amount > 20f)
                    {
                        using (EffectData effect = new EffectData())
                        {
                            effect.origin = this.Position;
                            effect.pooledstringid = 3294503035;
                            effect.normal = Vector3.zero;
                            
                            NetworkManager.BaseNetworkServer.write.Start();
                            NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.Effect);
                            effect.WriteToStream(NetworkManager.BaseNetworkServer.write);
                            NetworkManager.BaseNetworkServer.write.Send(new SendInfo(this.Network.NetConnection));
                            if (this.ListViewToMe.Count != 0)
                                NetworkManager.BaseNetworkServer.write.Send(new SendInfo(Extended.Rust.ToConnectionsList(this.ListViewToMe)));
                        }
                    }
                }
            }
        }
        #endregion   
        
        
        
        #endregion

        public void OnChangeActiveItem(Item newItem)
        {
            if (this.ActiveItem?.HeldEntity != null)
            {
                this.ActiveItem.HeldEntity.SetHeld(false);
                this.ActiveItem.HeldEntity.SendNetworkUpdate();
            }
            this.ActiveItem = newItem;
            if (this.ActiveItem?.HeldEntity != null)
            {
                this.ActiveItem.HeldEntity.SetHeld(true);
                this.ActiveItem.HeldEntity.SendNetworkUpdate();
            }
        }
    }
}