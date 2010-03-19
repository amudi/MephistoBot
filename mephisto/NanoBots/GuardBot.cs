using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;
using mephisto.Navigation;

namespace mephisto.NanoBots
{
    [Characteristics(
        ContainerCapacity = 0,
        CollectTransfertSpeed = 0,
        Scan = 5,
        MaxDamage = 5,
        DefenseDistance = 12,
        Constitution = 28)
    ]
    public class GuardBot : NanoCollector
    {
        private Player player;
        private Group agroup = null;
        public Group group
        {
            get
            {
                return agroup;
            }
            set
            {
                agroup = value;
            }
        }

        public enum GuardBotState
        {
            defending = 0,
            chasing,
            idle
        }

        public GuardBotState GuardState;

        public GuardBot()
        { }

        public void DoNext(Player _player)
        {
            player = _player;
            if (group == null)
            {
                foreach (Group g in MyAI.arrGroup)
                {
                    if (!g.hasGuard)
                    {
                        g.hasGuard = true;
                        group = g;
                        g.Guard = this;
                        break;
                    }
                }
            }
            if (group == null) // kalo masih null juga -> bot baru dibuat setelah salah satu hancur
            {
                foreach (NanoBot bot in _player.NanoBots)
                {
                    if (bot is GuardBot)
                    {
                        ((GuardBot)bot).group = null;
                    }
                }
                foreach (Group g in MyAI.arrGroup)
                {
                    g.Guard = null;
                    g.hasGuard = false;
                }
                return;
            }

            HandleEnemies();
            HandleDestinationChange();

            if (this.State == NanoBotState.WaitingOrders)
            {
                this.GuardState = GuardBotState.idle;
                this.MoveTo(Global.PF.FindWay(this.Location, group.ActiveDestination).Points);
            }
        }

        private void HandleDestinationChange()
        {
            if (this.State == NanoBotState.Moving)
            {
                if ((this.PointInfo != group.ActiveDestination) && ((ContainerBot)group.Container[0]).State == NanoBotState.Moving)
                {
                    this.StopMoving();
                    this.MoveTo(Global.PF.FindWay(this.Location, group.ActiveDestination).Points);
                }
            }
        }

        private void HandleEnemies()
        {
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                if (bot.PlayerID == 0)
                {
                    if (GuardBot.squareDist(bot.Location, this.Location) <= (this.DefenseDistance * this.DefenseDistance))
                    {
                        this.StopMoving();
                        this.DefendTo(bot.Location, 2);
                    }
                    else if (GuardBot.squareDist(bot.Location, this.Location) <= 50)
                    {
                        this.StopMoving();
                        //this.MoveTo(bot.Location);
                        this.MoveTo(Global.PF.FindWay(this.Location, bot.Location).Points);
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
