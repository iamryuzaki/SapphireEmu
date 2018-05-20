using System.Collections.Generic;
using ProtoBuf;

namespace SapphireEmu.Rust.GObject.Component
{
    public class BasePlayerInventory
    {
        public BasePlayer OwnerPlayer { get; private set; }
        public ItemContainer ContainerBelt { get; private set; }
        public ItemContainer ContainerWear { get; private set; }
        public ItemContainer ContainerMain { get; private set; }

        public BasePlayerInventory(BasePlayer ownerPlayer)
        {
            this.OwnerPlayer = ownerPlayer;
            this.ContainerBelt = new ItemContainer(ownerPlayer, 6);
            this.ContainerBelt.SetFlag(E_ItemContainerType.IsPlayer, true);
            this.ContainerBelt.SetFlag(E_ItemContainerType.Belt, true);
            this.ContainerWear = new ItemContainer(ownerPlayer, 6);
            this.ContainerWear.SetFlag(E_ItemContainerType.IsPlayer, true);
            this.ContainerWear.SetFlag(E_ItemContainerType.Clothing, true);
            this.ContainerMain = new ItemContainer(ownerPlayer, 24);
            this.ContainerMain.SetFlag(E_ItemContainerType.IsPlayer, true);
        }

        public List<Item> FindItemIDs(int id)
        {
            List<Item> items = new List<Item>();
            if (this.ContainerBelt != null)
            {
                items.AddRange(this.ContainerBelt.FindItemsByItemID(id));
            }
            if (this.ContainerMain != null)
            {
                items.AddRange(this.ContainerMain.FindItemsByItemID(id));
            }
            if (this.ContainerWear != null)
            {
                items.AddRange(this.ContainerWear.FindItemsByItemID(id));
            }
            return items;
        }

        public void OnInventoryUpdate()
        {
            this.ContainerBelt.OnItemConainerUpdate();
            this.ContainerMain.OnItemConainerUpdate();
            this.ContainerWear.OnItemConainerUpdate();
        }
        
        public ProtoBuf.PlayerInventory GetProtobufObject()
        {
            ProtoBuf.PlayerInventory inventory = new PlayerInventory
            {
                invBelt = this.ContainerBelt.GetProtobufObject(),
                invWear = this.ContainerWear.GetProtobufObject(),
                invMain = this.ContainerMain.GetProtobufObject(),
            };
            return inventory;
        }
    }
}