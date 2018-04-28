using System.Collections.Generic;
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
            if (Find(playerAttack.attack.hitID, out BaseEntity hitEntity))
            {
                if (hitEntity is BaseCombatEntity hitCombatEntity)
                {
                    if (firedProjectiles.TryGetValue(playerAttack.projectileID, out ItemInformation info))
                    {
                        hitCombatEntity.Hurt(info.BaseProjectile.Damage);
                    }
                }
            }
        }
        #endregion
    }
}