using System;

namespace SapphireEmu.Data.Base.GObject
{
    [Flags]
    public enum E_EntityFlags
    {
        Placeholder = 1,
        On = 2,
        OnFire = 4,
        Open = 8,
        Locked = 16,
        Debugging = 32,
        Disabled = 64,
        Reserved1 = 128,
        Reserved2 = 256,
        Reserved3 = 512,
        Reserved4 = 1024,
        Reserved5 = 2048,
        Broken = 4096,
        Busy = 8192,
        Reserved6 = 16384,
        Reserved7 = 32768,
        Reserved8 = 65536
    }
}