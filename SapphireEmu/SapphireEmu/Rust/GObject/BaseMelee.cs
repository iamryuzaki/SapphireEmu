using Network;
using ProtoBuf;
using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public class BaseMelee : BaseHeldEntity
    {
        #region [Method] OnRPC_OnPlayerAttack
        [RPCMethod(ERPCMethodType.PlayerAttack)]
        void OnRPC_PlayerAttack(Message packet)
        {
            BasePlayer player = packet.ToPlayer();
            using (PlayerAttack playerAttack = PlayerAttack.Deserialize(packet.read))
            {
                if (Find(playerAttack.attack.hitID, out BaseEntity hitEntity))
                {
                    if (hitEntity is BaseCombatEntity hitCombatEntity)
                    {
                        Item ActiveItem = player.ActiveItem;
                        if (ActiveItem.Information.IsHoldable())
                        {
                            hitCombatEntity.Hurt(ActiveItem.Information.BaseMelee.Damage);
                        } else ConsoleSystem.LogError($"[{nameof(OnRPC_PlayerAttack)}][NOT HELD ENTITY] Trying hit from <{ActiveItem.Information.ItemID}>");
                    }
                }
            }
        }
        #endregion
    }
}