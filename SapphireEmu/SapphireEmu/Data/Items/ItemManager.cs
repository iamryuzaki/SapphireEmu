using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SapphireEmu.Data.Base
{
    public static class ItemManager
    {
        public static Dictionary<Int32, ItemInformation> itemDictionary;
        public static List<ItemInformation> itemlist;


        internal static void Load()
        {
            string filepath = Path.Combine(BuildingInformation.DirectoryBase, "items.json");
            itemlist = JsonConvert.DeserializeObject<List<ItemInformation>>(filepath);
            itemDictionary = itemlist.ToDictionary(item => item.ItemID, item => item);
        }
    }
}