using System;
using System.Collections.Generic;
using System.Reflection;
using Network;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Environment
{
    public class ConsoleCommand : Attribute
    {
        public string Command { get; private set; }

        public ConsoleCommand(string command)
        {
            this.Command = command;
        }
    }
    
    public partial class ConsoleNetwork
    {
        private static Dictionary<string, Reflection.FastMethodInfo> ListCommandMethods = new Dictionary<string, Reflection.FastMethodInfo>();
        
        public static void Load()
        {
            Type[] types = typeof(RPCNetwork).Assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = types[i].GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                for (var j = 0; j < methods.Length; j++)
                {
                    object[] args = methods[j].GetCustomAttributes(typeof(ConsoleCommand), true);
                    if (args.Length != 0)
                    {
                        ConsoleCommand commandMethod = args[0] as ConsoleCommand;
                        ListCommandMethods[commandMethod.Command] = new Reflection.FastMethodInfo(methods[j]);
                    }
                }
            }
        }
        
        internal static void OnClientCommand(Message packet)
        {
            string command = packet.read.String();
            if (packet.connection == null || !packet.connection.connected)
            {
                ConsoleSystem.LogWarning(string.Concat("Client without connection tried to run command: ", command));
                return;
            }
            Arg arg = new Arg(command, packet.connection);
            if (arg.Invalid)
            {
                ConsoleSystem.LogWarning($"Invalid console command from [{packet.ToPlayer().SteamID}/{packet.ToPlayer().Username}]: {command}");
                return;
            }

            if (ListCommandMethods.TryGetValue(arg.Command, out Reflection.FastMethodInfo method) == false)
            {
                ConsoleSystem.LogWarning($"Unknown console command from [{packet.ToPlayer().SteamID}/{packet.ToPlayer().Username}]: {arg.Command}");
                return;
            }

            method.Invoke(null, arg);

            if (string.IsNullOrEmpty(arg.Reply) == false)
            {
                SendClientReply(packet.connection, arg.Reply);
            }
        }
        
        internal static void SendClientReply(Connection cn, string strCommand)
        {
            if (!NetworkManager.BaseNetworkServer.IsConnected())
            {
                return;
            }
            NetworkManager.BaseNetworkServer.write.Start();
            NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.ConsoleMessage);
            NetworkManager.BaseNetworkServer.write.String(strCommand);
            NetworkManager.BaseNetworkServer.write.Send(new SendInfo(cn));
        }
    }
}