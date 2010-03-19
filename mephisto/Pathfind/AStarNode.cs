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
    public class AStarNode
    {
        public AStarNode Parent;
        public Point PointData;
        public int PathLength;
        public float F, H;
        public int G;

        public AStarNode(AStarNode parent, Point p, int length)
        {
            this.Parent = parent;
            this.PointData = p;
            this.PathLength = length;
        }
    }
}
