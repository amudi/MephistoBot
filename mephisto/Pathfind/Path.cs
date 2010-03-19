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
    public class Path
    {
        public Point[] Points;
        public int Cost;

        public Path(Point[] path, int cost)
        {
            this.Points = path;
            this.Cost = cost;
        }
    }
}
