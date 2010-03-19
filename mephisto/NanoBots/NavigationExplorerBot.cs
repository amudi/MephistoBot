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
        ContainerCapacity = 0,
        CollectTransfertSpeed = 0,
        Scan = 20,
        MaxDamage = 0,
        DefenseDistance = 0,
        Constitution = 20)
    ]
    public class NavigationExplorerBot : NanoExplorer
    {
        private Player player;
        private NavPoint dest;
        public static List<NavNode> allNodes = new List<NavNode>();
        public static bool first = true;
        Random r = new Random();
        private int numCurHP = 0;

        public NavigationExplorerBot()
        {}

        public void DoNext(Player _player)
        {
            player = _player;

            if (first)
            {
                foreach (NavigationStruct node in MyAI.listNavExpObj)
                {
                    foreach (NavNode n in node.Nodes)
                    {
                        allNodes.Add(n);
                    }
                }
                //allNodes.Sort(new NavNodeComparer());
                first = false;
            }

            /*int numBot = 0;
            foreach (NanoBot bot in _player.NanoBots)
            {
                if (bot is NavigationExplorerBot)
                    numBot++;
            }
            if (Global.NBNAVEXPTOBUILD < numBot)
            {
                // pertama kali atau bot sudah ada yang mati
                foreach (NavNode node in allNodes)
                {
                    node.Bot = null;
                    node.Done = false;
                    return;
                }
            }*/

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
                    HandleFreeRoam();
                    //this.MoveTo(Global.SPF.FindWay(this.Location, Global.MYAI.PierreTeamInjectionPoint).Points);
                    return;
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
                            this.MoveTo(Global.SPF.FindWay(this.Location, dest.Location).Points);
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
            /*if (this.State == NanoBotState.WaitingOrders)
            {
                Point dest = new Point(this.Location.X + r.Next(-15, 15), this.Location.Y + r.Next(-15, 15));
                if (player.Tissue.IsInMap(dest.X, dest.Y)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Bone)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Vessel)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Special))
                {
                    this.MoveTo(Global.SPF.FindWay(this.Location, dest).Points);
                    return;
                }
            }*/
            this.MoveTo(Global.SPF.FindWay(this.Location, 
                new Point(player.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint)[numCurHP].X, 
                          player.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint)[numCurHP++].Y)).Points);
            if (numCurHP >= player.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint).Count)
                numCurHP = 0;
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
