using System;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using SapphireEmu.Data;
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
            Data.Base.Message.Load();
            Data.Base.Network.Load();
        }
        
    }
}