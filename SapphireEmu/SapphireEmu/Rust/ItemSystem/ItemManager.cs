using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SapphireEmu.Data;
using SapphireEngine;

namespace SapphireEmu.Rust
{
    public static class ItemManager
    {
        public static Dictionary<Int32, ItemInformation> itemDictionary;
        public static List<ItemInformation> itemlist;


        internal static void Initialization()
        {
            string filepath = Path.Combine(BuildingInformation.DirectoryBase, "items.json");
            itemlist = JsonConvert.DeserializeObject<List<ItemInformation>>(File.ReadAllText(filepath));
            itemDictionary = itemlist.ToDictionary(item => item.ItemID, item => item);
            
            ConsoleSystem.Log($"[{nameof(ItemManager)}] Loaded <{itemlist.Count}> items!");
        }

        public static ItemInformation FindInformation(int itemid)
        {
            if (itemDictionary.TryGetValue(itemid, out ItemInformation info) == false)
            {
                return null;
            }
            return info;
        }


        public static Item CreateItem(int itemid, uint amount = 1, ulong skinid = 0)
        {
            ItemInformation info = FindInformation(itemid);
            if (info == null)
            {
                return null;
            }
            
            return Item.CreateItem(info, amount, skinid);
        }
        
        public static Item CreateByPartialName(string shortname, uint amount = 1, ulong skinid = 0)
        {
            ItemInformation iteminfo = itemlist.Find(p => p.Shortname == shortname);
            if (iteminfo == null)
            {
                return null;
            }
            
            return Item.CreateItem(iteminfo, amount, skinid);
        }
    }
}