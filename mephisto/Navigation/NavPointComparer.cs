using System;
using System.Collections.Generic;
using VG.Common;
using VG.Mission;
using VG.Map;
using System.Collections;
using System.Drawing;

namespace mephisto.Navigation
{
    public class NavPointComparer : IComparer<NavPoint>
    {
        public int Compare(NavPoint x, NavPoint y)
        {
            if ((x.EndTurn <= y.EndTurn) && (x.StartTurn < y.StartTurn))
                return -1;
            else if ((x.EndTurn == y.EndTurn) && (x.StartTurn == y.StartTurn))
                return 0;
            else if ((x.EndTurn >= y.EndTurn) && (x.StartTurn > y.StartTurn))
                return 1;
            else if (x.StartTurn < y.StartTurn)
                return -1;
            else if (x.StartTurn == y.StartTurn)
                return 0;
            else if (x.StartTurn > y.StartTurn)
                return 1;
            //NOT REACHED
            return 2;
        }
    }
}
