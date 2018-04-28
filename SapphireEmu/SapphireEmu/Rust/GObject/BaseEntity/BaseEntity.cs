using SapphireEmu.Data.Base.GObject;

namespace SapphireEmu.Rust.GObject
{
    public partial class BaseEntity : BaseNetworkable
    {
        public E_EntityFlags EntityFlags = 0;
        
        public void SetFlag(E_EntityFlags f, bool b)
        {
            if (b)
            {
                if (this.HasFlag(f))
                {
                    return;
                }
                this.EntityFlags |= f;
            }
            else
            {
                if (!this.HasFlag(f))
                {
                    return;
                }
                this.EntityFlags &= ~f;
            }
        }
        
        public bool HasFlag(E_EntityFlags f)
        {
            return ((this.EntityFlags & f) == f);
        }
    }
}