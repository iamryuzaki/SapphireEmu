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
                ConsoleSystem.Log($"GameZona_{X}_{Y}: Registration: Networkables count - " + this.ListNetworkablesInZona.Count);
                if (networkable.CurentGameZona != null)
                {
                    networkable.CurentGameZona.UnRegistration(networkable);
                    this.Change(networkable);
                }
                else
                    this.Teleportation(networkable);

                ListNetworkablesInZona.Add(networkable);
                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected && ListPlayersInZona.Contains(networkable as BasePlayer) == false)
                    ListPlayersInZona.Add(networkable as BasePlayer);
                
                networkable.CurentGameZona = this;
            }
        }

        public void UnRegistration(BaseNetworkable networkable)
        {
            if (this.ListNetworkablesInZona.Contains(networkable))
            {
                ConsoleSystem.Log($"GameZona_{X}_{Y}: UnRegistration");
                this.ListNetworkablesInZona.Remove(networkable);
                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                    this.ListPlayersInZona.Remove((networkable as BasePlayer));
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
                        GameZona lastView;
                        int finishLine = networkable.CurentGameZona.Y + Settings.MapZonesLine;
                        int startLine = networkable.CurentGameZona.Y - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                        {
                            if (ZonaManager.IsInMap(unforward, i))
                            {
                                lastView = ZonaManager.GetGameZona(unforward, i);
                                if (lastView.ListPlayersInZona.Count != 0)
                                {
                                    for (var i1 = 0; i1 < lastView.ListPlayersInZona.Count; i1++)
                                    {
                                        if (networkable.ListViewToMe.Contains(lastView.ListPlayersInZona[i1]))
                                            networkable.ListViewToMe.Remove(lastView.ListPlayersInZona[i1]);
                                    }

                                    NetworkManager.BaseNetworkServer.write.Start();
                                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                                    NetworkManager.BaseNetworkServer.write.EntityID(networkable.UID);
                                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo(lastView.ListPlayersInZona.ToConnectionsList()));
                                }

                                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                                {
                                    NetworkManager.BaseNetworkServer.write.Start();
                                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.GroupDestroy);
                                    NetworkManager.BaseNetworkServer.write.EntityID(lastView.UID);
                                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo((networkable as BasePlayer).NetConnection));
                                }
                            }
                        }
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
                        {
                            if (ZonaManager.IsInMap(i, unforward))
                            {
                                lastView = ZonaManager.GetGameZona(i, unforward);
                                if (lastView.ListPlayersInZona.Count != 0)
                                {
                                    for (var i1 = 0; i1 < lastView.ListPlayersInZona.Count; i1++)
                                    {
                                        if (networkable.ListViewToMe.Contains(lastView.ListPlayersInZona[i1]))
                                            networkable.ListViewToMe.Remove(lastView.ListPlayersInZona[i1]);
                                    }
                                    
                                    NetworkManager.BaseNetworkServer.write.Start();
                                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                                    NetworkManager.BaseNetworkServer.write.EntityID(networkable.UID);
                                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo(lastView.ListPlayersInZona.ToConnectionsList()));
                                }

                                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                                {
                                    NetworkManager.BaseNetworkServer.write.Start();
                                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.GroupDestroy);
                                    NetworkManager.BaseNetworkServer.write.EntityID(lastView.UID);
                                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo((networkable as BasePlayer).NetConnection));
                                }
                            }
                        }
                    }
                }

                if (networkable.ListViewToMe.Count != 0)
                {
                    NetworkManager.BaseNetworkServer.write.Start();
                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.GroupChange);
                    NetworkManager.BaseNetworkServer.write.EntityID(networkable.UID);
                    NetworkManager.BaseNetworkServer.write.UInt32(this.UID);
                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo(networkable.ListViewToMe.ToConnectionsList()));
                }

                if (changeX != 0)
                {
                    int forward = changeX * Settings.MapZonesLine + networkable.CurentGameZona.X;
                    if (ZonaManager.IsInMap(forward, networkable.CurentGameZona.Y))
                    {
                        GameZona lastView;
                        int finishLine = networkable.CurentGameZona.Y + Settings.MapZonesLine;
                        int startLine = networkable.CurentGameZona.Y - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                        {
                            if (ZonaManager.IsInMap(forward, i))
                            {
                                lastView = ZonaManager.GetGameZona(forward, i);
                                if (lastView.ListPlayersInZona.Count != 0)
                                {
                                    for (var i1 = 0; i1 < lastView.ListPlayersInZona.Count; i1++)
                                    {
                                        if (networkable.ListViewToMe.Contains(lastView.ListPlayersInZona[i1]) == false)
                                            networkable.ListViewToMe.Add(lastView.ListPlayersInZona[i1]);
                                    }

                                    networkable.SendNetworkUpdate(new SendInfo(lastView.ListPlayersInZona.ToConnectionsList()));
                                }
                                
                                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                                {
                                    ConsoleSystem.Log($"GameZona_{X}_{Y}: Sync Object For X");
                                    for (var i1 = 0; i1 < lastView.ListNetworkablesInZona.Count; i1++)
                                        lastView.ListNetworkablesInZona[i1].SendNetworkUpdate(new SendInfo((networkable as BasePlayer).NetConnection));
                                }
                            }
                        }
                    }
                }

                if (changeY != 0)
                {
                    int forward = changeY * Settings.MapZonesLine + networkable.CurentGameZona.Y;
                    if (ZonaManager.IsInMap(networkable.CurentGameZona.X, forward))
                    {
                        GameZona lastView;
                        int finishLine = networkable.CurentGameZona.X + Settings.MapZonesLine;
                        int startLine = networkable.CurentGameZona.X - Settings.MapZonesLine;

                        for (int i = startLine; i <= finishLine; i++)
                        {
                            if (ZonaManager.IsInMap(i, forward))
                            {
                                lastView = ZonaManager.GetGameZona(i, forward);
                                if (lastView.ListPlayersInZona.Count != 0)
                                {
                                    for (var i1 = 0; i1 < lastView.ListPlayersInZona.Count; i1++)
                                    {
                                        if (networkable.ListViewToMe.Contains(lastView.ListPlayersInZona[i1]) == false)
                                            networkable.ListViewToMe.Add(lastView.ListPlayersInZona[i1]);
                                    }

                                    networkable.SendNetworkUpdate(new SendInfo(lastView.ListPlayersInZona.ToConnectionsList()));
                                }
                                
                                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                                {
                                    ConsoleSystem.Log($"GameZona_{X}_{Y}: Sync Object For Y");
                                    for (var i1 = 0; i1 < lastView.ListNetworkablesInZona.Count; i1++)
                                        lastView.ListNetworkablesInZona[i1].SendNetworkUpdate(new SendInfo((networkable as BasePlayer).NetConnection));
                                }
                            }
                        }
                    }
                }
            } else
                this.Teleportation(networkable);
        }

        private void Teleportation(BaseNetworkable networkable)
        {
            //TODO: Need rewrite tihs method... Need use ChangeGroup if dont need EntityDestroy =( By ~ TheRyuzaki
            
            if (networkable.CurentGameZona != null)
            {
                if (networkable.ListViewToMe.Count != 0)
                {
                    NetworkManager.BaseNetworkServer.write.Start();
                    NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.EntityDestroy);
                    NetworkManager.BaseNetworkServer.write.EntityID(networkable.UID);
                    NetworkManager.BaseNetworkServer.write.Send(new SendInfo(networkable.ListViewToMe.ToConnectionsList()));
                }

                networkable.ListViewToMe.Clear();

                if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                {
                    
                    int last_finishLineX = networkable.CurentGameZona.X + Settings.MapZonesLine;
                    int last_startLineX = networkable.CurentGameZona.X - Settings.MapZonesLine;
                    int last_finishLineY = networkable.CurentGameZona.Y + Settings.MapZonesLine;
                    int last_startLineY = networkable.CurentGameZona.Y - Settings.MapZonesLine;

                    GameZona lastZona;
                    for (int x = last_startLineX; x <= last_finishLineX; x++)
                    {
                        for (int y = last_startLineY; y <= last_finishLineY; y++)
                        {
                            if (ZonaManager.IsInMap(x, y))
                            {
                                lastZona = ZonaManager.GetGameZona(x, y);
                                
                                NetworkManager.BaseNetworkServer.write.Start();
                                NetworkManager.BaseNetworkServer.write.PacketID(Message.Type.GroupDestroy);
                                NetworkManager.BaseNetworkServer.write.EntityID(lastZona.UID);
                                NetworkManager.BaseNetworkServer.write.Send(new SendInfo((networkable as BasePlayer).NetConnection));
                            }
                        }
                    }
                }
            }
            
            int finishLineX = this.X + Settings.MapZonesLine;
            int startLineX = this.X - Settings.MapZonesLine;
            int finishLineY = this.Y + Settings.MapZonesLine;
            int startLineY = this.Y - Settings.MapZonesLine;


            GameZona newZona;
            for (int x = startLineX; x < finishLineX; x++)
            {
                for (int y = startLineY; y < finishLineY; y++)
                {
                    if (ZonaManager.IsInMap(x, y))
                    {
                        newZona = ZonaManager.GetGameZona(x, y);
                        for (var i = 0; i < newZona.ListPlayersInZona.Count; i++)
                            networkable.ListViewToMe.Add(newZona.ListPlayersInZona[i]);
                        
                        if (networkable is BasePlayer && (networkable as BasePlayer).IsConnected)
                        {
                            for (var i1 = 0; i1 < newZona.ListNetworkablesInZona.Count; i1++)
                                newZona.ListNetworkablesInZona[i1].SendNetworkUpdate(new SendInfo((networkable as BasePlayer).NetConnection));
                        }
                    }
                }
            }
            
            if (networkable.ListViewToMe.Count != 0 && networkable.CurentGameZona != null)
                networkable.SendNetworkUpdate();
        }

        public void OnReceivingDataFromZona(BasePlayer player)
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
                            if (lastZona.ListNetworkablesInZona[i].ListViewToMe.Contains(player) == false)
                            {
                                lastZona.ListNetworkablesInZona[i].ListViewToMe.Add(player);
                                lastZona.ListNetworkablesInZona[i].SendNetworkUpdate(new SendInfo(player.NetConnection));
                            }
                        }
                    }
                }
            }
            
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