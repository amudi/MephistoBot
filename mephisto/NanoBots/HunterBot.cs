using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;

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
    public class HunterBot : NanoCollector
    {
        private Player player;
        private static Random r = new Random();
        private int jarakKejar = 50;

        public HunterBot()
        {}

        public void DoNext(Player _player)
        {
            player = _player;
            HandleEnemies();

            if (this.State == NanoBotState.WaitingOrders)
            {
                if (this.Location == player.PierreTeamInjectionPoint)
                {
                    Global.isPierreAIDead = true;
                    bool found = false;
                    Point target = Point.Empty;
                    foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
                    {
                        // Pierre AI dead
                        // cari White Cell lainnya
                        if (bot.PlayerID == 0)
                        {
                            jarakKejar = 1200;
                            found = true;
                            target = bot.Location;
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (player.NanoBots.Count == Utils.NbrMaxBots)
                        {
                            // nanobots full, destroy some
                            this.ForceAutoDestruction();
                            Global.NBHUNTERBOTTOBUILD--;
                        }
                        else
                        {
                            HandleFinishingTouch();
                        }
                    }
                    else
                    {
                        this.MoveTo(Global.PF.FindWay(this.Location, target).Points);
                    }
                }
                else
                {
                    if (!Global.isPierreAIDead)
                    {
                        this.MoveTo(Global.PF.FindWay(this.Location, Global.MYAI.PierreTeamInjectionPoint).Points);
                    }
                    else
                    {
                        Point dest = new Point(this.Location.X + r.Next(-15, 15), this.Location.Y + r.Next(-15, 15));
                        if (player.Tissue.IsInMap(dest.X, dest.Y)
                            && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Bone)
                            && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Vessel)
                            && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Special))
                        {
                            this.MoveTo(Global.SPF.FindWay(this.Location, dest).Points);
                            return;
                        }
                    }
                }
            }
        }

        private void HandleFinishingTouch()
        {
            int min = int.MaxValue;
            NanoBotInfo target = null;
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                int jarak = HunterBot.squareDist(this.Location, bot.Location);
                if (jarak < min)
                {
                    jarak = min;
                    target = bot;
                }
            }
            if (target != null)
                this.MoveTo(Global.PF.FindWay(this.Location, target.Location).Points);
            else
            {
                Point dest = new Point(this.Location.X + r.Next(-15, 15), this.Location.Y + r.Next(-15, 15));
                if (player.Tissue.IsInMap(dest.X, dest.Y)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Bone)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Vessel)
                    && (player.Tissue[dest.X, dest.Y].AreaType != AreaEnum.Special))
                {
                    this.MoveTo(Global.SPF.FindWay(this.Location, dest).Points);
                    return;
                }
            }
        }

        private void HandleEnemies()
        {
            foreach (NanoBotInfo bot in player.OtherNanoBotsInfo)
            {
                if (bot.PlayerID == 0)
                {
                    if (HunterBot.squareDist(bot.Location, this.Location) <= (this.DefenseDistance * this.DefenseDistance))
                    {
                        this.StopMoving();
                        this.DefendTo(bot.Location, 8);
                    }
                    else if (HunterBot.squareDist(bot.Location, this.Location) <= jarakKejar)
                    {
                        this.StopMoving();
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
