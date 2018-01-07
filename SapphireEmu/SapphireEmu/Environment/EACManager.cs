using EasyAntiCheat.Server;
using EasyAntiCheat.Server.Hydra;
using EasyAntiCheat.Server.Hydra.Cerberus;
using EasyAntiCheat.Server.Scout;
using Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SapphireEmu.Data;
using SapphireEngine;
using UnityEngine;

namespace SapphireEmu.Environment
{
    public class EACManager
    {
        [CompilerGenerated] 
        private static EasyAntiCheatServer<EasyAntiCheat.Server.Hydra.Client>.ClientStatusHandler HandlerClientStatusHandler;
        private static Dictionary<EasyAntiCheat.Server.Hydra.Client, Connection> client2connection = new Dictionary<EasyAntiCheat.Server.Hydra.Client, Connection>();
        private static Dictionary<Connection, EasyAntiCheat.Server.Hydra.Client> connection2client = new Dictionary<Connection, EasyAntiCheat.Server.Hydra.Client>();
        private static Dictionary<Connection, ClientStatus> connection2status = new Dictionary<Connection, ClientStatus>();
        public static EasyAntiCheat.Server.Scout.Scout eacScout;
        private static EasyAntiCheatServer<EasyAntiCheat.Server.Hydra.Client> easyAntiCheat = null;
        public static ICerberus playerTracker;

        public static void Decrypt(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
        {
            easyAntiCheat.NetProtect.UnprotectMessage(GetClient(connection), src, (long) srcOffset, dst, (long) dstOffset);
        }

        public static void DoShutdown()
        {
            client2connection.Clear();
            connection2client.Clear();
            connection2status.Clear();
            if (eacScout != null)
            {
                ConsoleSystem.Log("EasyAntiCheat Scout Shutting Down");
                eacScout.Dispose();
                eacScout = null;
            }
            if (easyAntiCheat != null)
            {
                ConsoleSystem.Log("EasyAntiCheat Server Shutting Down");
                easyAntiCheat.Dispose();
                easyAntiCheat = null;
            }
        }

        public static void DoStartup()
        {
            client2connection.Clear();
            connection2client.Clear();
            connection2status.Clear();
            StreamWriter writer = new StreamWriter(BuildingInformation.DirectoryLogs + "/Log.EAC.txt", true)
            {
                AutoFlush = true
            };
            Log.SetOut(writer);
            Log.Prefix = string.Empty;
            Log.Level = LogLevel.Info;
            if (HandlerClientStatusHandler == null)
                HandlerClientStatusHandler = EACManager.HandleClientUpdate;
            easyAntiCheat = new EasyAntiCheatServer<EasyAntiCheat.Server.Hydra.Client>(HandlerClientStatusHandler, 20, Settings.Hostname);
            playerTracker = easyAntiCheat.Cerberus;
            playerTracker.LogGameRoundStart(string.Concat(new object[] {Settings.MapName, "_", Settings.MapSize, "_", Settings.MapSeed}));
            eacScout = new EasyAntiCheat.Server.Scout.Scout();
        }

        public static void DoUpdate()
        {
            if (easyAntiCheat != null)
            {
                easyAntiCheat.HandleClientUpdates();
                if ((NetworkManager.BaseNetworkServer != null) && NetworkManager.BaseNetworkServer.IsConnected())
                {
                    EasyAntiCheat.Server.Hydra.Client client;
                    byte[] buffer;
                    int num;
                    while (easyAntiCheat.PopNetworkMessage(out client, out buffer, out num))
                    {
                        SendToClient(client, buffer, num);
                    }
                }
            }
        }

        public static void Encrypt(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
        {
            easyAntiCheat.NetProtect.ProtectMessage(GetClient(connection), src, (long) srcOffset, dst, (long) dstOffset);
        }

        public static EasyAntiCheat.Server.Hydra.Client GetClient(Connection connection)
        {
            EasyAntiCheat.Server.Hydra.Client client;
            connection2client.TryGetValue(connection, out client);
            return client;
        }

        public static Connection GetConnection(EasyAntiCheat.Server.Hydra.Client client)
        {
            Connection connection;
            client2connection.TryGetValue(client, out connection);
            return connection;
        }

        private static void HandleClientUpdate(ClientStatusUpdate<EasyAntiCheat.Server.Hydra.Client> clientStatus)
        {
            using (TimeWarning.New("AntiCheatKickPlayer", (long) 10L))
            {
                EasyAntiCheat.Server.Hydra.Client clientObject = clientStatus.ClientObject;
                Connection connection = GetConnection(clientObject);
                if (connection == null)
                {
                    ConsoleSystem.LogError("EAC status update for invalid client: " + clientObject.ClientID);
                }
                else if (!HasIgnore(connection))
                {
                    if (clientStatus.RequiresKick)
                    {
                        string message = clientStatus.Message;
                        if (string.IsNullOrEmpty(message))
                        {
                            message = clientStatus.Status.ToString();
                        }
                        ConsoleSystem.Log(string.Concat(new object[] {"[EAC] Kicking ", connection.userid, " (", message, ")"}));
                        connection.authStatus = "eac";
                        NetworkManager.BaseNetworkServer.Kick(connection, "EAC: " + message);
                        if (clientStatus.Status == ClientStatus.ClientBanned)
                        {
                            // TODO: Need write GameBanned function use
                        }
                        easyAntiCheat.UnregisterClient(clientObject);
                        client2connection.Remove(clientObject);
                        connection2client.Remove(connection);
                        connection2status.Remove(connection);
                    }
                    else if (clientStatus.Status == ClientStatus.ClientAuthenticatedLocal)
                    {
                        OnAuthenticatedLocal(connection);
                        easyAntiCheat.SetClientNetworkState(clientObject, false);
                    }
                    else if (clientStatus.Status == ClientStatus.ClientAuthenticatedRemote)
                    {
                        OnAuthenticatedRemote(connection);
                    }
                }
            }
        }

        public static bool IsAuthenticated(Connection connection)
        {
            ClientStatus status;
            connection2status.TryGetValue(connection, out status);
            return (status == ClientStatus.ClientAuthenticatedRemote);
        }

        private static void OnAuthenticatedLocal(Connection connection)
        {
            if (connection.authStatus == string.Empty)
            {
                connection.authStatus = "ok";
            }
            connection2status[connection] = ClientStatus.ClientAuthenticatedLocal;
        }

        private static void OnAuthenticatedRemote(Connection connection)
        {
            connection2status[connection] = ClientStatus.ClientAuthenticatedRemote;
        }

        public static void OnFinishLoading(Connection connection)
        {
            if (easyAntiCheat != null)
            {
                EasyAntiCheat.Server.Hydra.Client clientObject = GetClient(connection);
                easyAntiCheat.SetClientNetworkState(clientObject, true);
            }
        }

        public static void OnJoinGame(Connection connection)
        {
            if (easyAntiCheat != null)
            {
                EasyAntiCheat.Server.Hydra.Client clientObject = easyAntiCheat.GenerateCompatibilityClient();
                easyAntiCheat.RegisterClient(clientObject, connection.userid.ToString(), connection.ipaddress, connection.ownerid.ToString(), connection.username, (connection.authLevel <= 0) ? PlayerRegisterFlags.PlayerRegisterFlagNone : PlayerRegisterFlags.PlayerRegisterFlagAdmin);
                client2connection.Add(clientObject, connection);
                connection2client.Add(connection, clientObject);
                connection2status.Add(connection, ClientStatus.ClientDisconnected);
                if (HasIgnore(connection))
                {
                    OnAuthenticatedLocal(connection);
                    OnAuthenticatedRemote(connection);
                }
            }
            else
            {
                OnAuthenticatedLocal(connection);
                OnAuthenticatedRemote(connection);
            }
        }

        public static void OnLeaveGame(Connection connection)
        {
            if (easyAntiCheat != null)
            {
                EasyAntiCheat.Server.Hydra.Client clientObject = GetClient(connection);
                easyAntiCheat.UnregisterClient(clientObject);
                client2connection.Remove(clientObject);
                connection2client.Remove(connection);
                connection2status.Remove(connection);
            }
        }

        public static void OnMessageReceived(Network.Message message)
        {
            if (!connection2client.ContainsKey(message.connection))
            {
                ConsoleSystem.LogError("EAC network packet from invalid connection: " + message.connection.userid);
            }
            else
            {
                EasyAntiCheat.Server.Hydra.Client clientObject = GetClient(message.connection);
                MemoryStream stream = message.read.MemoryStreamWithSize();
                easyAntiCheat.PushNetworkMessage(clientObject, stream.GetBuffer(), (int) stream.Length);
            }
        }

        public static void OnStartLoading(Connection connection)
        {
            if (easyAntiCheat != null)
            {
                EasyAntiCheat.Server.Hydra.Client clientObject = GetClient(connection);
                easyAntiCheat.SetClientNetworkState(clientObject, false);
            }
        }

        private static void SendToClient(EasyAntiCheat.Server.Hydra.Client client, byte[] message, int messageLength)
        {
            Connection connection = GetConnection(client);
            if (connection == null)
            {
                ConsoleSystem.LogError("EAC network packet for invalid client: " + client.ClientID);
            }
            else if (NetworkManager.BaseNetworkServer.write.Start())
            {
                NetworkManager.BaseNetworkServer.write.PacketID(Network.Message.Type.EAC);
                NetworkManager.BaseNetworkServer.write.UInt32((uint) messageLength);
                NetworkManager.BaseNetworkServer.write.Write(message, 0, messageLength);
                NetworkManager.BaseNetworkServer.write.Send(new SendInfo(connection));
            }
        }

        public static bool HasIgnore(Connection connection)
        {
            return (!Settings.Secure || (connection.authLevel >= 3) || Settings.NoSteam);
        }
    }
}