using Network;
using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Rust.GObject
{
    public partial class BaseEntity
    {
        #region [Method] OnRPC_BroadcastSignalFromClient
        [RPCMethod(ERPCMethodType.BroadcastSignalFromClient)]
        void OnRPC_BroadcastSignalFromClient(Message packet)
        {
            E_Signal eSignal = (E_Signal)packet.read.Int32();
            string arg = packet.read.String();
            this.SignalBroadcast(eSignal, arg, packet.connection);
        }
        #endregion
        
        #region [Methods] Send Signals
        public void SignalBroadcast(E_Signal eSignal, string arg, Connection sourceConnection = null)
        {
            SendInfo sendInfo = new SendInfo(this.ListViewToMe.ToConnectionsList());
            this.ClientRPCEx<int, string>(sendInfo, null, ERPCMethodType.SignalFromServerEx, (int)eSignal, arg);
        }

        public void SignalBroadcast(E_Signal eSignal, Connection sourceConnection = null)
        {
            SendInfo sendInfo = new SendInfo(this.ListViewToMe.ToConnectionsList());
            this.ClientRPCEx<int>(sendInfo, null, ERPCMethodType.SignalFromServer, (int)eSignal);
        }
        #endregion
    }
}