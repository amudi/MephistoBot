using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;
using mephisto.Navigation;

namespace mephisto.NanoBots
{
    [Characteristics(
        ContainerCapacity = 20,
        CollectTransfertSpeed = 2,
        Scan = 3,
        MaxDamage = 4,
        DefenseDistance = 5,
        Constitution = 16)
    ]
    public class NavigationCollectorBot : NanoCollector
    {
        private Player player;
        private NavPoint dest;
        public static List<NavNode> allNodes = new List<NavNode>();
        public static List<Point> aznPoint = new List<Point>();
        public static bool first = true;
        Random r = new Random();

        public NavigationCollectorBot()
        {}

        public void DoNext(Player _player)
        {
            player = _player;

            if (first)
            {
                foreach (Entity e in player.Tissue.GetEntitiesByType(EntityEnum.AZN))
                {
                    aznPoint.Add(new Point(e.X, e.Y));
                }
                foreach (NavigationStruct node in MyAI.listNavColObj)
                {
                    foreach (NavNode n in node.Nodes)
                    {
                        allNodes.Add(n);
                    }
                }
                allNodes.Sort(new NavNodeComparer());
                first = false;
            }

            HandleStockKosong();
            HandleEnemies();

            if (this.State == NanoBotState.Moving)
            {
                foreach (NavNode node in allNodes)
                {
                    if (node.NavPt.Location == this.PointInfo)
                    {
                        node.Bot = this;
                        node.Done = false;
                    }
                }
            }

            //cek apakah sudah selesai
            if (this.State == NanoBotState.WaitingOrders)
            {
                bool selesai = true;
                foreach (BaseObjective b in player.Mission.Objectives)
                {
                    if ((b is NavigationObjective) && (b.Status == ObjectiveStatus.ToBeDone))
                    {
                        selesai = false;
                        break;
                    }
                }
                if (selesai)
                {
                    if (this.player.NanoBots.Count < Utils.NbrMaxBots)
                    {
                        //this.MoveTo(player.PierreTeamInjectionPoint);
                        //HandleFreeRoam();
                        this.MoveTo(Global.PF.FindWay(this.Location, Global.MYAI.PierreTeamInjectionPoint).Points);
                        return;
                    }
                    else
                    {
                        this.ForceAutoDestruction();
                    }
                }
                else
                {
                    //set destination
                    if (dest == null)
                    {
                        dest = FindNextDest();
                    }

                    //move to destination and wait
                    if (dest != null)
                    {
                        if ((this.Location != dest.Location) || (player.CurrentTurn < dest.StartTurn))
                        {
                            //this.MoveTo(dest.Location);
                            this.MoveTo(Global.PF.FindWay(this.Location, dest.Location).Points);
                            return;
                        }
                        else
                        {
                            dest = null;
                            foreach (NavNode node in allNodes)
                            {
                                if (node.Bot.Equals(this))
                                {
                                    node.Bot = null;
                                    node.Done = true;
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }

        private NavigationStruct FindActiveNavObj()
        {
            NavigationStruct obj = null;
            foreach (NavigationStruct ns in MyAI.listNavExpObj)
            {
                if ((ns.isActive) && (ns.NavigationObj.Status == ObjectiveStatus.ToBeDone))
                {
                    obj = ns;
                    return obj;
                }
            }

            //kalo null, aktifkan salah satu
            if (obj == null)
            {
                foreach (NavigationStruct ns in MyAI.listNavExpObj)
                {
                    if ((!ns.isActive) && (ns.NavigationObj.Status == ObjectiveStatus.ToBeDone))
                    {
                        obj = ns;
                        ns.isActive = true;
                        return obj;
                    }
                }
            }
            return obj;
        }

        private NavPoint FindNextDest()
        {
            foreach (NavNode p in allNodes)
            {
                if ((p.Bot == null) && (!p.Done) && (!p.NavPt.Reached))
                {
                    p.Bot = this;
                    return p.NavPt;
                }
            }
            return null;
        }

        private void HandleFreeRoam()
        {
            if (this.State == NanoBotState.WaitingOrders)
            {
                Point dest = new Point(this.Location.X + r.Next(-15, 7), this.Location.Y + r.Next(7, -15));
                if (player.Tissue.IsInMap(dest.X, dest.Y)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Bone)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Vessel)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Special))
                {
                    //this.MoveTo(dest);
                    this.MoveTo(Global.PF.FindWay(this.Location, dest).Points);
                    return;
                }
            }
        }

        private void HandleEnemies()
        {
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                if (bot.PlayerID == 0)
                {
                    if (NavigationCollectorBot.squareDist(bot.Location, this.Location) <= (this.DefenseDistance * this.DefenseDistance))
                    {
                        this.StopMoving();
                        this.DefendTo(bot.Location, 8);
                    }
                    else if (NavigationCollectorBot.squareDist(bot.Location, this.Location) <= ((this.DefenseDistance * this.DefenseDistance) + 50))
                    {
                        this.StopMoving();
                        this.MoveTo(Global.PF.FindWay(this.Location, bot.Location).Points);
                    }
                }
            }
        }

        private void HandleStockKosong()
        {
            if (this.Stock == 0)
            {
                Point dest = Global.FindClosest(EntityEnum.AZN, this.Location, this.NanoBotType);
                if (this.Location != dest)
                {
                    if (this.PointInfo != dest)
                    {
                        this.StopMoving();
                        this.MoveTo(Global.PF.FindWay(this.Location, Global.FindClosest(EntityEnum.AZN, this.Location, this.NanoBotType)).Points);
                    }
                }
                else
                {
                    this.StopMoving();
                    this.CollectFrom(this.Location, 10);
                }
            }
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
