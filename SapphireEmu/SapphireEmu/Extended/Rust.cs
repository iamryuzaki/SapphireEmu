using System.Collections.Generic;
using System.Resources;
using Network;
using SapphireEmu.Rust.GObject;

namespace SapphireEmu.Extended
{
    public static class Rust
    {
        public static global::Network.Connection[] ToConnectionsList(this List<BasePlayer> listPlayers)
        {
            Connection[] listConnections = new Connection[listPlayers.Count];

            for (var i = 0; i < listPlayers.Count; i++)
                listConnections[i] = listPlayers[i].NetConnection;
            
            return listConnections;
        }

        public static BasePlayer ToPlayer(this Message message)
        {
            if (message.connection != null && BasePlayer.ListPlayers.TryGetValue(message.connection.userid, out var player))
                return player;
            return null;
        }

        public static BasePlayer ToPlayer(this Connection connection)
        {
            if (connection != null && BasePlayer.ListPlayers.TryGetValue(connection.userid, out var player))
                return player;
            return null;
        }
    }
}    