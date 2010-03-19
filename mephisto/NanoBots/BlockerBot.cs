using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;

namespace mephisto.NanoBots
{
    [Characteristics(ContainerCapacity = 0,
     CollectTransfertSpeed = 0,
     Scan = 10,
     MaxDamage = 0,
     DefenseDistance = 0,
     Constitution = 80)]
    public class BlockerBot : NanoBlocker
    {
        public BlockerBot()
        {}
    }
}
