using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;
using mephisto.Pathfind;

namespace mephisto.NanoBots
{
    [Characteristics(
        ContainerCapacity = 0,
        CollectTransfertSpeed = 0,
        Scan = 30,
        MaxDamage = 0,
        DefenseDistance = 0,
        Constitution = 10)
    ]
    public class ScannerBot : VG.Common.NanoExplorer
    {
        public ScannerBot()
        {}

        public void DoNext(Player _player)
        {
            if /*(*/(this.State == NanoBotState.WaitingOrders)/* && (ScannerBot.squareDist(this.Location, _player.AI.Location) > (this.Scan * this.Scan)))*/
            {
                this.MoveTo(Global.PF.FindWay(this.Location, _player.AI.Location).Points);
                return;
            }
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
