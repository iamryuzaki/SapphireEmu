using SapphireEmu.Data;
using SapphireEmu.Data.Base;
using SapphireEmu.Rust;
using SapphireEmu.Rust.ZonaManager;
using SapphireEngine;

namespace SapphireEmu.Environment
{
    public class ApplicationManager : SapphireType
    {
        public static void Initialization() => Framework.Initialization<ApplicationManager>();

        public override void OnAwake()
        {
            ConsoleSystem.OutputPath = BuildingInformation.DirectoryLogs + "/output.log";
            ConsoleSystem.OnConsoleInput += ConsoleNetwork.OnServerCommand;
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
            DefaultMessages.Load();
            RPCNetwork.Load();
            ConsoleNetwork.Load();
        }
        
    }
}