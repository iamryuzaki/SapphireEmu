using System;
using System.Collections.Generic;
using System.Reflection;
using Network;
using SapphireEmu.Extended;
using SapphireEmu.Rust.GObject;
using SapphireEngine;

namespace SapphireEmu.Environment
{
    public class RPCNetwork
    {
        private static Dictionary<ERPCMethodType, Reflection.FastMethodInfo> ListRPCMethods = new Dictionary<ERPCMethodType, Reflection.FastMethodInfo>();
        
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
                        ListRPCMethods[rpcMethod.Type] = new Reflection.FastMethodInfo(methods[j]);
                    }
                }
            }
        }

        public static void Run(uint uid, ERPCMethodType method, Message message)
        {
            if (ListRPCMethods.TryGetValue(method, out var methodInfo))
            {
                if (BaseNetworkable.ListNetworkables.TryGetValue(uid, out var networkable))
                {
                    try
                    {
                        methodInfo.Invoke(networkable, message);
                    }
                    catch (Exception ex)
                    {
                        ConsoleSystem.LogError("[Data.Base.Network Run] Exception: " + ex.Message);
                    }
                } else
                    ConsoleSystem.LogWarning("[Data.Base.Network Run] Dont have networkable uid: " + uid);
            } else 
                ConsoleSystem.LogWarning("[Data.Base.Network Run] Dont have released method: " + method + " from " + BaseNetworkable.ListNetworkables?[uid]);
        }
    }
}