using ProtoBuf;
using SapphireEmu.Data.Base.GObject.Component;

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