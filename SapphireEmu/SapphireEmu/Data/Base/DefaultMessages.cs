using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SapphireEngine;

namespace SapphireEmu.Data.Base
{
    public class DefaultMessages
    {
        public static readonly string PathFileBase = BuildingInformation.DirectoryBase + "Message.dat";

        public static string Network_Connection_BadVersion_Client { get; private set; }
        public static string Network_Connection_BadVersion_Server { get; private set; }
        public static string Network_Connection_NewConnection { get; private set; }
        public static string Network_Connection_NewConnection_Authed { get; private set; }
        public static string Network_Connection_NewDisconnected { get; private set; }

        #region [Method] [Static] Load
        public static void Load()
        {
            if (File.Exists(PathFileBase))
            {
                Dictionary<string, string> lines_from_file = new Dictionary<string, string>();
                var lines = File.ReadAllLines(PathFileBase);
                for (int i = 0; i < lines.Length; ++i)
                {
                    if (lines[i].Length >= 2)
                    {
                        int indexSpliter = lines[i].IndexOf('=');
                        if (indexSpliter > 0)
                        {
                            string key = lines[i].Substring(0, indexSpliter).ToLower();
                            key = key.Trim();

                            string value = string.Empty;
                            if (indexSpliter + 1 < lines[i].Length)
                            {
                                value = lines[i].Remove(0, indexSpliter + 1);
                                value = value.Trim();
                            }
                            lines_from_file[key] = value;
                        }
                    }
                }
                var props = typeof(DefaultMessages).GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                for (int i = 0; i < props.Length; ++i)
                {
                    var prop_name = props[i].Name.ToLower();
                    if (lines_from_file.ContainsKey(prop_name))
                    {
                        props[i].SetValue(null, lines_from_file[prop_name], new object[0]);
                    }
                }
                ConsoleSystem.Log("[Data.Base.Message] Loaded!");
            }
            else
            {
                ConsoleSystem.LogWarning("[Data.Base.Message] Not found file: " + PathFileBase);
            }
        }
        #endregion
    }
}