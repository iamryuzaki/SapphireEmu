using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Rust.Commands
{
    public class Player
    {
        [ConsoleCommand("respawn")]
        public static void Respawn(ConsoleNetwork.Arg arg)
        {
            var player = arg.Connection.ToPlayer();
            if (player == null)
            {
                ConsoleSystem.LogError("[CMD=respawn] player missing!");
                return;
            }
            if (player.IsAlive)
            {
                return;
            }
            player.Respawn();
        }
    }
}