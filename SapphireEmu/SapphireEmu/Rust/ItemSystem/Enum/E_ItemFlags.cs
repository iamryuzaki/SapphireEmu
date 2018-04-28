using System;

namespace SapphireEmu.Rust
{
    [Flags]
    public enum E_ItemFlags
    {
        Cooking = 0x10,
        IsLocked = 8,
        IsOn = 2,
        None = 0,
        OnFire = 4,
        Placeholder = 1
    }
}