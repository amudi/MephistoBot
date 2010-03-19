using System;
using System.Collections.Generic;
using VG.Common;
using VG.Mission;
using VG.Map;
using System.Collections;
using System.Drawing;

namespace mephisto.Navigation
{
    public class NavNodeComparer : IComparer<NavNode>
    {
        public int Compare(NavNode x, NavNode y)
        {
            if ((x.NavPt.EndTurn <= y.NavPt.EndTurn) && (x.NavPt.StartTurn < y.NavPt.StartTurn))
                return -1;
            else if ((x.NavPt.EndTurn == y.NavPt.EndTurn) && (x.NavPt.StartTurn == y.NavPt.StartTurn))
                return 0;
            else if ((x.NavPt.EndTurn >= y.NavPt.EndTurn) && (x.NavPt.StartTurn > y.NavPt.StartTurn))
                return 1;
            else if (x.NavPt.StartTurn < y.NavPt.StartTurn)
                return -1;
            else if (x.NavPt.StartTurn == y.NavPt.StartTurn)
                return 0;
            else if (x.NavPt.StartTurn > y.NavPt.StartTurn)
                return 1;
            //NOT REACHED
            return 2;
        }
    }
}
