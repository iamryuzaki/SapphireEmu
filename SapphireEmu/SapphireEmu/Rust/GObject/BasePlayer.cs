using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch.Extend;
using Network;
using ProtoBuf;
using SapphireEmu.Data.Base.GObject;
using SapphireEmu.Environment;
using SapphireEngine;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BasePlayer : BaseCombatEntity
    {
        public static Dictionary<UInt64, BasePlayer> ListPlayers = new Dictionary<ulong, BasePlayer>();
        public static List<BasePlayer> ListOnlinePlayers = new List<BasePlayer>();

        public Connection NetConnection { get; private set; } = null;
        public Boolean IsConnected => this.NetConnection != null && this.NetConnection.connected;
        public Boolean IsSleeping => this.HasPlayerFlag(E_PlayerFlags.Sleeping);
        
        public UInt64 SteamID = 0UL;
        public String Username = "Blaster :D";
        
        public E_PlayerFlags PlayerFlags = E_PlayerFlags.Sleeping;
        public E_PlayerButton PlayerButtons = 0;
        public E_PlayerModelState PlayerModelState = E_PlayerModelState.Sleeping;

        private PlayerTick m_lastPlayerTickPacket = new PlayerTick();


        #region [Method] OnConnected
        public void OnConnected(Connection _connection)
        {
            if (ListOnlinePlayers.Contains(this) == false)
                ListOnlinePlayers.Add(this);
            
            this.NetConnection = _connection;
            this.Username = this.NetConnection.username;
            
            this.SetPlayerFlag(E_PlayerFlags.Connected, true);
            this.SetPlayerFlag(E_PlayerFlags.IsAdmin, true);
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, true);

            if (this.UID != 0)
            {
                this.OnPlayerCreated();
                this.CurentGameZona.OnReceivingNetworkablesFromView(this);
                this.CurentGameZona.OnReconnectedPlayer(this); 
            }
            else
                this.Spawn((uint) Data.Base.PrefabID.BasePlayer);
            
            this.ClientRPCEx(new SendInfo(this.NetConnection), null, Data.Base.Network.RPCMethod.ERPCMethodType.FinishLoading);
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, false);
            this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.NetConnection));
        }
        #endregion

        #region [Method] OnPlayerCreated
        public void OnPlayerCreated()
        {
            if (this.IsConnected)
            {
                this.SendNetworkUpdate(new SendInfo(this.NetConnection));
                this.ClientRPCEx(new SendInfo(this.NetConnection), null, Data.Base.Network.RPCMethod.ERPCMethodType.StartLoading);
            }
        }
        #endregion

        #region [Method] OnDisconnected
        public void OnDisconnected()
        {
            if (ListOnlinePlayers.Contains(this))
                ListOnlinePlayers.Remove(this);

            this.NetConnection = null;
            
            this.CurentGameZona.OnDisconnectedPlayer(this);
            
            this.SetPlayerFlag(E_PlayerFlags.Connected, false);
            this.SendSleepingStart();
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

        #region [Method] OnReceivedTick
        public void OnReceivedTick(Message _message)
        {
            bool needUpdateFlags = false;
            
            PlayerTick msg = PlayerTick.Deserialize(_message.read, this.m_lastPlayerTickPacket, true);
            this.PlayerButtons = (E_PlayerButton)msg.inputState.buttons;
            if (this.PlayerModelState != (E_PlayerModelState) msg.modelState.flags)
            {
                this.PlayerModelState = (E_PlayerModelState) msg.modelState.flags;
                needUpdateFlags = true;
            }

            if (this.IsSleeping)
            {
                if (this.HasPlayerButton(E_PlayerButton.FIRE_PRIMARY) || this.HasPlayerButton(E_PlayerButton.FIRE_SECONDARY) || this.HasPlayerButton(E_PlayerButton.JUMP))
                {
                    #region [Section] Spawn Test GameObjects

                    if (ListOnlinePlayers.Count == 1)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            var player = this.AddType<BasePlayer>();
                            player.SteamID = (ulong) i + 1;
                            player.Position = new Vector3(0, 215 + (i * 0.1f), 0);
                            player.Spawn((uint) Data.Base.PrefabID.BasePlayer);
                        }
                    }
                    #endregion
                    
                    this.SetPlayerFlag(E_PlayerFlags.Sleeping, false);
                    needUpdateFlags = true;
                }
                
            }
            else
            {
                if (this.Position != msg.position || this.Rotation != msg.inputState.aimAngles)
                {
                    this.Position = msg.position;
                    this.Rotation = msg.inputState.aimAngles;
                    this.OnPositionChanged();
                }
            }

            if (needUpdateFlags == true) 
                this.SendNetworkUpdate_PlayerFlags();
            this.m_lastPlayerTickPacket = msg.Copy();
        }
        #endregion

        #region [Method] GetEntityProtobuf
        
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
                baseCombat = new BaseCombat
                {
                    health = this.Health,
                    state = (int) this.LifeState
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
                    modelState = new ModelState {flags = (int) this.PlayerModelState}
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
                    modelState = new ModelState {flags = (int) this.PlayerModelState}
                }
            };
        }
        #endregion

        #region [Method] SendNetworkUpdate_PlayerFlags
        public void SendNetworkUpdate_PlayerFlags() 
        {
            if (this.IsConnected)
                this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.NetConnection));
            
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
            this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.NetConnection));
            
            this.ClientRPCEx(new SendInfo(this.NetConnection), null, Data.Base.Network.RPCMethod.ERPCMethodType.StartLoading);
            
            this.ClientRPCEx<Vector3>(_sendInfo, null, Data.Base.Network.RPCMethod.ERPCMethodType.ForcePositionTo, _vector3);
            
            this.OnPositionChanged();
            
            this.ClientRPCEx(new SendInfo(this.NetConnection), null, Data.Base.Network.RPCMethod.ERPCMethodType.FinishLoading);
            
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, false);
            this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.NetConnection));
            
        }
        #endregion

        public void SendSleepingStart()
        {
            this.SetPlayerFlag(E_PlayerFlags.Sleeping, true);
            this.PlayerModelState = E_PlayerModelState.Sleeping;
            this.SendNetworkUpdate_PlayerFlags();
        }

        public void SendSleepingStop()
        {
            this.SetPlayerFlag(E_PlayerFlags.Sleeping, false);
            this.SendNetworkUpdate_PlayerFlags();
        }
    }
}