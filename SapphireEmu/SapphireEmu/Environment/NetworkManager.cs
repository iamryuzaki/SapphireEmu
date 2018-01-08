using System;
using System.IO;
using System.Text;
using Facepunch;
using Network;
using ProtoBuf;
using SapphireEmu.Data;
using SapphireEmu.Data.Base;
using SapphireEmu.Data.Base.GObject;
using SapphireEmu.Struct.Network;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;
using BaseNetworkable = SapphireEmu.Rust.GObject.BaseNetworkable;
using BasePlayer = SapphireEmu.Rust.GObject.BasePlayer;
using Message = Network.Message;
using Server = Facepunch.Network.Raknet.Server;

namespace SapphireEmu.Environment
{
    public class NetworkManager : SapphireType
    {
        public static NetworkManager Instance { get; private set; }
        public static Facepunch.Network.Raknet.Server BaseNetworkServer { get; private set; }

        private MemoryStream m_queryBuffer = new MemoryStream();
        
        #region [SapphireEngine Hook] [Example] OnAwake
        public override void OnAwake()
        {
            Instance = this;
            BaseNetworkServer  = new Server();
            BaseNetworkServer.port = Settings.GamePort;
            BaseNetworkServer.ip = Settings.ServerIP;
            BaseNetworkServer.onUnconnectedMessage = OnUnconnectedMessage;
            BaseNetworkServer.onMessage = OnMessage;
            BaseNetworkServer.onDisconnected = OnDisconnected;
            BaseNetworkServer.Start();
        }
        #endregion

        #region [SapphireEngine Hook] [Example] OnUpdate
        public override void OnUpdate()
        {
            EACManager.DoUpdate();
            BaseNetworkServer.Cycle();
        }
        #endregion

        #region [SapphireEngine Hook] [Example] OnShutdown
        private void OnShutdown()
        {
            if (BaseNetworkServer != null)
            {
                BaseNetworkServer.Stop("Server is shutdown!");
            }
        }
        #endregion

        #region [Method] [Example] OnDisconnected
        private void OnDisconnected(string _reasone, Connection _connection)
        {
            EACManager.OnLeaveGame(_connection);
            BasePlayer player = Extended.Rust.ToPlayer(_connection);
            if (player != null)
                player.OnDisconnected();
            ConsoleSystem.Log(string.Format(Data.Base.Message.Network_Connection_NewDisconnected, _connection.userid, _connection.username, _reasone));
        }
        #endregion

        #region [Method] [Example] OnMessage
        private void OnMessage(Message _message)
        {
            switch (_message.type)
            {
                #region [Section] [Case] Message.Type.GiveUserInformation
                case Message.Type.GiveUserInformation:
                    this.OnGiveUserInformation(_message);
                    break;
                #endregion
                #region [Section] [Case] Message.Type.Ready
                case Message.Type.Ready:
                    this.OnClientReady(_message);
                    break;
                #endregion
                #region [Section] [Case] Message.Type.RPCMessage
                case Message.Type.RPCMessage:
                    this.OnRPCMessage(_message);
                    break;
                #endregion
                #region [Section] [Case] Message.Type.Tick
                case Message.Type.Tick:
                    this.OnPlayerTick(_message);
                    break;
                #endregion
                #region [Section] [Case] Message.Type.EAC
                case Message.Type.EAC:
                    EACManager.OnMessageReceived(_message);
                    break;
                #endregion
            }
        }
        #endregion

        #region [Method] [Example] OnUnconnectedMessage
        private bool OnUnconnectedMessage(int _type, Read _read, uint _ip, int _port)
        {
            try
            {
                if (_type != 255 || _read.UInt8() != 255 || _read.UInt8() != 255 || _read.UInt8() != 255)
                    return false;

                _read.Position = 0L;
                int unread = _read.unread;

                if (unread > 4096)
                    return true;

                if (this.m_queryBuffer.Capacity < unread)
                    this.m_queryBuffer.Capacity = unread;

                int size = _read.Read(this.m_queryBuffer.GetBuffer(), 0, unread);
                SteamworksManager.BaseSteamServer.Query.Handle(this.m_queryBuffer.GetBuffer(), size, _ip, (ushort) _port);
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("[NetworkManager]: Exception to NetworkManager.OnUnconnectedMessage: " + ex);
            }
            return true;
        }
        #endregion

        #region [Method] [Example] OnPlayerTick
        private void OnPlayerTick(Message _message)
        {
            BasePlayer player = Extended.Rust.ToPlayer(_message);
            if (player != null)
            {
                player.OnReceivedTick(_message);
            }
        }
        #endregion

        #region [Method] [Example] OnRPCMessage
        private void OnRPCMessage(Message _message)
        {
            uint uid = _message.read.EntityID();
            uint methodUID = _message.read.UInt32();
            Data.Base.Network.Run(uid, (Data.Base.Network.RPCMethod.ERPCMethodType)methodUID, _message);
        }
        #endregion

        #region [Method] [Example] OnClientReady
        private void OnClientReady(Message _message)
        {
            _message.connection.decryptIncoming = true;
            using (ClientReady ready = ClientReady.Deserialize(_message.read))
            {
                foreach (ClientReady.ClientInfo info in ready.clientInfo)
                    _message.connection.info.Set(info.name, info.value);

                BasePlayer player = Extended.Rust.ToPlayer(_message);
                if (player == null)
                {
                    player = this.AddType<BasePlayer>();
                    player.SteamID = _message.connection.userid;
                    player.Position = new Vector3(0,215,0);
                    BasePlayer.ListPlayers.Add(player.SteamID, player);
                }
                
                player.OnConnected(_message.connection); 
            }
        }
        #endregion
        
        #region [Method] [Example] OnGiveUserInformation
        private void OnGiveUserInformation(Message _message)
        {
            var packet = UserGiveInformation.ParsePacket(_message);
            _message.connection.userid = packet.SteamID;
            _message.connection.username = packet.Username;
            _message.connection.os = packet.OS;
            _message.connection.protocol = packet.ConnectionProtocol;
            _message.connection.token = packet.SteamToken;

//### Temporary disabled - By ~ TheRyuzaki, this code is connection version variable. ###       
//            if (_message.connection.protocol > Settings.GameVersion)
//            {
//                BaseNetworkServer.Kick(_message.connection, Data.Base.Message.Network_Connection_BadVersion_Server);
//                return;
//            }
//            if (_message.connection.protocol < Settings.GameVersion)
//            {
//                BaseNetworkServer.Kick(_message.connection, Data.Base.Message.Network_Connection_BadVersion_Client);
//                return;
//            }
            ConsoleSystem.Log(String.Format(Data.Base.Message.Network_Connection_NewConnection, _message.connection.userid, _message.connection.username));
            SteamworksManager.Instance.OnUserAuth(_message.connection);
        }
        #endregion

        #region [Method] OnUserAuthSuccess
        public void OnUserAuthSuccess(Connection _connection)
        {
            using (Approval approval = new Approval())
            {
                uint encryption = (uint) Settings.NetworkEncryptionLevel;
                approval.level = Settings.MapName;
                approval.levelSeed = (uint)Settings.MapSeed;
                approval.levelSize = (uint)Settings.MapSize;
                approval.checksum = "null";
                approval.hostname = Settings.Hostname;
                approval.official = false;
                approval.encryption = encryption;
                if (BaseNetworkServer.write.Start())
                {
                    BaseNetworkServer.write.PacketID(Message.Type.Approved);
                    approval.WriteToStream(BaseNetworkServer.write);
                    BaseNetworkServer.write.Send(new SendInfo(_connection));
                }
                _connection.encryptionLevel = encryption;
                _connection.encryptOutgoing = true;
            }
            _connection.connected = true;
            ConsoleSystem.Log(String.Format(Data.Base.Message.Network_Connection_NewConnection_Authed, _connection.userid, _connection.username));
        }
        #endregion
    }
}