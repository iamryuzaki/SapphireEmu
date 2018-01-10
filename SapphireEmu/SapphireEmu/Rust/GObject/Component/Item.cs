using SapphireEmu.Data.Base.GObject.Component;

namespace SapphireEmu.Rust.GObject.Component
{
    public class Item
    {
        public static Item CreateItem(int itemid, ItemContainer container = null) => new Item(itemid, container);
        
        public uint UID;
        
        public int ItemID;
        public float MaxCondition = 100f;
        public float Condition = 100f;
        public int Amount = 1;
        public int Position = 0;

        public ItemContainer Container = null;
        
        public E_ItemFlags ItemFlags = E_ItemFlags.None;

        private Item(int itemid, ItemContainer container)
        {
            this.ItemID = itemid;
            this.UID = BaseNetworkable.TakeUID();
            this.Container = container;

            Container?.AddItemToContainer(this);
        }

        public ProtoBuf.Item GetProtobufObject()
        {
            ProtoBuf.Item item = new ProtoBuf.Item();

            return item;
        }
    }
}