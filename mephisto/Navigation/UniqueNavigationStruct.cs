using System;
using System.Collections.Generic;
using VG.Common;
using VG.Mission;
using VG.Map;
using System.Collections;
using System.Drawing;

namespace mephisto.Navigation
{
    public class UniqueNavigationStruct
    {
        private UniqueNavigationObjective obj;

        public UniqueNavigationStruct(UniqueNavigationObjective o)
        {
            obj = o;
            isAssigned = false;
        }


        public UniqueNavigationObjective NavigationObj
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

        public bool isAssigned;
    }
}
