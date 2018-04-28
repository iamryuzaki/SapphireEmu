using System;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using SapphireEmu.Data;
using SapphireEmu.Data.Base;
using SapphireEmu.Rust.ZonaManager;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;

namespace SapphireEmu.Environment
{
    public class ApplicationManager : SapphireType
    {
        public static void Initialization() => Framework.Initialization<ApplicationManager>();

        public override void OnAwake()
        {
            ConsoleSystem.OutputPath = BuildingInformation.DirectoryLogs + "/output.log";
            this.Database_Initialization();
            ZonaManager.Initialization();
            ItemManager.Initialization();
            this.AddType<NetworkManager>();
            this.AddType<SteamworksManager>();
        }

        public override void OnUpdate()
        {
            
        }

        private void OnShutdown()
        {
            
        }
        
        private void Database_Initialization()
        {
            Data.Base.DefaultMessages.Load();
            Data.Base.Network.Load();
            Data.Base.ConsoleNetwork.Load();
        }
        
    }
}