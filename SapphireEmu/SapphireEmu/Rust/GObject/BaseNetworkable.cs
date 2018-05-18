using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Network;
using ProtoBuf;
using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEmu.Rust.ZonaManager;
using SapphireEngine;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseNetworkable : SapphireType
    {
        public static Dictionary<UInt32, BaseNetworkable> ListNetworkables = new Dictionary<uint, BaseNetworkable>();
        public static UInt32 TakeUID()
        {
            m_LastNetworkableUID++;
            return m_LastNetworkableUID;
        }
        private static UInt32 m_LastNetworkableUID = 0;
        
        public UInt32 UID { get; private set; } = 0;
        public UInt32 PrefabID { get; private set; }
        public Boolean IsSpawned => this.UID != 0;
        
        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public GameZona CurentGameZona = null;
        public Boolean IsComponent { get; set; } = false;
        
        public bool _limitedNetworking;
        
        // Which player is subscribed to me and view me
        public List<BasePlayer> ListViewToMe = new List<BasePlayer>();

        #region [Property] LimitNetworking
        public bool limitNetworking
        {
            get => this._limitedNetworking;
            set
            {
                if (value == this._limitedNetworking)
                {
                    return;
                }
                this._limitedNetworking = value;
                if (!this._limitedNetworking)
                {
                    this.OnNetworkLimitEnd();
                }
                else
                {
                    this.OnNetworkLimitStart();
                }
            }
        }
        #endregion

        #region [Methods] NetworkLimit [Start|End]
        private void OnNetworkLimitStart()
        {
            List<Connection> subscribers = this.GetSubscribers().Where(p=>p.IsConnected).Select(p=>p.Network.NetConnection).ToList();
            if (subscribers.Count == 0)
            {
                return;
            }
//            subscribers.RemoveAll((Connection x) => this.ShouldNetworkTo(x.player as BasePlayer));
            this.OnNetworkSubscribersLeave(subscribers);
            var children = (this as BaseEntity)?.Children;
            if (children != null && children.Count > 0)
            {
                foreach (BaseEntity child in children)
                {
                    child.OnNetworkLimitStart();
                }
            }
        }
        
        private void OnNetworkLimitEnd()
            {
                this.SendNetworkUpdate();
                var children = (this as BaseEntity)?.Children;
                if (children != null && children.Count > 0)
                {
                    foreach (BaseEntity child in children)
                    {
                        child.OnNetworkLimitEnd();
                    }
                }
            }
        #endregion

        #region [Methods] Visibility

        public void OnNetworkSubscribersLeave(List<Connection> connections)
        {
            if (!NetworkManager.BaseNetworkServer.IsConnected())
            {
                return;
            }
            
            if (NetworkManager.BaseNetworkServer.write.Start())
            {
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                NetworkManager.BaseNetworkServer.write.EntityID(this.UID);
                NetworkManager.BaseNetworkServer.write.UInt8(0);
                NetworkManager.BaseNetworkServer.write.Send(new SendInfo(connections));
            }
        }


        #endregion

        #region [Method] GetSubscribers
        public virtual List<BasePlayer> GetSubscribers()
        {
            return ListViewToMe;
        }
        #endregion
        
        #region [Methods] Find Entities
        public static bool FindNetworkable(UInt32 uid, out BaseNetworkable entity)
        {
            return ListNetworkables.TryGetValue(uid, out entity);
        }

        public static bool Find<T>(UInt32 uid, out T entity)
            where T : BaseNetworkable
        {
            entity = null;
            return FindNetworkable(uid, out BaseNetworkable networkable) && 
                   (entity = (networkable as T)) != null;

        }
        #endregion
        
        #region [Method] Spawn
        public void Spawn(uint _prefabID)
        {
            this.UID = TakeUID();
            this.PrefabID = _prefabID;
            ListNetworkables[this.UID] = this;
            if (this is BasePlayer player)
                player.Network.OnPlayerInit();
            
            if (this.IsComponent == false)
                this.OnPositionChanged();
        }
        #endregion

        #region [Method] Despawn
        public void Despawn()
        {
            if (this.IsSpawned && ListNetworkables.TryGetValue(this.UID, out _))
            {
                NetworkManager.BaseNetworkServer.write.Start();
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                NetworkManager.BaseNetworkServer.write.EntityID(this.UID);
                NetworkManager.BaseNetworkServer.write.Send(new SendInfo(ListViewToMe.ToConnectionsList()));
                
                this.CurentGameZona?.UnRegistration(this);
                ListNetworkables.Remove(this.UID);
                ListViewToMe.Clear();
            }
        }
        #endregion

        #region [Method] OnDestroy
        public override void OnDestroy()
        {
            this.Despawn();
        }
        #endregion

        #region [Method] SendNetworkUpdate
        public virtual void SendNetworkUpdate(Entity _entity = null)
        {
            if (_entity == null)
            {
                _entity = new Entity();
                GetEntityProtobuf(_entity);
            }

            if (this is BasePlayer player && player.IsConnected)
            {
                this.SendNetworkUpdate(new SendInfo(player.Network.NetConnection), _entity);
                _entity.baseCombat.health = 0.01f;
            }

            if (this.ListViewToMe.Count != 0)
                this.SendNetworkUpdate(new SendInfo(this.ListViewToMe.ToConnectionsList()), _entity); 
        }
        
        public virtual void SendNetworkUpdate(SendInfo _sendInfo, Entity _entity = null)
        {
            if (_entity == null)
            {
                _entity = new Entity();
                GetEntityProtobuf(_entity);
            }
            
            #region [Section] Temporary FIX - Alistair has promised to resolve the issue with connection.validate.entityUpdates
            
            if (_sendInfo.connection == null)
            {
                var connections = _sendInfo.connections.ToArray();
                for (var i = 0; i < connections.Length; i++)
                {
                    connections[i].validate.entityUpdates++;
                    
                    NetworkManager.BaseNetworkServer.write.Start();
                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.Entities);
                    NetworkManager.BaseNetworkServer.write.UInt32(connections[i].validate.entityUpdates);
                    _entity.WriteToStream(NetworkManager.BaseNetworkServer.write);
                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo(connections[i]));
                }
                return;
            }

            #endregion
            
            NetworkManager.BaseNetworkServer.write.Start();
            NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.Entities);
            NetworkManager.BaseNetworkServer.write.UInt32(++_sendInfo.connection.validate.entityUpdates);
            _entity.WriteToStream(NetworkManager.BaseNetworkServer.write);
            NetworkManager.BaseNetworkServer.write.Send(_sendInfo);
        }
        #endregion

        #region [Method] SendNetworkPositionUpdate

        public void SendNetworkPositionUpdate()
        {
            if (this is BasePlayer player && player.IsConnected)
                this.SendNetworkPositionUpdate(new SendInfo(player.Network.NetConnection));
            
            if (this.ListViewToMe.Count != 0)
                this.SendNetworkPositionUpdate(new SendInfo(ListViewToMe.ToConnectionsList()));
        }
        public virtual void SendNetworkPositionUpdate(SendInfo _sendInfo)
        {
            if (this.ListViewToMe.Count != 0)
            {
                NetworkManager.BaseNetworkServer.write.Start();
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityPosition);
                NetworkManager.BaseNetworkServer.write.EntityID(this.UID);
                NetworkManager.BaseNetworkServer.write.Vector3(this.Position);
                NetworkManager.BaseNetworkServer.write.Vector3(this.Rotation);
                NetworkManager.BaseNetworkServer.write.Send(_sendInfo);
            }
        }
        #endregion

        #region [Method] OnPositionChanged
        public virtual void OnPositionChanged()
        {
            GameZona zona = ZonaManager.ZonaManager.GetGameZona(this.Position);
            
            if (this.CurentGameZona != zona)
            {
                zona.Registration(this);
            }
            this.SendNetworkPositionUpdate();
        }
        #endregion

        #region [Method] GetEntityProtobuf

        public virtual void GetEntityProtobuf(Entity entity)
        {
            entity.baseNetworkable = new ProtoBuf.BaseNetworkable
            {
                @group = 0,
                prefabID = this.PrefabID,
                uid = this.UID
            };
        }

        #endregion
        
        #region [Methods] ClientRPC
        private bool ClientRPCStart(Connection sourceConnection, ERPCMethodType method)
        {
            if (NetworkManager.BaseNetworkServer.write.Start())
            {
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.RPCMessage);
                NetworkManager.BaseNetworkServer.write.UInt32(this.UID);
                NetworkManager.BaseNetworkServer.write.UInt32((uint)method);
                NetworkManager.BaseNetworkServer.write.UInt64(sourceConnection?.userid ??  0UL);
                return true;
            }
            return false;
        }
        
        public void ClientRPCEx(SendInfo sendInfo, Connection sourceConnection, ERPCMethodType method)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1>(SendInfo sendInfo, Connection sourceConnection, ERPCMethodType method, T1 arg1)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1,T2>(SendInfo sendInfo, Connection sourceConnection, ERPCMethodType method, T1 arg1, T2 arg2)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCWrite<T2>(arg2);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1,T2,T3>(SendInfo sendInfo, Connection sourceConnection, ERPCMethodType method, T1 arg1, T2 arg2, T3 arg3)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCWrite<T2>(arg2);
                this.ClientRPCWrite<T3>(arg3);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1,T2,T3,T4>(SendInfo sendInfo, Connection sourceConnection, ERPCMethodType method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCWrite<T2>(arg2);
                this.ClientRPCWrite<T3>(arg3);
                this.ClientRPCWrite<T4>(arg4);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        private void ClientRPCSend(SendInfo sendInfo)
        {
            NetworkManager.BaseNetworkServer.write.Send(sendInfo);
        }
        
        private void ClientRPCWrite<T>(T arg)
        {
            NetworkManager.BaseNetworkServer.write.WriteObject<T>(arg);
        }
        #endregion
    }
}