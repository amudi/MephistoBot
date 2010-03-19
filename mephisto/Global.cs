using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using VG.Map;
using VG.Common;
using mephisto;
using mephisto.Navigation;
using mephisto.NanoBots;
using mephisto.Pathfind;

namespace mephisto
{
    public class Global
    {
        //AI Stats
        public static MyAI MYAI;
        public static ArrayList HPList; // HP dengan Needle yang belum di-assign ke salah satu Group
        public static bool AllHPBuilt;
        public static int NbNeedle;

        //max bots count
        public static int NBSCANNERTOBUILD;
        public static int NBCONTAINERTOBUILD;
        public static int NBDEFENDERTOBUILD;
        public static int NBHUNTERBOTTOBUILD;
        public static int NBNAVEXPTOBUILD;
        public static int NBNAVCOLTOBUILD;
        public static int NBUNAVEXPTOBUILD;
        public static int NBUNAVCOLTOBUILD;
        public static int NBBLOCKERTOBUILD;
        public static int NBGUARDTOBUILD;

        //pathfinder
        public static AStar PF;
        public static AStarSpecial SPF;

        //Pierre 
        public static bool isPierreAIDead;

        public static NeedleInfo[][] NeedleGrid = new NeedleInfo[200][];
        
        public Global()
        {
            // init NeedleGrid
            for (int i = 0; i < 200; i++)
                NeedleGrid[i] = new NeedleInfo[200];

            // init static variables
            Global.NBSCANNERTOBUILD = 1;
            Global.NBDEFENDERTOBUILD = 3;
            Global.NBHUNTERBOTTOBUILD = 3;
            Global.NBCONTAINERTOBUILD = 10;
            Global.NBNAVEXPTOBUILD = 0;
            Global.NBNAVCOLTOBUILD = 0;
            Global.NBUNAVEXPTOBUILD = 0;
            Global.NBUNAVCOLTOBUILD = 0;
            Global.NBBLOCKERTOBUILD = 5;
            Global.NBGUARDTOBUILD = Global.NBCONTAINERTOBUILD/2;

            Global.NbNeedle = 0;

            Global.isPierreAIDead = false;
            HPList = new ArrayList(30);
            Global.AllHPBuilt = false;
        }

        public static bool CanShoot(Point from, Point to, int defenseDist)
        {
            int dist = ((from.X - to.X) * (from.X - to.X)) + 
                       ((from.Y - to.Y) * (from.Y - to.Y));
            bool retval = dist < (defenseDist * defenseDist) ? true : false;
            return retval;
        }

        public static Point FindClosest(EntityEnum e, Point from, NanoBotType botType)
        {
            // initial boundary
            int boundary = 50;

            // initial region
            ArrayList list = new ArrayList(20);

            EntityCollection c =
                e == EntityEnum.HoshimiPoint ?
                MYAI.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint) :
                MYAI.Tissue.GetEntitiesByType(EntityEnum.AZN);

            /*
            // check whether already on nearest entity
            foreach (Entity ee in c)
            {
                if (from.X == ee.X && from.Y == ee.Y)
                    return new Point(ee.X, ee.Y);
            }*/

            while ((list.Count <= 0) || (boundary > 200))
            {
                foreach (Entity ee in c)
                {
                    if ((e == EntityEnum.HoshimiPoint) && (Global.NeedleGrid[ee.X][ee.Y] != null))
                    {
                        if (Global.NeedleGrid[ee.X][ee.Y].Skipped)
                            continue;	// skip this HP cos already built by other player

                        if (Global.NeedleGrid[ee.X][ee.Y].NeedleBuilt)
                            continue;   // AI ignores needle already built

                        else if (botType == NanoBotType.NanoContainer && Global.NeedleGrid[ee.X][ee.Y].NeedleTargetted)
                            continue;	// Container ignores needle already targetted (may not be built yet)

                    }
                    else if ((e == EntityEnum.AZN) && (!Global.isPierreAIDead))
                    { // check if we should avoid this AZN
                        if (Global.CanShoot(new Point(ee.X, ee.Y), Global.MYAI.PierreTeamInjectionPoint, 10))
                            continue;
                    }

                    if (ee.X >= (from.X - boundary) && ee.X <= (from.X + boundary) &&
                        ee.Y >= (from.Y - boundary) && ee.Y <= (from.Y + boundary)
                        )
                    {
                        list.Add(new Point(ee.X, ee.Y));
                    }
                }

                if (list.Count > 0)
                    break;

                // else increment boundary
                boundary += 50;
            }
            if (list.Count <= 0)
                return Point.Empty;

            // Now calculate each HP/AZN to find the closest one
            Point p = Point.Empty;
            int curDistance;
            int minDistance = int.MaxValue;
            Point closestPt = from;
            for (int i = 0; i < list.Count; i++)
            {
                p = (Point)list[i];
                curDistance = Global.GetPathLength(from, p);
                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    closestPt = p;
                }
            }

            // return closest HP/AZN;
            return closestPt;
        }

        public static int GetPathLength(Point from, Point to)
        {
            Path path = Global.PF.FindWay(from, to);
            return path.Cost;
        }

        public static Point NeedToDefend(Point cur, int defenseDist)
        {
            foreach (NanoBotInfo bot in MYAI.OtherNanoBotsInfo)
            {
                if (bot.PlayerID != 0)
                    continue;   // only shoot at Pierre
                if (Global.CanShoot(cur, bot.Location, defenseDist))
                    return bot.Location;
            }
            return Point.Empty; // no enemies nearby
        }

        public static bool IsValidPoint(Point pt)
        {
            return ( (Global.MYAI.Tissue.IsInMap(pt.X, pt.Y)) &&
                     (Global.MYAI.Tissue[pt.X, pt.Y].AreaType != AreaEnum.HighDensity) &&
                     (Global.MYAI.Tissue[pt.X, pt.Y].AreaType != AreaEnum.MediumDensity) &&
                     (Global.MYAI.Tissue[pt.X, pt.Y].AreaType != AreaEnum.LowDensity)
                   );
        }
    }
}
