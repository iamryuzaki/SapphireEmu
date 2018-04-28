using System;

namespace SapphireEmu.Data.Base.GObject
{
    [Flags]
    public enum E_EntityFlags
    {
        Broken = 0x1000,
        Busy = 0x2000,
        Debugging = 0x20,
        Disabled = 0x40,
        Locked = 0x10,
        On = 2,
        OnFire = 4,
        Open = 8,
        Placeholder = 1,
        Reserved1 = 0x80,
        Reserved2 = 0x100,
        Reserved3 = 0x200,
        Reserved4 = 0x400,
        Reserved5 = 0x800,
        Reserved6 = 0x4000,
        Reserved7 = 0x8000,
        Reserved8 = 0x10000
    }
}