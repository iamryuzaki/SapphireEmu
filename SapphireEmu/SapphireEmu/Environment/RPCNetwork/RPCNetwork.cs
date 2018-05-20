using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Network;
using SapphireEmu.Extended;
using SapphireEmu.Rust.GObject;
using SapphireEngine;
using SapphireEngine.Functions;

namespace SapphireEmu.Environment
{
    public class RPCNetwork
    {
        private static Dictionary<ERPCMethodType, Dictionary<Type, Reflection.FastMethodInfo>> ListRPCMethods = new Dictionary<ERPCMethodType, Dictionary<Type, Reflection.FastMethodInfo>>();
        
        public static void Load()
        {
            Type[] types = typeof(RPCNetwork).Assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = types[i].GetMethods(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (var j = 0; j < methods.Length; j++)
                {
                    object[] args = methods[j].GetCustomAttributes(typeof(RPCMethodAttribute), true);
                    if (args.Length != 0)
                    {
                        RPCMethodAttribute rpcMethod = args[0] as RPCMethodAttribute;
                        if (!ListRPCMethods.ContainsKey(rpcMethod.Type))
                            ListRPCMethods[rpcMethod.Type] = new Dictionary<Type, Reflection.FastMethodInfo>();
                        ListRPCMethods[rpcMethod.Type].Add(types[i],new Reflection.FastMethodInfo(methods[j]));
                    }
                }
            }
        }

        public static void Run(uint uid, ERPCMethodType method, Message message)
        {
            if (ListRPCMethods.TryGetValue(method, out var methods))
            {
                if (BaseNetworkable.ListNetworkables.TryGetValue(uid, out var networkable))
                {
                    var type = networkable.GetType();
                    var rpcmethod = methods.FirstOrDefault(p => p.Key == type || p.Key.IsSubclassOf(type)).Value;
                    if (rpcmethod != null)
                    {
                        try
                        {
                            using (new TimeDebugger($"{method}", 0.001f))
                            {
                                rpcmethod.Invoke(networkable, message);
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleSystem.LogError("[Data.Base.Network Run] Exception: " + ex.Message + System.Environment.NewLine+ex.StackTrace);
                        }
                    }
                } else
                    ConsoleSystem.LogWarning("[Data.Base.Network Run] Dont have networkable uid: " + uid);
            } else 
                ConsoleSystem.LogWarning("[Data.Base.Network Run] Dont have released method: " + method + " from " + BaseNetworkable.ListNetworkables?[uid]);
        }
    }
}