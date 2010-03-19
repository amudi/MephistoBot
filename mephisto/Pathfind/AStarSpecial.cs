using System;
using System.Collections.Generic;
using System.Drawing;
using VG.Map;
using VG.Common;
using mephisto;
using mephisto.Navigation;
using mephisto.NanoBots;

namespace mephisto.Pathfind
{
    /// <summary>
    /// AStar yang hanya memperhitungkan stream.
    /// Khusus untuk NanoExplorer
    /// </summary>
    public class AStarSpecial : AStar
    {
        public AStarSpecial(Tissue t) : base(t)
        {}

        public override Path FindWay(Point from, Point goal)
        {
            if (from.X == goal.X && from.Y == goal.Y)
                return new Path(null, 0);
            
            #region Check if in Cache
            string fromTo = GetCacheKey(from, goal);
            if (cache[fromTo] != null)
            {
                return (Path)cache[fromTo];
            }
            #endregion

            if (!firstTime)
            { // reset
                for (int i = 0; i < 200; i++)
                    for (int j = 0; j < 200; j++)
                        openSet[i][j] = closedSet[i][j] = false;

                if (!openList.IsEmpty)
                    openList.Clear();
            }
            firstTime = false;

            // Prepare starting node
            int g;

            nodeGrid[from.X][from.Y] = new AStarNode(null, from, 0);

            // put start node to openList
            openList.Put(nodeGrid[from.X][from.Y]);
            openSet[from.X][from.Y] = true;

            AStarNode popped = null;
            Point successor = new Point(0, 0);
            do
            {
                if (openList.IsEmpty)	// no path found
                {
                    Path noPath = new Path(null, int.MaxValue);
                    return noPath;
                }

                // Pop the best node so far
                popped = openList.RemoveMin();

                if (popped.PointData.Equals(goal))	// path found
                    break;

                if (closedSet[popped.PointData.X][popped.PointData.Y] == true)
                { // successors already generated
                    continue;
                }

                // Generate successors
                for (int i = 0; i < offset.Length; i++)
                {
                    successor.X = popped.PointData.X + offset[i].X;
                    successor.Y = popped.PointData.Y + offset[i].Y;

                    // ignore some successors
                    if (closedSet[successor.X][successor.Y] == true  // already expanded 
                        || map[successor.X, successor.Y].AreaType == AreaEnum.Bone // impassable
                        || map[successor.X, successor.Y].AreaType == AreaEnum.Vessel // impassable
                        || map[successor.X, successor.Y].AreaType == AreaEnum.Special // impassable
                        || map.IsInMap(successor.X, successor.Y) == false // not in map
                        || (successor.X == from.X && successor.Y == from.Y) // same as from loc
                        )
                    {
                        continue;
                    }

                    if ((!Global.MYAI.PierreTeamInjectionPoint.IsEmpty) && (!Global.isPierreAIDead))
                    { // avoid Pierre
                        if (Global.CanShoot(successor, Global.MYAI.PierreTeamInjectionPoint, 10))
                            continue;
                    }

                    // valid successor

                    // not at goal yet
                    g = popped.G + GetTurnCost(successor, offset[i]);

                    // if node with successor position is already in open...
                    if (openSet[successor.X][successor.Y] == true)
                    {
                        // is it lower?
                        if (nodeGrid[successor.X][successor.Y].G < g)
                        {
                            continue;	// skip this successor
                        }

                    }

                    // set successor to parent
                    nodeGrid[successor.X][successor.Y] = new AStarNode(popped, successor, popped.PathLength + 1);

                    // add sucessor to open list
                    nodeGrid[successor.X][successor.Y].H = GetHCost(successor, goal);
                    nodeGrid[successor.X][successor.Y].G = g;
                    nodeGrid[successor.X][successor.Y].F = nodeGrid[successor.X][successor.Y].G + nodeGrid[successor.X][successor.Y].H;

                    openSet[successor.X][successor.Y] = true;
                    openList.Put(nodeGrid[successor.X][successor.Y]);
                }

                // put popped on closed list (already expanded)
                closedSet[popped.PointData.X][popped.PointData.Y] = true;

            } while (true);

            Point[] path = new Point[popped.PathLength];
            AStarNode current = popped;
            AStarNode temp = null;
            int pathElem = path.Length - 1;
            while (current.Parent != null) // until original loc is met
            {
                path[pathElem--] = current.PointData;
                temp = current;
                current = current.Parent;
                temp = null;	// enable Garbage-Collection
            }

            Path foundPath = new Path(path, path.Length);
            
            // hide destination Point
            Point[] pt = new Point[foundPath.Points.Length + 2];
            for (int idx = 0; idx < pt.Length - 2; idx++)
            {
                pt[idx] = foundPath.Points[idx];
            }
            pt[pt.Length - 1] = new Point(0, 0);
            foundPath = new Path(pt, pt.Length + 1);

            #region Put in Cache Only if Dest is AZN or HP
            if (cacheable[goal.X][goal.Y] == true)
            {
                // from->to
                cache[fromTo] = foundPath;
            }
            #endregion

            return foundPath;
        }

        protected override int GetTurnCost(Point at, Point move)
        {
            int cost = 1;
            BloodStream bs = map.IsInStream(at.X, at.Y);
            if (bs != null)
            {
                if (bs.Direction == BloodStreamDirection.EstWest)
                {
                    if (move.X == -1)
                        cost -= 2;
                    else if (move.X == 1)
                        cost += 2;
                }
                else if (bs.Direction == BloodStreamDirection.NorthSouth)
                {
                    if (move.Y == 1)
                        cost -= 2;
                    else if (move.Y == -1)
                        cost += 2;
                }
                else if (bs.Direction == BloodStreamDirection.SouthNorth)
                {
                    if (move.Y == -1)
                        cost -= 2;
                    else if (move.Y == 1)
                        cost += 2;
                }
                else if (bs.Direction == BloodStreamDirection.WestEst)
                {
                    if (move.X == 1)
                        cost -= 2;
                    else if (move.X == -1)
                        cost += 2;
                }
            }
            if (cost <= 0)
                cost = 1;
            return cost;
        }
    }
}
