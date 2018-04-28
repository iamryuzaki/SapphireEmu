using System;

namespace SapphireEmu.Environment
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCMethodAttribute : Attribute
    {
        public ERPCMethodType Type { get; private set; }

        public RPCMethodAttribute(ERPCMethodType type)
        {
            this.Type = type;
        }
    }
}