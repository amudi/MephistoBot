using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;

namespace mephisto.NanoBots
{
    [Characteristics(ContainerCapacity = 50,
     CollectTransfertSpeed = 5,
     Scan = 0,
     MaxDamage = 0,
     DefenseDistance = 0,
     Constitution = 15)]
    public class ContainerBot : NanoContainer
    {
        public Group group
        {
            get
            {
                return this.agroup;
            }
            set
            {
                this.agroup = value;
            }
        }
        private Group agroup = null;

        public enum ContainerState {
            Ngisi = 0,
            Transfer,
            Nyerang,
            JalanBiasa
        }

        private ContainerState containerState;

        public static List<Point> aznPoint = new List<Point>();
        private static bool first = true;

        public ContainerBot()
        {}

        public void DoNext(Player _player)
        {
            //inisiasi list azn point
            if (first)
            {
                first = false;
                foreach (VG.Map.Entity e in _player.Tissue.get_EntitiesByType(VG.Map.EntityEnum.AZN))
                {
                    aznPoint.Add(new Point(e.X, e.Y));
                }
            }

            //alokasi grup
            if (this.group == null)
            {
                for (int i = 0; i < MyAI.arrGroup.Length; i++)
                {
                    if (MyAI.arrGroup[i].CountContainer < 2)
                    {
                        this.group = MyAI.arrGroup[i];
                        //MyAI.arrGroup[i].CountContainer += 1;
                        MyAI.arrGroup[i].Container.Add(this);
                        break;
                    }
                }
            }
            if (this.group == null) // kalo masih null juga -> bot baru dibuat setelah salah satu hancur
            {
                foreach (NanoBot bot in _player.NanoBots)
                {
                    if (bot is ContainerBot)
                    {
                        ((ContainerBot)bot).group = null;   // reset grup
                    }
                    if ((bot is NeedleBot) && (((NeedleBot)bot).Stock < 100))
                    {
                        Global.HPList.Add(bot.Location);
                    }
                }
                for (int i = 0; i < MyAI.arrGroup.Length; i++)
                {
//                    for (int j = 0; j < MyAI.arrGroup[i].Container.Count; j++)
//                    //foreach (ContainerBot bot in MyAI.arrGroup[i].Container)
//                    {
//                        if (((ContainerBot)MyAI.arrGroup[i].Container[j]).HitPoint <= 0)
//                        {
//                            ContainerBot tmpBot = (ContainerBot)MyAI.arrGroup[i].Container[3 - j];
                            MyAI.arrGroup[i].Container = new ArrayList();
//                            if (tmpBot.HitPoint > 0)
//                            {
//                                MyAI.arrGroup[i].Container.Add(tmpBot);
//                            }
                            MyAI.arrGroup[i].Done = true;
                            MyAI.arrGroup[i].IsSet = false;
                            MyAI.arrGroup[i].NeedleLocation = Point.Empty;
//                            break;
//                        }
//                    }
                }
                return;
            }

            HandleDefendingGuard();
            HandleStockKosong();
            HandleGroupChange();
            HandleAIChangeDirection();
            HandleEmptyNeedle();

            #region ContainerBot's logic
            if (this.State == NanoBotState.WaitingOrders)
            {
                if ((!this.group.IsSet) || (group.Done))
                {
                    // cek jika ada Needle di HPList
                    //if (Global.HPList.Count > 0)
                    //{
                    //    this.group.IsSet = true;
                    //    this.group.Done = false;
                    //    this.group.NeedleLocation = (Point)Global.HPList[0];
                    //    Global.HPList.RemoveAt(0);
                    //    //this.containerState = ContainerState.Transfer;
                    //    return;
                    //}
                    //else
                        if (group.ActiveDestination != _player.AI.Location)
                        {
                            group.ActiveDestination = _player.AI.Location;
                        }
                        this.MoveTo(Global.PF.FindWay(this.Location, group.ActiveDestination).Points);
                }
                else
                {
                    this.StopMoving();
                    this.containerState = ContainerState.Transfer;
                    if (this.Location.Equals(this.group.NeedleLocation))
                    {
                        this.TransferTo(this.group.NeedleLocation, 10);
                    }
                    else
                    {
                        if (group.ActiveDestination != group.NeedleLocation)
                        {
                            group.ActiveDestination = group.NeedleLocation;
                        }
                        this.MoveTo(Global.PF.FindWay(this.Location, this.group.ActiveDestination).Points);
                    }
                    return;
                }
            }
            #endregion
        }

        private void HandleDefendingGuard()
        {
            if ((group.Guard != null) && (group.hasGuard) && (this.State == NanoBotState.Moving))
            {
                if (this.group.Guard.State == NanoBotState.Defending)
                {
                    this.StopMoving();
                }
            }
        }

        private void HandleEmptyNeedle()
        {
            if (!this.group.IsSet)
            {
                if (Global.HPList.Count > 0)
                {
                    this.group.IsSet = true;
                    this.group.Done = false;
                    int min = int.MaxValue;
                    Point tmp = Point.Empty;
                    foreach (Point hp in Global.HPList)
                    {
                        int jarak = Global.GetPathLength(this.Location, hp);
                        if (jarak < min)
                        {
                            min = jarak;
                            tmp = hp;
                        }
                    }
                    this.group.NeedleLocation = tmp;
                    Global.HPList.Remove(tmp);
                }
            }
        }

        private void HandleStockKosong()
        {
            if (this.Stock < this.ContainerCapacity)
            {
                this.containerState = ContainerState.Ngisi;
                // find AZN destination
                Point dest = Point.Empty;
                //Point azn1 = Global.FindClosest(EntityEnum.AZN, this.Location, this.NanoBotType);
                //Point azn2 = Global.FindClosest(EntityEnum.AZN, Global.MYAI.AI.Location, this.NanoBotType);
                //if (this.group.IsSet)
                //{
                //    dest = Global.FindClosest(EntityEnum.AZN, this.group.NeedleLocation, this.NanoBotType);
                //}
                //else
                //{
                    dest = Global.FindClosest(EntityEnum.AZN, this.Location, this.NanoBotType);
                //}
                if (this.Location != dest)
                {
                    if (this.PointInfo != dest)
                    {
                        this.StopMoving();
                        group.ActiveDestination = Global.FindClosest(EntityEnum.AZN, this.Location, this.NanoBotType);
                        this.MoveTo(Global.PF.FindWay(this.Location, group.ActiveDestination).Points);
                    }
                }
                else
                {
                    this.StopMoving();
                    group.ActiveDestination = Point.Empty;
                    this.CollectFrom(this.Location, 10);
                    this.containerState = ContainerState.JalanBiasa;
                }
            }
        }

        private void HandleGroupChange()
        {
            //kalo grup IsSet == true, dan stock penuh,
            //cek lokasi. Kalo beda dengan needleLocation dan tujuannya juga bukan needle location,
            //alihkan tujuan
            if (this.group.IsSet)
            {
                if (this.Stock != 0)
                {
                    if ((this.Location != this.group.NeedleLocation) && (this.PointInfo != this.group.NeedleLocation))
                    {
                        this.StopMoving();
                        this.containerState = ContainerState.JalanBiasa;
                        group.ActiveDestination = group.NeedleLocation;
                        this.MoveTo(Global.PF.FindWay(this.Location, this.group.ActiveDestination).Points);
                    }
                }
            }
        }

        private void HandleAIChangeDirection()
        {
            if ((this.State == NanoBotState.Moving) &&
                (Global.MYAI.AI.PointInfo != this.PointInfo) &&
                (!this.group.IsSet) &&
                (this.containerState == ContainerState.JalanBiasa)
               )
            {
                this.StopMoving();
                group.ActiveDestination = Global.MYAI.AI.PointInfo;
                this.MoveTo(Global.PF.FindWay(this.Location, group.ActiveDestination).Points);
            }
        }

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }
    }
}
