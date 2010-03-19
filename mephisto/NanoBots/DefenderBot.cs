using System;
using System.Collections.Generic;
using System.Drawing;
using VG.Common;
using VG.Map;

namespace mephisto.NanoBots
{
    [Characteristics(
        ContainerCapacity = 0,
        CollectTransfertSpeed = 0,
        MaxDamage = 5,
        Constitution = 28,
        Scan = 5,
        DefenseDistance = 12)
    ]
    public class DefenderBot : NanoCollector
    {
        private Player player;
        private Random rand = new Random();

        public DefenderBot()
        {}

        public void DoNext(Player _player)
        {
            this.player = _player;
            HandleEnemies();
            HandleAIChangeDirection();
            if (this.State == NanoBotState.WaitingOrders)
            {
                Point p = new Point(player.AI.PointInfo.X/* + rand.Next(-5, 5)*/, player.AI.PointInfo.Y/* + rand.Next(-5, 5)*/);
                if (player.Tissue.IsInMap(p.X, p.Y) &&
                    (player.Tissue[p.X, p.Y].AreaType != AreaEnum.Bone) &&
                    (player.Tissue[p.X, p.Y].AreaType != AreaEnum.Vessel) &&
                    (player.Tissue[p.X, p.Y].AreaType != AreaEnum.Special))
                {
                    //this.MoveTo(p);
                    this.MoveTo(Global.PF.FindWay(this.Location, p).Points);
                }
                return;
            }
        }

        private void HandleEnemies()
        {
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                if (bot.PlayerID == 0)
                {
                    if (DefenderBot.squareDist(bot.Location, this.Location) <= (this.DefenseDistance * this.DefenseDistance))
                    {
                        this.StopMoving();
                        this.DefendTo(bot.Location, 2);
                    }
                    else if (DefenderBot.squareDist(bot.Location, this.Location) <= 50)
                    {
                        this.StopMoving();
                        //this.MoveTo(bot.Location);
                        this.MoveTo(Global.PF.FindWay(this.Location, bot.Location).Points);
                    }
                }
            }
        }

        private void HandleAIChangeDirection()
        {
            if ((this.State == NanoBotState.Moving) && (Global.MYAI.AI.PointInfo != this.PointInfo))
            {
                this.StopMoving();
                this.MoveTo(Global.PF.FindWay(this.Location, Global.MYAI.AI.PointInfo).Points);
            }
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
