using System;
using System.Collections.Generic;
using System.Drawing;

namespace mephisto
{
    public class HPNode
    {
        public int cost;
        public Point Loc;

        public HPNode(int cost_, Point loc)
        {
            cost = cost_;
            Loc = loc;
        }
    }
}
