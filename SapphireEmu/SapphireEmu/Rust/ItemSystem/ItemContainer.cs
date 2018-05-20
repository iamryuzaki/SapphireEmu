using System;
using System.Collections.Generic;
using Network;
using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEmu.Rust.GObject;

namespace SapphireEmu.Rust
{
    public class ItemContainer
    {
        public static Dictionary<UInt32, ItemContainer> ListContainers { get; } = new Dictionary<uint, ItemContainer>();

        public UInt32 UID = 0;

        public E_AllowedContentsType AllowedContents = 0;
        public E_ItemContainerType ItemContainerType = 0;
        public BaseEntity EntityOwner { get; }

        public Int32 Capacity = 6;
        public List<Item> ListItems { get; } = new List<Item>();
        public Dictionary<int, Item> ListSlots { get; } = new Dictionary<int, Item>();

        private List<int> availableSlots = new List<int>();

        public ItemContainer(BaseEntity entityOwner, int capacity = 6)
        {
            this.UID = BaseNetworkable.TakeUID();
            ListContainers[this.UID] = this;
            this.Capacity = capacity;
            this.EntityOwner = entityOwner;
        }

        #region [Method] Get and Set ItemContainer Flags
        public bool HasFlag(E_ItemContainerType _f)=> ((this.ItemContainerType & _f) == _f);

        public void SetFlag(E_ItemContainerType _f, bool _b)
        {
            if (_b)
            {
                if (!this.HasFlag(_f))
                    this.ItemContainerType |= _f;
            }
            else
            {
                if (this.HasFlag(_f))
                    this.ItemContainerType &= ~_f;
            }
        }
        #endregion

        public void OnItemConainerUpdate()
        {
            if (this.EntityOwner is BasePlayer playerOwner)
            {
                
                ProtoBuf.UpdateItemContainer container = new ProtoBuf.UpdateItemContainer
                {
                    container = new List<ProtoBuf.ItemContainer> {this.GetProtobufObject()},
                    type = (this.HasFlag(E_ItemContainerType.Belt) ? 1 : (this.HasFlag(E_ItemContainerType.Clothing) ? 2 : 0))
                };
                if (playerOwner.IsConnected)
                    playerOwner.ClientRPCEx<ProtoBuf.UpdateItemContainer>(new SendInfo(playerOwner.Network.NetConnection),null, ERPCMethodType.UpdatedItemContainer, container);
                if (this.HasFlag(E_ItemContainerType.Clothing) || this.HasFlag(E_ItemContainerType.Belt))
                    playerOwner.ClientRPCEx<ProtoBuf.UpdateItemContainer>(new SendInfo(playerOwner.ListViewToMe.ToConnectionsList()),null, ERPCMethodType.UpdatedItemContainer, container);
            }
        }

        public bool HasEmtptySlot() => this.ListSlots.Count < this.Capacity;

        public int GetFirstEmptySlot()
        {
            if (this.HasEmtptySlot())
                for (int i = 0; i < this.Capacity; i++)
                    if (ListSlots.TryGetValue(i, out _) == false)
                        return i;
            return -1;
        }

        public bool AddItemToContainer(Item item, int slot = -1)
        {
            if (this.HasEmtptySlot())
            {
                if (slot == -1 || this.ListSlots.TryGetValue(slot, out _))
                    slot = this.GetFirstEmptySlot();
                
                this.ListItems.Add(item);
                this.ListSlots[slot] = item;
                item.SetParent(this);
                item.PositionInContainer = slot;
                item.HeldEntity?.SetHeld(false);
                item.HeldEntity?.SendNetworkUpdate();

                return true;
            }
            return false;
        }

        public bool RemoveItemFromContainer(Item item)
        {
            if (ListSlots.TryGetValue(item.PositionInContainer, out Item targetItem) && targetItem == item)
            {
                ListSlots.Remove(item.PositionInContainer);
                ListItems.Remove(item);
                item.PositionInContainer = -1;
                item.SetParent(null);
                return true;
            }
            return false;
        }

        public bool ChangeItemSlotFromContainer(Item item, int newSlot)
        {
            if (ListSlots.TryGetValue(item.PositionInContainer, out Item targetItem) && targetItem == item)
            {
                if (targetItem.PositionInContainer != newSlot)
                {
                    ListSlots.Remove(item.PositionInContainer);
                    if (this.ListSlots.TryGetValue(newSlot, out Item lastItem))
                    {
                        lastItem.PositionInContainer = item.PositionInContainer;
                        ListSlots[item.PositionInContainer] = lastItem;
                    }

                    ListSlots[newSlot] = item;
                    item.PositionInContainer = newSlot;
                    return true;
                }
            }

            return false;
        }

        public List<Item> FindItemsByItemID(int itemid)
        {
            return this.ListItems.FindAll((Item x) => x.Information.ItemID == itemid);
        }

        public ProtoBuf.ItemContainer GetProtobufObject()
        {
            List<ProtoBuf.Item> listItems = new List<ProtoBuf.Item>();
            for (var i = 0; i < ListItems.Count; i++)
                listItems.Add(ListItems[i].GetProtobufObject());
            
            ProtoBuf.ItemContainer container = new ProtoBuf.ItemContainer
            {
                allowedContents = 1,
                allowedItem = 0,
                availableSlots = availableSlots,
                contents = listItems,
                flags = (int)this.ItemContainerType,
                maxStackSize = 0,
                slots = this.Capacity,
                temperature = 30f,
                UID = this.UID
            };

            return container;
        }
    }
}