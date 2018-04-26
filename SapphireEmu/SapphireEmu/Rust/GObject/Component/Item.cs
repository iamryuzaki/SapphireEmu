using System.Collections.Generic;
using SapphireEmu.Data.Base;
using SapphireEmu.Data.Base.GObject.Component;
using SapphireEmu.Environment;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject.Component
{
    public class Item
    {
        public static Dictionary<uint, Item> ListItemsInWorld = new Dictionary<uint, Item>();
        
        public static Item CreateItem(ItemID item, uint amount = 1) => new Item(item, amount);

        public uint UID { get; }

        public float Condition { get; private set; } = 50f;
        public uint Amount { get; private set; }
        public int PositionInContainer = -1;
        public uint HeldEnityUID => this.HeldEntity?.UID ?? 0;

        public ItemInformation Information { get; }
        
        public ItemContainer Container { get; set; } = null;
        public BaseHeldEntity HeldEntity { get; private set; } = null;

        public E_ItemFlags ItemFlags = E_ItemFlags.None;

        private Item(ItemID _item, uint _amount)
        {
            if (ItemInformation.ListPrefabsFromItemIds.TryGetValue((ItemID) _item, out var itemInformation))
            {
                this.UID = BaseNetworkable.TakeUID();
                ListItemsInWorld[this.UID] = this;
                
                this.Information = itemInformation;

                this.Condition = this.Information.MaxCondition;
                
                if (_amount <= 0)
                    this.Amount = 1;
                else if (_amount > this.Information.MaxStack)
                    this.Amount = this.Information.MaxStack;
                else
                    this.Amount = _amount;

                if (this.Information.CanHeldEntity)
                {
                    this.HeldEntity = Framework.Bootstraper.AddType<BaseHeldEntity>();
                    this.HeldEntity.ItemOwner = this;
                    this.HeldEntity.IsComponent = true;
                    this.HeldEntity.Spawn((uint)this.Information.PrefabUID);
                }
            }
        }

        #region [Method] [Example] GetProtobufObject
        public ProtoBuf.Item GetProtobufObject()
        {
            ProtoBuf.Item item = new ProtoBuf.Item
            {
                UID = this.UID,
                amount = (int)this.Amount,
                conditionData = new ProtoBuf.Item.ConditionData { condition = this.Condition, maxCondition = this.Information.MaxCondition },
                itemid = (int)this.Information.ItemID,
                slot = this.PositionInContainer,
                contents = null,
                flags = (int)ItemFlags,
                heldEntity = HeldEnityUID
            };         
            return item;
        }
        #endregion
    }
}