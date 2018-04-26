using UnityEngine.UI;

namespace SapphireEmu.Data
{
    public class Settings
    {
        public static string ServerIP = "0.0.0.0";
        public static int GamePort = 10015;
        public static int GameVersion = 2048;
        
        public static bool Secure = true;
        public static bool NoSteam = true;
        public static uint NetworkEncryptionLevel = 0;

        public static string Hostname = "[SapphireEmu] No Name Server";
        public static int Maxplayers = 32;
        public static string MapName = "CraggyIsland";
        public static int MapSeed = 1;
        public static int MapSize = 4096;

        public static string ServerImage = "";
        public static string ServerDescription = "";
        public static string ServerURL = "";

        public const int MapZonaPlayerView = 20;
        public const int MapZonaSize = 20;
        public const int MapZonaCount = 200; // 200 * 20 = 2000x2000  map
        public const int MapZonesLine = MapZonaPlayerView / MapZonaSize;
    }
}