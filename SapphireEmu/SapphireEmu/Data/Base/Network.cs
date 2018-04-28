using System;
using System.Collections.Generic;
using System.Reflection;
using SapphireEmu.Extended;
using SapphireEmu.Rust.GObject;
using SapphireEngine;

namespace SapphireEmu.Data.Base
{
    public class Network
    {
        private static Dictionary<RPCMethod.ERPCMethodType, Reflection.FastMethodInfo> ListRPCMethods = new Dictionary<RPCMethod.ERPCMethodType, Reflection.FastMethodInfo>();

        #region [Class] [Attribute] RPCMethod
        [AttributeUsage(AttributeTargets.Method)]
        public class RPCMethod : Attribute
        {
            public ERPCMethodType Type;
            
            public RPCMethod(ERPCMethodType type)
            {
                this.Type = type;
            }
            
            #region [Enum] ERPCMethodType
            public enum ERPCMethodType : UInt32
            {
                OnPlayerLanded = 2248815946, // падение игрока
                FinishLoading = 1052678473,
                StartLoading = 2832517786,
                OnModelState = 3470646823,
                UpdateMetabolism = 2310938162,
                BroadcastSignalFromClient = 555001694,
                PlayerAttack = 148642921, // конец атаки инструментами
                CLProject = 386279056, // начало атаки оружием
                OnProjectileAttack = 3322107216, // конец атаки оружием
                UpdateLoot = 3999757041, // с сервера: при перемещении в инвентаре который лутается (открытив в т.ч.)
                MoveItem = 4191184484, // с клиента: при перемещении
                AddUI = 92660469,
                DestroyUI = 1986762766,
                UpdatedItemContainer = 241499635,
                ForcePositionTo = 4247659151,
                Pickup = 3306490492, // поднятие ресурсов (гриб...)
                ItemCmd = 2116208967, // съедание
                UseSelf = 4147870035, // лечение
                KeepAlive = 1739731598, // начало поднятия
                Assist = 540658179, // само поднятия
                StartReload = 3302290555, //начало перезарядки
                Reload = 3360326041, //конец перезарядки
                OnDied = 3282506307,
                ClientKeepConnectionAlive = 2177997023,
                SignalFromServerEx = 3008034696,
                SignalFromServer = 4248935445,
                ClientLoadingComplete = 618836810,
                DirectionalDamage = 4047348697,
                CL_Punch = 632479192,
            }
            #endregion
        }
        #endregion
        
        public static void Load()
        {
            Type[] types = typeof(Network).Assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = types[i].GetMethods(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (var j = 0; j < methods.Length; j++)
                {
                    object[] args = methods[j].GetCustomAttributes(typeof(RPCMethod), true);
                    if (args.Length != 0)
                    {
                        RPCMethod rpcMethod = args[0] as RPCMethod;
                        ListRPCMethods[rpcMethod.Type] = new Reflection.FastMethodInfo(methods[j]);
                    }
                }
            }
        }

        public static void Run(uint uid, RPCMethod.ERPCMethodType method, global::Network.Message message)
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