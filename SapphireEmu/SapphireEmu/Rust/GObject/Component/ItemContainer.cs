using System;
using System.Collections.Generic;
using SapphireEmu.Data.Base.GObject.Component;

namespace SapphireEmu.Rust.GObject.Component
{
    public class ItemContainer
    {
        public UInt32 UID = 0;

        public E_AllowedContentsType AllowedContents = 0;
        public E_ItemContainerType ItemContainerType = 0;
        public BaseEntity EntityOwner = null;
        
        public Int32 Capacity = 6;
        public List<Item> ListItems = new List<Item>();
        public Dictionary<int, Item> ListSlots = new Dictionary<int, Item>();

        public ItemContainer(BaseEntity entityOwner, int capacity = 6)
        {
            this.UID = BaseNetworkable.TakeUID();
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
            
        }

        public bool HasEmtptySlot() => this.ListSlots.Count < this.Capacity;

        public int GetFirstEmptySlot()
        {
            for (int i = 0; i < this.Capacity; i++)
                if (ListSlots.TryGetValue(i, out var item))
                    return i;
            return -1;
        }

        public bool AddItemToContainer(Item item)
        {
            if (this.HasEmtptySlot())
            {
                int slot = this.GetFirstEmptySlot();
                this.ListItems.Add(item);
                this.ListSlots[slot] = item;
                item.Container = this;
                item.Position = slot;
                return true;
            }
            return false;
        }

        public bool RemoveItemFromContainer(Item item)
        {
            if (ListSlots.TryGetValue(item.Position, out Item targetItem) && targetItem == item)
            {
                ListSlots.Remove(item.Position);
                ListItems.Remove(item);
                item.Position = -1;
                item.Container = null;
                return true;
            }
            return false;
        }


        public ProtoBuf.ItemContainer GetProtobufObject()
        {
            ProtoBuf.ItemContainer container = new ProtoBuf.ItemContainer();

            return container;
        }
    }
}