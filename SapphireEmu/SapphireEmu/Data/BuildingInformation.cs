using System;

namespace SapphireEmu.Data
{
    public class BuildingInformation
    {
        public const string ApplicationName = "SapphireEmu";
        public const string ApplicationVersion = "0.0.2";
        public const int ApplicationBuild = 101;

        public const string Author = "TheRyuzaki";
        public const string Thanks = "";

        public const uint SteamworksAppID = 252480;
        public const string SteamworksModDir = "rust";
        public const string SteamworksGameDesc = "Rust";

        public static readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string DirectoryLogs = DirectoryRoot + "Logs/";
        public static readonly string DirectoryData = DirectoryRoot + "Data/";
        public static readonly string DirectoryBase = DirectoryData + "Base/";
        public static readonly string DirectoryBin = DirectoryData + "Bin/";
        
    }
}