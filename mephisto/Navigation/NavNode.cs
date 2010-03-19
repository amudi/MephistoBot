using System;
using System.Collections.Generic;
using VG.Common;
using VG.Mission;
using VG.Map;
using System.Collections;
using System.Drawing;

namespace mephisto.Navigation
{
    public class NavNode
    {
        private NavPoint pt;
        private bool isDone;
        private bool isAssigned;
        private NanoMoveable bot;

        public NavNode(NavPoint p)
        {
            pt = p;
            isAssigned = false;
            isDone = false;
        }

        public NanoMoveable Bot
        {
            get
            {
                return bot;
            }
            set
            {
                bot = value;
            }
        }

        public bool Done
        {
            get
            {
                return isDone;
            }
            set
            {
                isDone = value;
            }
        }

        public bool Assigned
        {
            get
            {
                return isAssigned;
            }
            set
            {
                isAssigned = value;
            }
        }

        public NavPoint NavPt
        {
            get
            {
                return pt;
            }
        }
    }
}
