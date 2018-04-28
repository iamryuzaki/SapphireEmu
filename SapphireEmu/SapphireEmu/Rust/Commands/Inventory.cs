using SapphireEmu.Environment;
using SapphireEmu.Extended;

namespace SapphireEmu.Rust.Commands
{
    public class Inventory
    {
        [ConsoleCommand("inventory.giveid")]
        public static void GiveId(ConsoleNetwork.Arg arg)
        {
            var player = arg.Connection.ToPlayer();
            Item item = ItemManager.CreateItem(arg.GetInt(0, 0), arg.GetUInt(1, 1));
            if (item == null)
            {
                arg.ReplyWith("Invalid Item!");
                return;
            }
            player.Inventory.ContainerBelt.AddItemToContainer(item);
            player.Inventory.ContainerBelt.OnItemConainerUpdate();
        }
    }
}