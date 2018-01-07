using System;
using System.Collections.Generic;
using SapphireEmu.Data;
using SapphireEngine;
using UnityEngine;

namespace SapphireEmu.Rust.ZonaManager
{
    public class ZonaManager
    {
        public static Dictionary<Int32, Dictionary<Int32, GameZona>> ListGameZones = new Dictionary<int, Dictionary<int, GameZona>>();
        public static int MapXMin = Settings.MapZonaCount / 2 * -1;
        public static int MapYMin = Settings.MapZonaCount / 2 * -1;
        public static int MapXMax = Settings.MapZonaCount / 2;
        public static int MapYMax = Settings.MapZonaCount / 2;

        public static void Initialization()
        {
            for (int x = MapXMin; x <= MapXMax; ++x)
            {
                ListGameZones[x] = new Dictionary<int, GameZona>();
                for (int y = MapYMin; y <= MapYMax; ++y)
                    ListGameZones[x][y] = new GameZona(x, y);
            }
            ConsoleSystem.Log("[ZonaManager]: Initialized zones " + Settings.MapZonaCount * Settings.MapZonaCount + " count from " + Settings.MapZonaSize * Settings.MapZonaCount + " size.");
        }

        public static bool IsInMap(int x, int y) => x >= MapXMin && x <= MapXMax && y >= MapYMin && y <= MapYMax;

        public static bool IsInMap(Vector3 position)
        {
            int positionX = (int)position.x * 2 / Settings.MapZonaSize / 2;
            int positionY = (int)position.z * 2 / Settings.MapZonaSize / 2;

            return IsInMap(positionX, positionY);
        }
        
        public static GameZona GetGameZona(int x, int y)
        {
            GameZona result = null;
            if (ListGameZones.TryGetValue(x, out _))
                ListGameZones[x].TryGetValue(y, out result);
            return result;
        }

        public static GameZona GetGameZona(Vector3 position)
        {
            int positionX = (int)position.x * 2 / Settings.MapZonaSize / 2;
            int positionY = (int)position.z * 2 / Settings.MapZonaSize / 2;

            return GetGameZona(positionX, positionY);
        }

        public static Vector2 GetGameZonaPointFromPosition(Vector3 position)
        {
            int positionX = (int)position.x * 2 / Settings.MapZonaSize / 2;
            int positionY = (int)position.z * 2 / Settings.MapZonaSize / 2;
            return new Vector2(positionX, positionY);
        }
    }
}