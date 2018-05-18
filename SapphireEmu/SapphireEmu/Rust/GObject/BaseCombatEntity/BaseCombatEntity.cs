using System;
using ProtoBuf;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseCombatEntity : BaseEntity
    {
        public Boolean IsAlive => Math.Abs(this.Health) > 0.01;
        
        public float Health = 100f;
        public E_LifeState LifeState => IsAlive ? E_LifeState.Alive : E_LifeState.Dead;

        public virtual void Hurt(float damage, E_DamageType type = E_DamageType.Generic, BaseCombatEntity initiator = null)
        {
            this.Health = damage > this.Health ? 0 : this.Health - damage;
            this.SendNetworkUpdate();
        }

        public override void GetEntityProtobuf(Entity entity)
        {
            base.GetEntityProtobuf(entity);

            entity.baseCombat = new BaseCombat
            {
                health = this.Health,
                state = (int) this.LifeState,
            };
        }
    }
}