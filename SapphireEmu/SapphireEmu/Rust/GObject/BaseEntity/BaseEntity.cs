using Network;
using SapphireEmu.Data.Base.GObject;
using SapphireEmu.Extended;

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

        #region [Methods] OnRPC Methods
        
        #region [Method] OnRPC_BroadcastSignalFromClient

        [Data.Base.Network.RPCMethod(Data.Base.Network.RPCMethod.ERPCMethodType.BroadcastSignalFromClient)]
        void OnRPC_BroadcastSignalFromClient(Message packet)
        {
            Signal signal = (Signal)packet.read.Int32();
            string arg = packet.read.String();
            this.SignalBroadcast(signal, arg, packet.connection);
        }
        #endregion
        
        #endregion

        #region [Methods] Send Signals
        public void SignalBroadcast(Signal signal, string arg, Connection sourceConnection = null)
        {
            SendInfo sendInfo = new SendInfo(base.ListViewToMe.ToConnectionsList())
            {
                method = SendMethod.Unreliable,
                priority = Priority.Immediate
            };
            this.ClientRPCEx<int, string>(sendInfo, sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType.SignalFromServerEx, (int)signal, arg);
        }

        public void SignalBroadcast(Signal signal, Connection sourceConnection = null)
        {
            SendInfo sendInfo = new SendInfo(base.ListViewToMe.ToConnectionsList())
            {
                method = SendMethod.Unreliable,
                priority = Priority.Immediate
            };
            this.ClientRPCEx<int>(sendInfo, sourceConnection, Data.Base.Network.RPCMethod.ERPCMethodType.SignalFromServer, (int)signal);
        }
        #endregion
    }
}