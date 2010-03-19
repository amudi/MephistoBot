using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;

namespace mephisto.NanoBots
{
    [Characteristics(ContainerCapacity = 100,
     CollectTransfertSpeed = 0,
     Scan = 10,
     MaxDamage = 5,
     DefenseDistance = 10,
     Constitution = 25)]
    public class DummyNeedle : NanoNeedle
    {
        public DummyNeedle()
        {}
    }
}
