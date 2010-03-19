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
    public class UniqueNavigationCollectorBot : NanoCollector
    {
        private Player player;
        private NavPoint[] pts;
        private UniqueNavigationObjective nav;
        private int ctr = 0;
        private bool first = true;
        public bool dapet = false;
        public static List<Point> aznPoint = new List<Point>();

        public UniqueNavigationCollectorBot()
        {
        }

        public void DoNext(Player _player)
        {
            player = _player;

            if (first)
            {
                foreach (Entity e in player.Tissue.get_EntitiesByType(EntityEnum.AZN))
                {
                    aznPoint.Add(new Point(e.X, e.Y));
                }
                first = false;
            }

            if (!dapet)
            {
                foreach (UniqueNavigationStruct u in MyAI.listUNavColObj)
                {
                    if (!u.isAssigned)
                    {
                        u.isAssigned = true;
                        dapet = true;
                        nav = u.NavigationObj;
                        //u.NavigationObj.NavPoints.Sort(new NavPointComparer());
                        pts = u.NavigationObj.NavPoints.ToArray();
                    }
                }
            }
            

            HandleStockKosong();
            HandleEnemies();

            if ((this.State == NanoBotState.WaitingOrders) && (dapet))
            {
                if (ctr < pts.Length)
                {
                    if /*((!pts[ctr].Reached) && */(this.Location != pts[ctr].Location)//)
                    {
                        //this.MoveTo(pts[ctr].Location);
                        this.MoveTo(Global.PF.FindWay(this.Location, pts[ctr].Location).Points);
                        return;
                    }
                    if ((pts[ctr].StartTurn <= player.CurrentTurn) && (this.Location == pts[ctr].Location))
                    {
                        ctr++;
                        return;
                    }
                }
                if ((ctr >= pts.Length) && (nav.Status != ObjectiveStatus.ToBeDone))
                {
                    this.StopMoving();
                    //this.ForceAutoDestruction();
                    return;
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

        private void HandleEnemies()
        {
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                if (bot.PlayerID == 0)
                {
                    if (UniqueNavigationCollectorBot.squareDist(bot.Location, this.Location) <= (this.DefenseDistance * this.DefenseDistance))
                    {
                        this.StopMoving();
                        this.DefendTo(bot.Location, 2);
                    }
                    else if (UniqueNavigationCollectorBot.squareDist(bot.Location, this.Location) <= 300)
                    {
                        this.StopMoving();
                        //this.MoveTo(bot.Location);
                        this.MoveTo(Global.PF.FindWay(this.Location, bot.Location).Points);
                    }
                }
            }
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }

}
