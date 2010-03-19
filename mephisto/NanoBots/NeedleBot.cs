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
    public class NeedleBot : NanoNeedle
    {
        private Player player;

        public NeedleBot()
        {}

        public void DoNext(Player _player)
        {
            player = _player;
            HandleEnemies();
            if (this.ContainerCapacity == this.Stock)
            {
                foreach (Group g in MyAI.arrGroup)
                {
                    if (g.NeedleLocation.Equals(this.Location))
                    {
                        g.Done = true;
                        g.IsSet = false;
                    }
                }
            }
        }

        private void HandleEnemies()
        {
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                if (bot.PlayerID == 0)
                {
                    if (NeedleBot.squareDist(bot.Location, this.Location) <= (this.DefenseDistance * this.DefenseDistance))
                    {
                        this.DefendTo(bot.Location, 5);
                    }
                }
            }
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
