using System;
using System.Collections.Generic;
using System.Reflection;
using Network;
using SapphireEmu.Extended;
using SapphireEngine;

namespace SapphireEmu.Environment
{
    public class ConsoleCommandAttribute : Attribute
    {
        public string Command { get; private set; }
        public bool IsAdmin { get; set; }

        public ConsoleCommandAttribute(string command)
        {
            this.Command = command;
        }
    }
    
    public partial class ConsoleNetwork
    {
        private static Dictionary<string, CommandMethod> ListCommandMethods = new Dictionary<string, CommandMethod>();
        public class CommandMethod
        {
            public Reflection.FastMethodInfo Call;
            public ConsoleCommandAttribute Attribute;

            public CommandMethod(Reflection.FastMethodInfo method, ConsoleCommandAttribute attribute)
            {
                this.Call = method;
                this.Attribute = attribute;
            }
        }
        
        public static void Load()
        {
            Type[] types = typeof(RPCNetwork).Assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = types[i].GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                for (var j = 0; j < methods.Length; j++)
                {
                    object[] args = methods[j].GetCustomAttributes(typeof(ConsoleCommandAttribute), true);
                    if (args.Length != 0)
                    {
                        ConsoleCommandAttribute attribute = args[0] as ConsoleCommandAttribute;
                        var method = new Reflection.FastMethodInfo(methods[j]);
                        
                        ListCommandMethods[attribute.Command] = new CommandMethod(method, attribute);
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
            Arg arg = Arg.FromClient(command, packet.connection);
            if (arg.Invalid)
            {
                ConsoleSystem.LogWarning($"Invalid console command from [{packet.ToPlayer().SteamID}/{packet.ToPlayer().Username}]: {command}");
                return;
            }

            if (ListCommandMethods.TryGetValue(arg.Command, out CommandMethod cmdMethod) == false)
            {
                ConsoleSystem.LogWarning($"Unknown console command from [{packet.ToPlayer().SteamID}/{packet.ToPlayer().Username}]: {arg.Command}");
                return;
            }

            if (cmdMethod.Attribute.IsAdmin && arg.IsAdmin == false)
            {
                SendClientReply(packet.connection, "You don't have permission to run this command");
                return;
            }

            cmdMethod.Call.Invoke(null, arg);

            if (string.IsNullOrEmpty(arg.Reply) == false)
            {
                SendClientReply(packet.connection, arg.Reply);
            }
        }
        
        internal static void OnServerCommand(string command)
        {
            Arg arg = Arg.FromServer(command);
            if (arg.Invalid)
            {
                ConsoleSystem.LogWarning($"Invalid console command: {command}");
                return;
            }

            if (ListCommandMethods.TryGetValue(arg.Command, out CommandMethod cmdMethod) == false)
            {
                ConsoleSystem.LogWarning($"Unknown console command: {arg.Command}");
                return;
            }

            cmdMethod.Call.Invoke(null, arg);

            if (string.IsNullOrEmpty(arg.Reply) == false)
            {
                ConsoleSystem.Log(arg.Reply);
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