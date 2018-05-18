using System;
using System.Collections.Generic;
using SapphireEmu.Rust.GObject;
using SapphireEngine;

namespace SapphireEmu.Rust
{
    public class Item
    {
        public static Dictionary<uint, Item> ListItemsInWorld = new Dictionary<uint, Item>();
        
        public static Item CreateItem(ItemInformation info, uint amount = 1, ulong skinid = 0) => new Item(info, amount, skinid);

        public uint UID { get; }

        public float Condition { get; private set; } = 50f;
        public uint Amount { get; private set; }
        public ulong SkinID { get; private set; }
        public int PositionInContainer = -1;
        public uint HeldEnityUID => this.HeldEntity?.UID ?? 0;

        public ItemInformation Information { get; }
        
        public ItemContainer Container { get; private set; } = null;
        public BaseHeldEntity HeldEntity { get; private set; } = null;

        public E_ItemFlags ItemFlags = E_ItemFlags.None;

        public event Action<Item> OnParentChanged; 

        private Item(ItemInformation _info, uint _amount, ulong _skinid)
        {
            this.UID = BaseNetworkable.TakeUID();
            ListItemsInWorld[this.UID] = this;

            this.Information = _info;

            this.SkinID = _skinid;
            this.Condition = this.Information.MaxCondition;

            if (_amount <= 0)
                this.Amount = 1;
            else if (_amount > this.Information.MaxStack)
                this.Amount = (uint) this.Information.MaxStack;
            else
                this.Amount = _amount;

            if (this.Information.IsHoldable())
            {
                if (ItemInformation.GetHeldType(_info.HeldType, out Type type) == false)
                {
                    ConsoleSystem.LogError($"[{nameof(Item)}] Unrealized class for <{_info.Shortname}>");
                }
                this.HeldEntity = (BaseHeldEntity)Framework.Bootstraper.AddType(type);
                this.HeldEntity.Spawn(this.Information.HeldEntity.PrefabID);
                this.HeldEntity.SendNetworkUpdate();
                
                this.HeldEntity.ItemOwner = this;
                this.HeldEntity.Initialization();
            }
        }

        public void SetParent(ItemContainer container)
        {
            this.Container = container;
            this.OnParentChanged?.Invoke(this);
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
                heldEntity = HeldEnityUID,
                skinid = this.SkinID 
            };         
            return item;
        }
        #endregion
    }
}