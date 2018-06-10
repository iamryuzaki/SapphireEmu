using Facepunch;
using Network;
using ProtoBuf;
using SapphireEmu.Environment;
using SapphireEngine;

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
            ProjectileShoot projectileShoot = ProjectileShoot.Deserialize(packet.read);
            if (projectileShoot.projectiles.Count > ItemOwner.Information.BaseProjectile.ProjectilesPerShot)
            {
                ConsoleSystem.Log($"projectiles.Count[{projectileShoot.projectiles.Count}] > ProjectilesPerShot[{ItemOwner.Information.BaseProjectile.ProjectilesPerShot}]");
                Release();
                return;
            }

            if (AmmoCount <= 0)
            {
                ConsoleSystem.Log($"AmmoCount[{AmmoCount}] <= 0");
                Release();
                return;
            }

            AmmoCount--;
            
            foreach (var projectile in projectileShoot.projectiles)
            {
                PlayerOwner.firedProjectiles.Add(projectile.projectileID, ItemOwner.Information);
            }
            base.SignalBroadcast(E_Signal.Attack, string.Empty, packet.connection);
            
            Release();

            void Release()
            {
                projectileShoot.Dispose();
                Pool.Free(ref projectileShoot);
            }
        }
        #endregion

        #region OnRPC_Reload
        [RPCMethod(ERPCMethodType.Reload)]
        void OnRPC_Reload(Message packet)
        {
            if (AmmoCount == AmmoMax) return;
            var items = PlayerOwner.Inventory.FindItemIDs(AmmoType);
            
            foreach (Item item in items)
            {
                int num = this.AmmoMax - this.AmmoCount;
                for (int i = 0; i < num && item.Amount > 0; i++)
                {
                    this.AmmoCount++;
                    item.Use(1);
                }
            }
            PlayerOwner.Inventory.OnInventoryUpdate();
            this.SendNetworkUpdate();
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