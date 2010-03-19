using System;
using System.Collections.Generic;
using VG.Common;
using VG.Mission;
using VG.Map;
using System.Collections;
using System.Drawing;

namespace mephisto.Navigation
{
    public class NavigationStruct
    {
        public NavigationStruct(NavigationObjective o)
        {
            obj = o;
            isActive = false;
            listNode = new List<NavNode>();
            foreach (NavPoint p in o.NavPoints)
            {
                listNode.Add(new NavNode(p));
            }
        }

        private NavigationObjective obj;
        public NavigationObjective NavigationObj
        {
            get
            {
                return obj;
            }
            set
            {
                obj = value;
            }
        }

        public bool isActive;
        private List<NavNode> listNode;
        public List<NavNode> Nodes
        {
            get
            {
                return listNode;
            }
        }
    }
}
