using System.IO;
using System.Linq;
using SapphireEmu.Data;
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
            Item item = ItemManager.CreateByItemID(arg.GetInt(0, 0), arg.GetUInt(1, 1));
            if (item == null)
            {
                arg.ReplyWith("Invalid Item!");
                return;
            }
            player.Inventory.ContainerBelt.AddItemToContainer(item);
            player.Inventory.ContainerBelt.OnItemConainerUpdate();
        }

        [ConsoleCommand("inventory.itemids", IsAdmin = true)]
        public static void ItemIDs(ConsoleNetwork.Arg arg)
        {
            string UppercaseFirst(string s)
            {
                // Check for empty string.
                if (string.IsNullOrEmpty(s))
                {
                    return string.Empty;
                }
                // Return char and concat substring.
                return char.ToUpper(s[0]) + s.Substring(1);
            }

            string filename = Path.Combine(BuildingInformation.DirectoryLogs, "ItemIDs.txt");
            
            var lines = ItemManager.itemlist.Select(p =>
                $"{string.Join("", p.Shortname.Split('.').Select(UppercaseFirst).ToArray())} = {p.ItemID},").ToArray();
            File.WriteAllText(filename,
                string.Join(System.Environment.NewLine, lines));
            
            arg.ReplyWith($"Success! Result <{filename}>");
        }
    }
}