using SapphireEmu.Data.Base.GObject.Component;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject.Component
{
    public class Item
    {
        public static Item CreateItem(int itemid, int amount = 1) => new Item(itemid, amount);
        
        public uint UID;
        
        public int ItemID;
        public float MaxCondition = 50f;
        public float Condition = 50f;
        public int Amount = 1;
        public int Position = 0;

        public ItemContainer Container = null;
        
        public E_ItemFlags ItemFlags = E_ItemFlags.None;

        private Item(int itemid, int amount)
        {
            this.ItemID = itemid;
            this.UID = BaseNetworkable.TakeUID();
            this.Amount = amount;
        }

        public ProtoBuf.Item GetProtobufObject()
        {
            ProtoBuf.Item item = new ProtoBuf.Item
            {
                UID = this.UID,
                amount = this.Amount,
                conditionData = new ProtoBuf.Item.ConditionData { condition = this.Condition, maxCondition = this.MaxCondition },
                itemid = this.ItemID,
                slot = this.Position
            };         
            return item;
        }
    }
}