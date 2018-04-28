using System;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseCombatEntity : BaseEntity
    {
        public Boolean IsAlive => this.Health != 0;
        
        public float Health = 100f;
        public E_LifeState LifeState => this.Health != 0 ? E_LifeState.Alive : E_LifeState.Dead;

        public virtual void Hurt(float damage, E_DamageType type = E_DamageType.Generic, BaseCombatEntity initiator = null)
        {
            this.Health = damage > this.Health ? 0 : this.Health - damage;
            if (this is BasePlayer player)
                ConsoleSystem.Log($"'{player.Username}' hurt <{damage}>, new hp {this.Health}");
            this.SendNetworkUpdate();
        }
    }
}