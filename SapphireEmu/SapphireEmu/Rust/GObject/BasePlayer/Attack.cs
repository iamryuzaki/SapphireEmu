using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using SapphireEmu.Environment;

namespace SapphireEmu.Rust.GObject
{
    public partial class BasePlayer
    {
        public Dictionary<int, ItemInformation> firedProjectiles = new Dictionary<int, ItemInformation>();

        #region [Method] OnRPC_OnProjectileAttack
        [RPCMethod(ERPCMethodType.OnProjectileAttack)]
        void OnRPC_OnProjectileAttack(Message packet)
        {
            PlayerProjectileAttack ppAttack = PlayerProjectileAttack.Deserialize(packet.read);
            PlayerAttack playerAttack = ppAttack.playerAttack;

            if (firedProjectiles.TryGetValue(playerAttack.projectileID, out ItemInformation info))
            {
                firedProjectiles.Remove(playerAttack.projectileID);
                
                if (Find(playerAttack.attack.hitID, out BaseEntity hitEntity))
                {
                    if (hitEntity is BaseCombatEntity hitCombatEntity)
                    {
                        hitCombatEntity.Hurt(info.BaseProjectile?.Damage ?? info.BaseMelee.Damage);
                    }
                }
            }

            Release();
            
            void Release()
            {
                Pool.Free(ref ppAttack);
            }
        }
        #endregion
    }
}