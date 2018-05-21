using Network;
using SapphireEmu.Environment;
using Message = Network.Message;

namespace SapphireEmu.Rust.GObject.Component
{
    public class BasePlayerNetwork
    {
        public BasePlayer PlayerOwner { get; private set; }
        public Connection NetConnection { get; private set; } = null;

        private PlayerTick m_lastPlayerTickPacket = new PlayerTick();
        
        
        public BasePlayerNetwork(BasePlayer playerOwner)
        {
            this.PlayerOwner = playerOwner;
        }

        #region [Method] OnConnected
        public void OnConnected(Connection _connection)
        {
            if (BasePlayer.ListOnlinePlayers.Contains(this.PlayerOwner) == false)
                BasePlayer.ListOnlinePlayers.Add(this.PlayerOwner);
            
            this.NetConnection = _connection;
            this.PlayerOwner.Username = this.NetConnection.username;
            
            this.PlayerOwner.SetPlayerFlag(E_PlayerFlags.Connected, true);
            this.PlayerOwner.SetPlayerFlag(E_PlayerFlags.IsAdmin, true);
            this.PlayerOwner.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, true);

            if (this.PlayerOwner.UID != 0)
            {
                this.OnPlayerInit();
                this.PlayerOwner.CurentGameZona.OnReceivingNetworkablesFromView(this.PlayerOwner);
                this.PlayerOwner.CurentGameZona.OnReconnectedPlayer(this.PlayerOwner); 
            }
            else
            {
                this.PlayerOwner.Spawn((uint) Data.Base.PrefabID.BasePlayer);
                this.PlayerOwner.Inventory.ContainerBelt.AddItemToContainer(ItemManager.CreateByPartialName("rock"));
                this.PlayerOwner.Inventory.ContainerBelt.AddItemToContainer(ItemManager.CreateByPartialName("torch"));
                this.PlayerOwner.Inventory.ContainerBelt.OnItemConainerUpdate();
            }

            this.PlayerOwner.ClientRPCEx(new SendInfo(this.NetConnection), null, ERPCMethodType.FinishLoading);
            this.PlayerOwner.SetPlayerFlag(E_PlayerFlags.ReceivingSnapshot, false);
            this.PlayerOwner.SendNetworkUpdate_PlayerFlags(new SendInfo(this.NetConnection));

            this.NetConnection.authLevel = 2;
        }
        #endregion

        #region [Method] OnPlayerCreated
        public void OnPlayerInit()
        {
            if (this.PlayerOwner.IsConnected)
            {
                this.PlayerOwner.SendNetworkUpdate(new SendInfo(this.NetConnection));
                this.PlayerOwner.ClientRPCEx(new SendInfo(this.NetConnection), null, ERPCMethodType.StartLoading);
            }
        }
        #endregion

        #region [Method] OnDisconnected
        public void OnDisconnected()
        {
            if (BasePlayer.ListOnlinePlayers.Contains(this.PlayerOwner))
                BasePlayer.ListOnlinePlayers.Remove(this.PlayerOwner);

            this.NetConnection = null;
            
            this.PlayerOwner.CurentGameZona.OnDisconnectedPlayer(this.PlayerOwner);
            
            this.PlayerOwner.SetPlayerFlag(E_PlayerFlags.Connected, false);
            this.PlayerOwner.SendSleepingStart();
        }
        #endregion
        
        #region [Method] OnReceivedTick
        public void OnReceivedTick(Message _message)
        {
            bool needUpdateFlags = false;
            
            PlayerTick msg = PlayerTick.Deserialize(_message.read, this.m_lastPlayerTickPacket, true);
            this.PlayerOwner.PlayerButtons = (E_PlayerButton)msg.inputState.buttons;
            if (ModelState.Equal(this.PlayerOwner.PlayerModelState, msg.modelState) == false)
            {
                this.PlayerOwner.PlayerModelState = msg.modelState;
                needUpdateFlags = true;
            }

            if (this.PlayerOwner.IsSleeping)
            {
                if (this.PlayerOwner.HasPlayerButton(E_PlayerButton.FIRE_PRIMARY) || this.PlayerOwner.HasPlayerButton(E_PlayerButton.FIRE_SECONDARY) || this.PlayerOwner.HasPlayerButton(E_PlayerButton.JUMP))
                {
                    this.PlayerOwner.SetPlayerFlag(E_PlayerFlags.Sleeping, false);
                    needUpdateFlags = true;
                }
                
            }
            else
            {
                if ((this.PlayerOwner.ActiveItem?.UID ?? 0)  != msg.activeItem)
                {
                    this.PlayerOwner.OnChangeActiveItem(Item.ListItemsInWorld.TryGetValue(msg.activeItem, out Item item) ? item : null);
                    needUpdateFlags = true;
                }
                if (this.PlayerOwner.Position != msg.position || this.PlayerOwner.Rotation != msg.inputState.aimAngles)
                {
                    this.PlayerOwner.Position = msg.position;
                    this.PlayerOwner.Rotation = msg.inputState.aimAngles;
                    this.PlayerOwner.OnPositionChanged();
                }
            }

            if (needUpdateFlags == true) 
                this.PlayerOwner.SendNetworkUpdate_PlayerFlags();
            this.m_lastPlayerTickPacket = msg.Copy();
        }
        #endregion
    }
}