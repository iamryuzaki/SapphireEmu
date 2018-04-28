using System;

namespace SapphireEmu.Rust
{
    [Flags]
    public enum E_ItemContainerType
    {
        Belt = 4,
        Clothing = 2,
        IsLocked = 0x10,
        IsPlayer = 1,
        NoBrokenItems = 0x40,
        NoItemInput = 0x80,
        ShowSlotsOnIcon = 0x20,
        SingleType = 8
    }
}