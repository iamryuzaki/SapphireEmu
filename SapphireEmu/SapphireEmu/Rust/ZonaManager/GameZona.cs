using System;
using System.Collections.Generic;
using Network;
using SapphireEmu.Data;
using SapphireEmu.Environment;
using SapphireEmu.Extended;
using SapphireEmu.Rust.GObject;
using SapphireEngine;

namespace SapphireEmu.Rust.ZonaManager
{
    public class GameZona
    {
        public Int32 X { get; private set; }
        public Int32 Y { get; private set; }
        public UInt32 UID { get; private set; }

        public List<BaseNetworkable> ListNetworkablesInZona = new List<BaseNetworkable>();
        public List<BasePlayer> ListPlayersInZona = new List<BasePlayer>();
        
        public GameZona(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.UID = (uint)(x + y);
        }

        public void Registration(BaseNetworkable networkable)
        {
            if (networkable.CurentGameZona == null || networkable.CurentGameZona != this)
            {
                if (networkable.CurentGameZona != null)
                {
                    networkable.CurentGameZona.UnRegistration(networkable);
                    this.Change(networkable);
                }
                else
                    this.Teleportation(networkable);

                ListNetworkablesInZona.Add(networkable);
                if (networkable is BasePlayer player && player.IsConnected && ListPlayersInZona.Contains(player) == false)
                    ListPlayersInZona.Add(player);
                
                networkable.CurentGameZona = this;
            }
        }

        public void UnRegistration(BaseNetworkable networkable)
        {
            if (this.ListNetworkablesInZona.Contains(networkable))
            {
                this.ListNetworkablesInZona.Remove(networkable);
                if (networkable is BasePlayer player && player.IsConnected)
                    this.ListPlayersInZona.Remove(player);
            }
        }

        private void Subscribe(BaseNetworkable networkable)
        {
            if (this.ListPlayersInZona.Count != 0)
            {
                for (var i1 = 0; i1 < this.ListPlayersInZona.Count; i1++)
                {
                    if (networkable.ListViewToMe.Contains(this.ListPlayersInZona[i1]) == false && this.ListPlayersInZona[i1] != networkable)
                        networkable.ListViewToMe.Add(this.ListPlayersInZona[i1]);
                }

                networkable.SendNetworkUpdate(new SendInfo(this.ListPlayersInZona.ToConnectionsList()));
            }
                                
            if (networkable is BasePlayer player && player.IsConnected)
            {
                for (var i1 = 0; i1 < this.ListNetworkablesInZona.Count; i1++)
                {
                    if (this.ListNetworkablesInZona[i1].ListViewToMe.Contains(player) == false)
                    {
                        this.ListNetworkablesInZona[i1].ListViewToMe.Add(player);
                        this.ListNetworkablesInZona[i1].SendNetworkUpdate(new SendInfo(player.NetConnection));
                    }
                }
            }
        }

        private void UnSubscribe(BaseNetworkable networkable)
        {
            if (this.ListPlayersInZona.Count != 0)
            {
                for (var i1 = 0; i1 < this.ListPlayersInZona.Count; i1++)
                {
                    if (networkable.ListViewToMe.Contains(this.ListPlayersInZona[i1]))
                        networkable.ListViewToMe.Remove(this.ListPlayersInZona[i1]);
                }

                NetworkManager.BaseNetworkServer.write.Start();
                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                NetworkManager.BaseNetworkServer.write.EntityID(networkable.UID);
                NetworkManager.BaseNetworkServer.write.Send(new SendInfo(this.ListPlayersInZona.ToConnectionsList()));
            }

            if (networkable is BasePlayer player && player.IsConnected)
            {
                for (var i1 = 0; i1 < this.ListNetworkablesInZona.Count; i1++)
                {
                    if (this.ListNetworkablesInZona[i1].ListViewToMe.Contains(player))
                    {
                        this.ListNetworkablesInZona[i1].ListViewToMe.Remove(player);

                        NetworkManager.BaseNetworkServer.write.Start();
                        NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                        NetworkManager.BaseNetworkServer.write.EntityID(this.ListNetworkablesInZona[i1].UID);
                        NetworkManager.BaseNetworkServer.write.Send(new SendInfo(player.NetConnection));
                    }
                }
            }
        }

        private void Change(BaseNetworkable networkable)
        {
            int changeX = this.X - networkable.CurentGameZona.X;
            int changeY = this.Y - networkable.CurentGameZona.Y;

            if (changeX >= -1 && changeX <= 1 && changeY >= -1 && changeY <= 1)
            {
                if (changeX != 0)
                {
                    int unforward = changeX * -1 * Settings.MapZonesLine + networkable.CurentGameZona.X;
                    if (ZonaManager.IsInMap(unforward, networkable.CurentGameZona.Y))
                    {
                        int finishLine = networkable.CurentGameZona.Y + Settings.MapZonesLine;
                        int startLine = networkable.CurentGameZona.Y - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                            ZonaManager.GetGameZona(unforward, i)?.UnSubscribe(networkable);
                    }
                }

                if (changeY != 0)
                {
                    int unforward = changeY * -1 * Settings.MapZonesLine + networkable.CurentGameZona.Y;
                    if (ZonaManager.IsInMap(networkable.CurentGameZona.X, unforward))
                    {
                        GameZona lastView;
                        int finishLine = networkable.CurentGameZona.X + Settings.MapZonesLine;
                        int startLine = networkable.CurentGameZona.X - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                            ZonaManager.GetGameZona(i, unforward)?.UnSubscribe(networkable);
                    }
                }

                if (changeX != 0)
                {
                    int forward = changeX * Settings.MapZonesLine + this.X;
                    if (ZonaManager.IsInMap(forward, this.Y))
                    {
                        GameZona lastView;
                        int finishLine = this.Y + Settings.MapZonesLine;
                        int startLine = this.Y - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                            ZonaManager.GetGameZona(forward, i)?.Subscribe(networkable);
                    }
                }

                if (changeY != 0)
                {
                    int forward = changeY * Settings.MapZonesLine + this.Y;
                    if (ZonaManager.IsInMap(this.X, forward))
                    {
                        GameZona lastView;
                        int finishLine = this.X + Settings.MapZonesLine;
                        int startLine = this.X - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                            ZonaManager.GetGameZona(i, forward)?.Subscribe(networkable);
                    }
                }
            } else
                this.Teleportation(networkable);
        }

        private void Teleportation(BaseNetworkable networkable)
        {
            //TODO: Need rewrite tihs method... Need dont use EntityDestroy if new zona view this Entity =( By ~ TheRyuzaki
            
            if (networkable.CurentGameZona != null)
            {
                int last_finishLineX = networkable.CurentGameZona.X + Settings.MapZonesLine;
                int last_startLineX = networkable.CurentGameZona.X - Settings.MapZonesLine;
                int last_finishLineY = networkable.CurentGameZona.Y + Settings.MapZonesLine;
                int last_startLineY = networkable.CurentGameZona.Y - Settings.MapZonesLine;

                for (int x = last_startLineX; x <= last_finishLineX; x++)
                    for (int y = last_startLineY; y <= last_finishLineY; y++)
                        ZonaManager.GetGameZona(x, y)?.UnSubscribe(networkable); 
            }
            
            int finishLineX = this.X + Settings.MapZonesLine;
            int startLineX = this.X - Settings.MapZonesLine;
            int finishLineY = this.Y + Settings.MapZonesLine;
            int startLineY = this.Y - Settings.MapZonesLine;
            
            for (int x = startLineX; x <= finishLineX; x++)
                for (int y = startLineY; y <= finishLineY; y++)
                    ZonaManager.GetGameZona(x, y)?.Subscribe(networkable);
        }

        public void OnReceivingNetworkablesFromView(BasePlayer player)
        {
            int last_finishLineX = this.X + Settings.MapZonesLine;
            int last_startLineX = this.X - Settings.MapZonesLine;
            int last_finishLineY = this.Y + Settings.MapZonesLine;
            int last_startLineY = this.Y - Settings.MapZonesLine;

            GameZona gameZona;
            for (int x = last_startLineX; x <= last_finishLineX; x++)
            {
                for (int y = last_startLineY; y <= last_finishLineY; y++)
                {
                    gameZona = ZonaManager.GetGameZona(x, y);
                    if (gameZona != null)
                    {
                        for (var i = 0; i < gameZona.ListNetworkablesInZona.Count; i++)
                        {
                            if (gameZona.ListNetworkablesInZona[i].ListViewToMe.Contains(player) == false && gameZona.ListNetworkablesInZona[i] != player)
                            {
                                gameZona.ListNetworkablesInZona[i].ListViewToMe.Add(player);
                                gameZona.ListNetworkablesInZona[i].SendNetworkUpdate(new SendInfo(player.NetConnection));
                            }
                        }
                    }

                }
            }
        }

        public void OnReconnectedPlayer(BasePlayer player)
        {
            if (this.ListPlayersInZona.Contains(player) == false)
                this.ListPlayersInZona.Add(player);
        }

        public void OnDisconnectedPlayer(BasePlayer player)
        {
            int last_finishLineX = this.X + Settings.MapZonesLine;
            int last_startLineX = this.X - Settings.MapZonesLine;
            int last_finishLineY = this.Y + Settings.MapZonesLine;
            int last_startLineY = this.Y - Settings.MapZonesLine;

            GameZona lastZona;
            for (int x = last_startLineX; x <= last_finishLineX; x++)
            {
                for (int y = last_startLineY; y <= last_finishLineY; y++)
                {
                    if (ZonaManager.IsInMap(x, y))
                    {
                        lastZona = ZonaManager.GetGameZona(x, y);
                        for (var i = 0; i < lastZona.ListNetworkablesInZona.Count; i++)
                        {
                            if (lastZona.ListNetworkablesInZona[i].ListViewToMe.Contains(player))
                                lastZona.ListNetworkablesInZona[i].ListViewToMe.Remove(player);
                        }
                    }
                }
            }

            if (this.ListPlayersInZona.Contains(player))
                this.ListPlayersInZona.Remove(player);
        }
    }
}