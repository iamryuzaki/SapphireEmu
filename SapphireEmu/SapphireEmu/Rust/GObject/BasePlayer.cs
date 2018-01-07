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
        
        
        public void OnConnected(Connection connection)
        {
            if (ListOnlinePlayers.Contains(this) == false)
                ListOnlinePlayers.Add(this);
            
            this.NetConnection = connection;
            this.Username = this.NetConnection.username;
            
            this.SetPlayerFlag(E_PlayerFlags.Connected, true);
            this.SetPlayerFlag(E_PlayerFlags.IsAdmin, true);
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, true);
            this.ListViewToMe.Add(this);
                
            if (this.UID == 0)
                this.Spawn((uint) Data.Base.PrefabID.BasePlayer);
            else
                this.SendNetworkUpdate(new SendInfo(this.NetConnection));
            
            this.SendFullSnapshot();
        }

        public void SendFullSnapshot()
        {
            if (this.ListViewToMe.Contains(this) == false)
                this.ListViewToMe.Add(this);
            
            EACManager.OnStartLoading(this.NetConnection);
            
            this.ClientRPCEx(new SendInfo(this.NetConnection), null, Data.Base.Network.RPCMethod.ERPCMethodType.StartLoading);
            
            this.OnPositionChanged();
            this.CurentGameZona.OnReceivingDataFromZona(this);
            
            this.ClientRPCEx(new SendInfo(this.NetConnection), null, Data.Base.Network.RPCMethod.ERPCMethodType.FinishLoading);
            
            this.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, false);
            EACManager.OnFinishLoading(this.NetConnection);
            
            this.SendNetworkUpdate_PlayerFlags(new SendInfo(this.NetConnection));
        }

        public void OnDisconnected()
        {
            if (ListOnlinePlayers.Contains(this))
                ListOnlinePlayers.Remove(this);

            this.NetConnection = null;
            
            if (this.ListViewToMe.Contains(this) == true)
                this.ListViewToMe.Remove(this);
            
            this.CurentGameZona.OnDisconnectedPlayer(this);
            
            this.SetPlayerFlag(E_PlayerFlags.Sleeping, true);
            this.SetPlayerFlag(E_PlayerFlags.Connected, false);
            this.SendNetworkUpdate_PlayerFlags();
        }
        
        #region [Methods] Has and Set Player Flags
        public bool HasPlayerFlag(E_PlayerFlags f)=> ((this.PlayerFlags & f) == f);

        public void SetPlayerFlag(E_PlayerFlags f, bool b)
        {
            if (b)
            {
                if (!this.HasPlayerFlag(f))
                    this.PlayerFlags |= f;
            }
            else
            {
                if (this.HasPlayerFlag(f))
                    this.PlayerFlags &= ~f;
            }
        }
        #endregion
        
        #region [Method] Has Player Buttons
        public bool HasPlayerButton(E_PlayerButton button)=> ((this.PlayerButtons & button) == button);
        #endregion

        #region [Method] OnReceivedTick
        public void OnReceivedTick(Message message)
        {
            bool needUpdateFlags = false;
            
            PlayerTick msg = PlayerTick.Deserialize(message.read, this.m_lastPlayerTickPacket, true);
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
                    var player = this.AddType<BasePlayer>();
                    player.SteamID = 1L;
                    player.Position = new Vector3(0,215,0);
                    player.Spawn((uint)Data.Base.PrefabID.BasePlayer);
                    
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
                    @group = this.CurentGameZona.UID,
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
                    @group = this.CurentGameZona.UID,
                    prefabID = this.PrefabID,
                    uid = this.UID
                },
                basePlayer = new ProtoBuf.BasePlayer
                {
                    userid = this.SteamID,
                    playerFlags = (int) this.PlayerFlags,
                    modelState = new ModelState {flags = (int) this.PlayerModelState}
                }
            };
        }
        #endregion

        #region [Method] SendNetworkUpdate_PlayerFlags
        public void SendNetworkUpdate_PlayerFlags() 
        {
            if (this.ListViewToMe.Count != 0)
            {
                this.SendNetworkUpdate_PlayerFlags(new SendInfo(Extended.Rust.ToConnectionsList(ListViewToMe)));
            }
        }
        public void SendNetworkUpdate_PlayerFlags(SendInfo sendInfo)
        {
            ConsoleSystem.Log("SendNetworkUpdate_PlayerFlags => " + this.Position);
            
            Entity entity = GetEntityProtobuf_PlayerFlags();

            this.SendNetworkUpdate(sendInfo, entity);
        }
        #endregion

        #region [Method] SendRPC_ForcePositionTo
        public void SendRPC_ForcePositionTo(Vector3 vector3)
        {
            if (this.ListViewToMe.Count != 0)
            {
                this.SendRPC_ForcePositionTo(new SendInfo(Extended.Rust.ToConnectionsList(ListViewToMe)), vector3);
            }
        }
        
        public void SendRPC_ForcePositionTo(SendInfo sendInfo, Vector3 vector3)
        {
            this.Position = vector3;
            this.SendFullSnapshot();
            
            NetworkManager.BaseNetworkServer.write.Start();
            NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.RPCMessage);
            NetworkManager.BaseNetworkServer.write.EntityID(this.UID);
            NetworkManager.BaseNetworkServer.write.UInt32((uint)Data.Base.Network.RPCMethod.ERPCMethodType.ForcePositionTo);
            NetworkManager.BaseNetworkServer.write.Vector3(vector3);
            NetworkManager.BaseNetworkServer.write.Send(sendInfo);
        }
        #endregion
    }
}