using SapphireEmu.Environment;
using SapphireEmu.Rust.GObject;
using SapphireEngine;
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
            BasePlayer.ListPlayers.Add(player.SteamID, player);
            player.Spawn((uint) Data.Base.PrefabID.BasePlayer);
            player.SetPlayerFlag(E_PlayerFlags.Sleeping, false);
            player.SendNetworkUpdate_PlayerFlags();
            
            player.Inventory.ContainerBelt.AddItemToContainer(ItemManager.CreateByPartialName("rifle.ak"));
            player.Inventory.ContainerBelt.OnItemConainerUpdate();
            player.ActiveItem = player.Inventory.ContainerBelt.ListItems[0];
            player.PlayerModelState = E_PlayerModelState.OnGround;
            player.ActiveItem.HeldEntity.SetHeld(true);
            player.ActiveItem.HeldEntity.SendNetworkUpdate();
            player.SendNetworkUpdate();
        }
    }
}