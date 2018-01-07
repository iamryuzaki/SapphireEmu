using System;
using System.IO;
using Network;
using SapphireEngine;

namespace SapphireEmu.Struct.Network
{
    public struct UserGiveInformation
    {
        public Byte PacketProtocol;
        public UInt64 SteamID;
        public UInt32 ConnectionProtocol;
        public String OS;
        public String Username;
        public String Branch;
        public Byte[] SteamToken;
        public Byte[] PacketBuffer;

        public static UserGiveInformation ParsePacket(Message _message)
        {
            try
            {
                _message.read.Position = 1;
                var userInformation = new UserGiveInformation();
                userInformation.PacketProtocol = _message.read.UInt8();
                userInformation.SteamID = _message.read.UInt64();
                userInformation.ConnectionProtocol = _message.read.UInt32();
                userInformation.OS = _message.read.String();
                userInformation.Username = _message.read.String();
                userInformation.Branch = _message.read.String();
                userInformation.SteamToken = _message.read.BytesWithSize();
                _message.peer.read.Position = 0L;
                using (BinaryReader br = new BinaryReader(_message.peer.read))
                    userInformation.PacketBuffer = br.ReadBytes((int) _message.peer.read.Length);
                return userInformation;
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("Error to Struct.Network.UserGiveInformation.ParsePacket(): " + ex.Message);
            }
            return default(UserGiveInformation);
        }
    }
}