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
    public class UniqueNavigationExplorerBot : NanoExplorer
    {
        private Player player;
        private NavPoint[] pts;
        private UniqueNavigationObjective nav;
        private int ctr = 0;
        public bool dapet = false;

        public UniqueNavigationExplorerBot()
        {}

        public void DoNext(Player _player)
        {
            player = _player;

            if (!dapet)
            {
                foreach (UniqueNavigationStruct u in MyAI.listUNavExpObj)
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

            if ((this.State == NanoBotState.WaitingOrders) && (dapet))
            {
                if ((ctr < pts.Length) && (nav.Status == ObjectiveStatus.ToBeDone))
                {
                    if /*((!pts[ctr].Reached) && */(this.Location != pts[ctr].Location)//)
                    {
                        //this.MoveTo(pts[ctr].Location);
                        this.MoveTo(Global.SPF.FindWay(this.Location, pts[ctr].Location).Points);
                        return;
                    }
                    if ((pts[ctr].StartTurn <= player.CurrentTurn) && (this.Location == pts[ctr].Location))
                    {
                        ctr++;
                        return;
                    }
                }
                if ((ctr >= pts.Length) || (nav.Status != ObjectiveStatus.ToBeDone))
                {
                    this.StopMoving();
                    //this.ForceAutoDestruction();
                    return;
                }
            }
        }

        /*private NavPoint FindNearestNavPoint()
        {
            NavPoint dest = null;
            int min = int.MaxValue;
            foreach (NavPoint p in nav.NavPoints)
            {
                if (!p.Reached)
                {
                    //int jarak = UniqueNavigationExplorerBot.squareDist(p.Location, this.Location);
                    int jarak = Global.GetPathLength(this.Location, p.Location);
                    if (jarak < min)
                    {
                        min = jarak;
                        dest = p;
                    }
                }
            }
            return dest;
        }*/

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
