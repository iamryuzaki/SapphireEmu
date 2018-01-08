using System;
using Network;
using SapphireEmu.Data;
using SapphireEngine;
using SapphireEngine.Functions;

namespace SapphireEmu.Environment
{
    public class SteamworksManager : SapphireType
    {
        public static SteamworksManager Instance { get; private set; }
        public static Facepunch.Steamworks.Server BaseSteamServer { get; private set; }
        
        private float m_ticked_updateSteamworksInformation = 0f;

        #region [SapphireEngine Hook] [Example] OnAwake
        public override void OnAwake()
        {
            Instance = this;
            var init = new Facepunch.Steamworks.ServerInit(BuildingInformation.SteamworksModDir, BuildingInformation.SteamworksGameDesc)
            {
                GamePort = (ushort)Settings.GamePort,
                IpAddress = IPAddres.GetUintFromIP(Settings.ServerIP),
                Secure = Settings.Secure,
                VersionString = Settings.GameVersion.ToString(),
            };
            BaseSteamServer = new Facepunch.Steamworks.Server(BuildingInformation.SteamworksAppID, init);
            if (BaseSteamServer.IsValid)
            {
                BaseSteamServer.LogOnAnonymous();
                Timer.SetInterval(OnUpdateServerInformation, 10f);
            }
            else
            {
                BaseSteamServer.Dispose();
                BaseSteamServer = null;
                ConsoleSystem.LogWarning("[SteamworksManager]: Steamworks is not initialized!");
            }
        }
        #endregion

        #region [SapphireEngine Hook] [Example] OnUpdate
        public override void OnUpdate()
        {
            BaseSteamServer.RunUpdateCallbacks();
            this.UpdateSteamQueryResponse();
                
            this.m_ticked_updateSteamworksInformation += DeltaTime;
            if (this.m_ticked_updateSteamworksInformation >= 10f)
            {
                this.m_ticked_updateSteamworksInformation = 0f;
                this.OnUpdateServerInformation();
            }
        }
        #endregion
        
        #region [SapphireEngine Hook] [Example] OnShutdown
        private void OnShutdown()
        {
            if (BaseSteamServer != null)
            {
                BaseSteamServer.Dispose();
                BaseSteamServer = null;
            }
        }
        #endregion

        #region [Method] [Example] OnUpdateServerInformation
        private void OnUpdateServerInformation()
        {
            try
            {
                BaseSteamServer.ServerName = Settings.Hostname;

                BaseSteamServer.MaxPlayers = Settings.Maxplayers;
                BaseSteamServer.Passworded = false;
                BaseSteamServer.MapName = Settings.MapName;

                BaseSteamServer.GameTags = string.Format("mp{0},cp{1},qp{5},v{2}{3},h{4},{6},{7},oxide,modded", new object[]
                {
                    Settings.Maxplayers,
                    0, 
                    Settings.GameVersion,
                    string.Empty,
                    "null",
                    0,
                    "stok",
                    "born1"
                });

                BaseSteamServer.SetKey("hash", "null");
                BaseSteamServer.SetKey("world.seed", Settings.MapSeed.ToString());
                BaseSteamServer.SetKey("world.size", Settings.MapSize.ToString());
                BaseSteamServer.SetKey("pve", "False");
                BaseSteamServer.SetKey("headerimage", Settings.ServerImage);
                BaseSteamServer.SetKey("url", Settings.ServerURL);
                BaseSteamServer.SetKey("uptime", ((int) DateTime.Now.Subtract(Framework.StartTimeApplication).TotalSeconds).ToString());
                BaseSteamServer.SetKey("gc_mb", "30");
                BaseSteamServer.SetKey("gc_cl", "30");
                BaseSteamServer.SetKey("fps", Framework.FPSLimit.ToString());
                BaseSteamServer.SetKey("fps_avg", Framework.FPSLimit.ToString());
                BaseSteamServer.SetKey("ent_cnt", "1");
                BaseSteamServer.SetKey("build", BuildingInformation.ApplicationVersion + ", build " + BuildingInformation.ApplicationBuild );
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("[Steamworks] Exception to SteamworksManager.OnUpdateSteamworksInformation(): " + ex);
            }
        }
        #endregion
        
        #region [Method] [Example] UpdateSteamQueryResponse
        
        private void UpdateSteamQueryResponse()
        {
            Facepunch.Steamworks.ServerQuery.Packet packet;
            while (BaseSteamServer.Query.GetOutgoingPacket(out packet))
            {
                NetworkManager.BaseNetworkServer.SendUnconnected(packet.Address, packet.Port, packet.Data, packet.Size);
            }
        }

        #endregion

        #region [Method] OnUserAuth
        public void OnUserAuth(Connection _connection)
        {
            bool authResult = BaseSteamServer.Auth.StartSession(_connection.token, _connection.userid);
            if (Settings.NoSteam == true || authResult == true)
            {
                NetworkManager.Instance.OnUserAuthSuccess(_connection);
            } else
                NetworkManager.BaseNetworkServer.Kick(_connection, "Steam auth failed");
        }
        #endregion
    }
}