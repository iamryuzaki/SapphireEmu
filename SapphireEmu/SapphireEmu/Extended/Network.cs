using System;
using Network;
using SapphireEngine;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace SapphireEmu.Extended
{
    public static class Network
    {
        public static class GenericsUtil
        {
            // Methods
            public static TDst Cast<TSrc, TDst>(TSrc obj)
            {
                CastImpl<TSrc, TDst>.Value = obj;
                return CastImpl<TDst, TSrc>.Value;
            }

            public static void Swap<T>(ref T a, ref T b)
            {
                T local = a;
                a = b;
                b = local;
            }

            // Nested Types
            private static class CastImpl<TSrc, TDst>
            {
                // Fields
                [ThreadStatic] public static TSrc Value;

                // Methods
                static CastImpl()
                {
                    if (typeof(TSrc) != typeof(TDst))
                    {
                        throw new InvalidCastException();
                    }
                }
            }
        }

        public static void WriteObject<T>(this Write write, T obj)
        {
            if (typeof(T) == typeof(Vector3))
            {
                write.Vector3(GenericsUtil.Cast<T, Vector3>(obj));
            }
            else if (typeof(T) == typeof(Ray))
            {
                write.Ray(GenericsUtil.Cast<T, Ray>(obj));
            }
            else if (typeof(T) == typeof(float))
            {
                write.Float(GenericsUtil.Cast<T, float>(obj));
            }
            else if (typeof(T) == typeof(short))
            {
                write.Int16(GenericsUtil.Cast<T, short>(obj));
            }
            else if (typeof(T) == typeof(ushort))
            {
                write.UInt16(GenericsUtil.Cast<T, ushort>(obj));
            }
            else if (typeof(T) == typeof(int))
            {
                write.Int32(GenericsUtil.Cast<T, int>(obj));
            }
            else if (typeof(T) == typeof(uint))
            {
                write.UInt32(GenericsUtil.Cast<T, uint>(obj));
            }
            else if (typeof(T) == typeof(byte[]))
            {
                write.Bytes(GenericsUtil.Cast<T, byte[]>(obj));
            }
            else if (typeof(T) == typeof(long))
            {
                write.Int64(GenericsUtil.Cast<T, long>(obj));
            }
            else if (typeof(T) == typeof(ulong))
            {
                write.UInt64(GenericsUtil.Cast<T, ulong>(obj));
            }
            else if (typeof(T) == typeof(string))
            {
                write.String(GenericsUtil.Cast<T, string>(obj));
            }
            else if (typeof(T) == typeof(sbyte))
            {
                write.Int8(GenericsUtil.Cast<T, sbyte>(obj));
            }
            else if (typeof(T) == typeof(byte))
            {
                write.UInt8(GenericsUtil.Cast<T, byte>(obj));
            }
            else if (typeof(T) == typeof(bool))
            {
                write.Bool(GenericsUtil.Cast<T, bool>(obj));
            }
            else if (obj is IProto)
            {
                ((IProto) obj).WriteToStream(write);
            }
            else
            {
                ConsoleSystem.LogError(string.Concat(new object[] {"NetworkData.Write - no handler to write ", obj, " -> ", obj.GetType()}));
            }
        }
    }
}