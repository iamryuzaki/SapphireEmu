using Network;
using ProtoBuf;
using SapphireEmu.Environment;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseProjectile : BaseHeldEntity
    {
        public int AmmoCount = 10;
        public int AmmoMax = 30;
        public int AmmoType = 815896488;


        #region OnRPC_CLProject
        [RPCMethod(ERPCMethodType.CLProject)]
        void OnRPC_ClProject(Message packet)
        {
            using (ProjectileShoot projectileShoot = ProjectileShoot.Deserialize(packet.read))
            {
                foreach (var projectile in projectileShoot.projectiles)
                {
                    PlayerOwner.firedProjectiles.Add(projectile.projectileID, ItemOwner.Information);
                }
            }
        }
        #endregion
        
        public override void GetEntityProtobuf(Entity entity)
        {
            base.GetEntityProtobuf(entity);
            entity.baseProjectile = new ProtoBuf.BaseProjectile()
            {
                primaryMagazine = new Magazine()
                {
                    ammoType = this.AmmoType,
                    capacity = this.AmmoMax,
                    contents = this.AmmoCount
                }
            };
        }
    }
}