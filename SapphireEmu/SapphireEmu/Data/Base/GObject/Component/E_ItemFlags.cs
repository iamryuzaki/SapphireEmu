using System;

namespace SapphireEmu.Data.Base.GObject.Component
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