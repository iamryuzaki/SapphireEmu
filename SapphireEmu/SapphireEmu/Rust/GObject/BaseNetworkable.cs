using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Network;
using ProtoBuf;
using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEmu.Rust.ZonaManager;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseNetworkable : SapphireType
    {
        public static Dictionary<UInt32, BaseNetworkable> ListNetworkables = new Dictionary<uint, BaseNetworkable>();
        private static UInt32 m_LastNetworkableUID = 0;
        
        public UInt32 UID { get; private set; } = 0;
        public UInt32 PrefabID { get; private set; }
        public Boolean IsSpawned => this.UID != 0;
        
        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public GameZona CurentGameZona = null;
        
        public List<BasePlayer> ListViewToMe = new List<BasePlayer>();

        public void Spawn(uint prefabID)
        {
            m_LastNetworkableUID++;
            this.UID = m_LastNetworkableUID;
            this.PrefabID = prefabID;
            ListNetworkables[this.UID] = this;
            this.OnPositionChanged();
            this.SendNetworkUpdate();
        }

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

        public override void OnDestroy()
        {
            this.Despawn();
        }

        #region [Method] SendNetworkUpdate

        public void SendNetworkUpdate(Entity entity = null)
        {
            if (this.ListViewToMe.Count != 0)
            {
                this.SendNetworkUpdate(new SendInfo(this.ListViewToMe.ToConnectionsList()));
            }
        }
        
        public virtual void SendNetworkUpdate(SendInfo sendInfo, Entity entity = null)
        {
            ConsoleSystem.Log("SendNetworkUpdate");
            if (entity == null)
                entity = GetEntityProtobuf();
            
            #region [Section] Tempolary FIX
            
            if (sendInfo.connection == null)
            {
                var connections = sendInfo.connections.ToArray();
                for (var i = 0; i < connections.Length; i++)
                {
                    connections[i].validate.entityUpdates++;
                    
                    NetworkManager.BaseNetworkServer.write.Start();
                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.Entities);
                    NetworkManager.BaseNetworkServer.write.UInt32(connections[i].validate.entityUpdates);
                    entity.WriteToStream(NetworkManager.BaseNetworkServer.write);
                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo(connections[i]));
                }
                return;
            }

            #endregion
            
            NetworkManager.BaseNetworkServer.write.Start();
            NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.Entities);
            NetworkManager.BaseNetworkServer.write.UInt32(++sendInfo.connection.validate.entityUpdates);
            entity.WriteToStream(NetworkManager.BaseNetworkServer.write);
            NetworkManager.BaseNetworkServer.write.Send(sendInfo);
        }
        #endregion

        #region [Method] SendNetworkPositionUpdate

        public void SendNetworkPositionUpdate()
        {
            if (this.ListViewToMe.Count != 0)
            {
                this.SendNetworkPositionUpdate(new SendInfo(ListViewToMe.ToConnectionsList()));
            }
        }
        public virtual void SendNetworkPositionUpdate(SendInfo sendInfo)
        {
            if (this.ListViewToMe.Count != 0)
            {
                NetworkManager.BaseNetworkServer.write.Start();
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityPosition);
                NetworkManager.BaseNetworkServer.write.EntityID(this.UID);
                NetworkManager.BaseNetworkServer.write.Vector3(this.Position);
                NetworkManager.BaseNetworkServer.write.Vector3(this.Rotation);
                NetworkManager.BaseNetworkServer.write.Send(sendInfo);
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

        public virtual Entity GetEntityProtobuf()
        {
            var entity = new Entity
            {
                baseNetworkable = new ProtoBuf.BaseNetworkable
                {
                    @group = this.CurentGameZona.UID,
                    prefabID = this.PrefabID,
                    uid = this.UID
                }
            };
            return entity;
        }

        #endregion
        
        #region [Methods] ClientRPC
        private bool ClientRPCStart(Connection sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType method)
        {
            if (NetworkManager.BaseNetworkServer.write.Start())
            {
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.RPCMessage);
                NetworkManager.BaseNetworkServer.write.UInt32(this.UID);
                NetworkManager.BaseNetworkServer.write.UInt32((uint)method);
                NetworkManager.BaseNetworkServer.write.UInt64((sourceConnection != null) ? sourceConnection.userid : ((ulong) 0L));
                return true;
            }
            return false;
        }
        
        public void ClientRPCEx(SendInfo sendInfo, Connection sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType method)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1>(SendInfo sendInfo, Connection sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType method, T1 arg1)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1,T2>(SendInfo sendInfo, Connection sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType method, T1 arg1, T2 arg2)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCWrite<T2>(arg2);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1,T2,T3>(SendInfo sendInfo, Connection sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType method, T1 arg1, T2 arg2, T3 arg3)
        {
            if (NetworkManager.BaseNetworkServer.IsConnected() && this.ClientRPCStart(sourceConnection, method))
            {
                this.ClientRPCWrite<T1>(arg1);
                this.ClientRPCWrite<T2>(arg2);
                this.ClientRPCWrite<T3>(arg3);
                this.ClientRPCSend(sendInfo);
            }
        }
        
        public void ClientRPCEx<T1,T2,T3,T4>(SendInfo sendInfo, Connection sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
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