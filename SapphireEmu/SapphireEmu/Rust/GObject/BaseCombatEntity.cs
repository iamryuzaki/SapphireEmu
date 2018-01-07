using System;
using SapphireEmu.Data.Base.GObject;

namespace SapphireEmu.Rust.GObject
{
    public class BaseCombatEntity : BaseEntity
    {
        public Boolean IsAlive => this.Health != 0;
        
        public float Health = 100f;
        public E_LifeState LifeState => this.Health != 0 ? E_LifeState.Alive : E_LifeState.Dead;
    }
}