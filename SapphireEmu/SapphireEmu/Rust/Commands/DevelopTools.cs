using SapphireEmu.Environment;
using SapphireEmu.Rust.GObject;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;

namespace SapphireEmu.Rust.Commands
{
    public class DevelopTools
    {
        [ConsoleCommand("spawnbot", IsAdmin = true)]
        static void SpawnBot()
        {
            var player = Framework.Bootstraper.AddType<BasePlayer>();
            player.SteamID = (ulong)(new System.Random()).Next(0, 1000000);
            player.Position = new Vector3(0,4,0);
            player.PlayerModelState.sleeping = false;
            player.PlayerModelState.onground = true;
            player.SetPlayerFlag(E_PlayerFlags.Sleeping, false);
            player.Spawn((uint) Data.Base.PrefabID.BasePlayer);

            player.Inventory.ContainerBelt.AddItemToContainer(ItemManager.CreateByPartialName("rifle.ak"));
            player.OnChangeActiveItem(player.Inventory.ContainerBelt.ListItems[0]);
            player.SendNetworkUpdate();
//            Timer.SetInterval(() =>
//            {
//                player.SignalBroadcast(BaseEntity.E_Signal.Alt_Attack);
//                ConsoleSystem.Log($"count => {player.ListViewToMe.Count}");
//            }, 1f);
        }
    }
}