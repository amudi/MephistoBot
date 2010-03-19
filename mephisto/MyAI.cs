using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using VG.Common;
using VG.Map;
using VG.Mission;
using mephisto.NanoBots;
using mephisto.Navigation;
using mephisto.Pathfind;

namespace mephisto
{
    public class MyAI : Player
    {
        #region member

        public static Group[] arrGroup;
        public static List<NavigationStruct> listNavExpObj = new List<NavigationStruct>();
        public static List<NavigationStruct> listNavColObj = new List<NavigationStruct>();
        public static List<UniqueNavigationStruct> listUNavExpObj = new List<UniqueNavigationStruct>();
        public static List<UniqueNavigationStruct> listUNavColObj = new List<UniqueNavigationStruct>();
        private static int nextGroup = 0;
        private static List<Point> hoshimiPoint = new List<Point>();
        private bool firstInjected = true;
        private bool firstInjected2 = true;
        private bool first1 = true;
        private bool skipped = false;
        //private bool firstNeedleBuilt = false;

        //current bots count
        private int numHunters;
        private int numScanners;
        private int numContainers;
        private int numDefender;
        private int numNeedle;
        private int numNavExpBot;
        private int numNavColBot;
        private int numUNavExpBot;
        private int numUNavColBot;
        private int numBlocker;
        private int numGuard;

        #endregion

        #region constructor

        public MyAI(string name, int id) : base(name, id)
        {
            // init the Global class
            new Global();
            // inisiasi static vars
            nextGroup = 0;
            listNavExpObj = new List<NavigationStruct>();
            listNavColObj = new List<NavigationStruct>();
            hoshimiPoint = new List<Point>();

            // static di kelas lain
            NavigationCollectorBot.aznPoint = new List<Point>();
            NavigationCollectorBot.allNodes = new List<NavNode>();
            NavigationCollectorBot.first = true;
            NavigationExplorerBot.allNodes = new List<NavNode>();
            NavigationExplorerBot.first = true;
            UniqueNavigationCollectorBot.aznPoint = new List<Point>();
            ContainerBot.aznPoint = new List<Point>();

            Global.MYAI = this;

            arrGroup = new Group[Global.NBCONTAINERTOBUILD / 2];
            for (int i = 0; i < arrGroup.Length; i++)
            {
                arrGroup[i] = new Group();
            }

            // delegates
            this.ChooseInjectionPointEvent += new ChooseInjectionPointEventHandler(MyAI_ChooseInjectionPointEvent);
            this.WhatToDoNextEvent += new WhatToDoNextEventHandler(MyAI_WhatToDoNextEvent);
        }

        #endregion

        #region choose injection point event

        private Point GetInjectionPoint()
        {
            HPNodeMinHeap hpHeap = new HPNodeMinHeap(this.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint).Count);
            int radius = 200;
            foreach (Entity e in this.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint))
            {
                HPNode hp = new HPNode(0, new Point(e.X, e.Y)); // buat HPNode baru

                // hitung area cost
                if (this.Tissue[hp.Loc.X, hp.Loc.Y].AreaType == AreaEnum.LowDensity)
                    hp.cost -= 5;
                else if (this.Tissue[hp.Loc.X, hp.Loc.Y].AreaType == AreaEnum.MediumDensity)
                    hp.cost -= 1;
                else if (this.Tissue[hp.Loc.X, hp.Loc.Y].AreaType == AreaEnum.HighDensity)
                    hp.cost += 3;

                /*
                // perhitungkan PierreIP
                foreach (InjectionPointInfo ipInfo in OtherInjectionPointsInfo)
                {
                    if (ipInfo.PlayerID == 0)
                        if (MyAI.squareDist(ipInfo.Location, hp.Loc) <= radius)
                            hp.cost += 500;
                }*/

                // cek sekitar hp ini
                Point p;
                int numAzn = 0;
                int numHp = 0;
                foreach (Entity entity in this.Tissue.GetEntitiesByType(EntityEnum.AZN))
                {
                    p = new Point(entity.X, entity.Y);
                    if (MyAI.squareDist(hp.Loc, p) <= radius)
                    //if (Global.GetPathLength(hp.Loc, p) <= radius)
                    {
                        numAzn++;
                        if (numAzn == 1)
                            hp.cost -= 8;
                        else
                            hp.cost -= 1;
                    }
                    p = Point.Empty;
                }
                foreach (Entity entity in this.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint))
                {
                    if (e.X == hp.Loc.X && e.Y == hp.Loc.Y)
                        continue;   // skip if it is here

                    p = new Point(entity.X, entity.Y);
                    if (MyAI.squareDist(hp.Loc, p) <= radius)
                    //if (Global.GetPathLength(hp.Loc, p) <= radius)
                    {
                        numHp++;
                        if (numHp == 1)
                            hp.cost -= 1;
                        else if ((numHp <= 4) && (numHp > 1))
                            hp.cost -= 3;
                        else
                            hp.cost -= 5;
                    }
                    p = Point.Empty;
                }
                foreach (BaseObjective baseObj in Mission.Objectives)
                {
                    if (baseObj is UniqueNavigationObjective)
                    {
                        /*foreach (NavPoint navp in ((UniqueNavigationObjective)baseObj).NavPoints)
                        {
                            //if (MyAI.squareDist(hp.Loc, navp.Location) <= (radius * 3))
                            if (navp.EndTurn <= 500)
                            {
                                if (Global.GetPathLength(hp.Loc, navp.Location) < 100)
                                    hp.cost -= 5;
                                else
                                    hp.cost += 10;
                            }                            
                        }*/
                        if (Global.GetPathLength(hp.Loc, ((UniqueNavigationObjective)baseObj).NavPoints[0].Location) < 100)
                        {
                            if (((UniqueNavigationObjective)baseObj).NavPoints[0].EndTurn <= 500)
                                hp.cost -= 5;
                        }
                        else
                        {
                            hp.cost += 10;
                        }
                    }
                    else if (baseObj is NavigationObjective)
                    {
                        foreach (NavPoint navp in ((NavigationObjective)baseObj).NavPoints)
                        {
                            //if (MyAI.squareDist(hp.Loc, navp.Location) <= (radius * 3))
                            if (navp.EndTurn <= 500)
                            {
                                if (Global.GetPathLength(hp.Loc, navp.Location) < 100)
                                    hp.cost -= 5;
                                else
                                    hp.cost += 10;
                            }                            
                        }
                    }
                }
                //hp.cost += (int)(hp.cost * 0.001f);
                hpHeap.Put(hp); // masukkan ke heap
            }
            return hpHeap.GetMin().Loc; // ambil minimum element
        }

        public void MyAI_ChooseInjectionPointEvent()
        {
            Global.PF = new AStar(this.Tissue);
            Global.SPF = new AStarSpecial(this.Tissue);
            
            this.AI.InternalName = "AI";
            this.InjectionPointWanted = GetInjectionPoint();
            //if (pilihan == Point.Empty)
            //{
            //    EntityCollection eCollection = this.Tissue.GetEntitiesByType(EntityEnum.AZN);
            //    this.InjectionPointWanted = new Point(eCollection[0].X, eCollection[0].Y);
            //    //this.InjectionPointWanted = new Point(100,85);
            //}

        }

        #endregion

        #region what to do next event

        public void MyAI_WhatToDoNextEvent()
        {
            numHunters = 0;
            numScanners = 0;
            numContainers = 0;
            numDefender = 0;
            numNeedle = 0;
            numNavExpBot = 0;
            numNavColBot = 0;
            numUNavExpBot = 0;
            numUNavColBot = 0;
            numBlocker = 0;
            numGuard = 0;
            
            #region handle objectives

            if (first1)
            {
                int jmlCol = 0;
                int jmlExp = 0;
                foreach (BaseObjective bo in Mission.Objectives)
                {
                    if (bo.Status == ObjectiveStatus.ToBeDone)
                    {
                        if (bo is UniqueNavigationObjective)
                        {
                            UniqueNavigationObjective unav = (UniqueNavigationObjective)bo;
                            if (unav.NanoBotType == NanoBotType.NanoExplorer)
                            {
                                if (unav.NavPoints.Count > 20)
                                    continue;
                                Global.NBUNAVEXPTOBUILD++;
                                listUNavExpObj.Add(new UniqueNavigationStruct(unav));
                            }
                            else if (unav.NanoBotType == NanoBotType.Unknown)
                            {
                                bool needStock = false;
                                foreach (NavPoint np in unav.NavPoints)
                                {
                                    if (np.Stock > 0)
                                    {
                                        needStock = true;
                                        break;
                                    }
                                }
                                if (!needStock)
                                {
                                    if (unav.NavPoints.Count > 20)
                                        continue;
                                    Global.NBUNAVEXPTOBUILD++;
                                    listUNavExpObj.Add(new UniqueNavigationStruct(unav));
                                }
                                else
                                {
                                    if (unav.NavPoints.Count > 13)
                                        continue;
                                    Global.NBUNAVCOLTOBUILD++;
                                    listUNavColObj.Add(new UniqueNavigationStruct(unav));
                                }
                            }
                            else
                            {
                                if (unav.NavPoints.Count > 13)
                                    continue;
                                Global.NBUNAVCOLTOBUILD++;
                                listUNavColObj.Add(new UniqueNavigationStruct(unav));
                            }
                        }
                        else if (bo is NavigationObjective)
                        {
                            NavigationObjective n = (NavigationObjective)bo;
                            if (n.NanoBotType == NanoBotType.NanoExplorer)
                            {
                                if (n.NavPoints.Count > 20)
                                    continue;
                                jmlExp += n.NavPoints.Count;
                                listNavExpObj.Add(new NavigationStruct(n));
                            }
                            else if (n.NanoBotType == NanoBotType.Unknown)
                            {
                                bool needStock = false;
                                foreach (NavPoint np in n.NavPoints)
                                {
                                    if (np.Stock > 0)
                                    {
                                        needStock = true;
                                        break;
                                    }
                                }
                                if (!needStock)
                                {
                                    if (n.NavPoints.Count > 20)
                                        continue;
                                    jmlExp += n.NavPoints.Count;
                                    listNavExpObj.Add(new NavigationStruct(n));
                                }
                                else
                                {
                                    if (n.NavPoints.Count > 13)
                                        continue;
                                    jmlCol += n.NavPoints.Count;
                                    listNavColObj.Add(new NavigationStruct(n));
                                }
                            }
                            else
                            {
                                if (n.NavPoints.Count > 13)
                                    continue;
                                jmlCol += n.NavPoints.Count;
                                listNavColObj.Add(new NavigationStruct(n));
                            }
                        }
                    }
                }
                if (jmlExp > 0)
                {
                    Global.NBNAVEXPTOBUILD = (jmlExp / 3);
                    if ((jmlExp % 3) > 0)
                        Global.NBNAVEXPTOBUILD++;
                    if (Global.NBNAVEXPTOBUILD > 5)
                        Global.NBNAVEXPTOBUILD = 5;
                }
                if (jmlCol > 0)
                {
                    Global.NBNAVCOLTOBUILD = (jmlCol / 2);
                    if ((jmlCol % 2) > 0)
                        Global.NBNAVCOLTOBUILD++;
                    if (Global.NBNAVCOLTOBUILD > 5)
                        Global.NBNAVCOLTOBUILD = 7;
                }
                first1 = false;
            }


            #endregion
           
            #region construct bots

            foreach (NanoBot bot in this.NanoBots)
            {
                if (bot is HunterBot)
                {
                    numHunters++;
                    ((HunterBot)bot).DoNext(this);
                }
                else if (bot is DefenderBot)
                {
                    numDefender++;
                    ((DefenderBot)bot).DoNext(this);
                }
                else if (bot is ScannerBot)
                {
                    numScanners++;
                    ((ScannerBot)bot).DoNext(this);
                }
                else if (bot is ContainerBot)
                {
                    numContainers++;
                    ((ContainerBot)bot).DoNext(this);
                }
                else if (bot is GuardBot)
                {
                    numGuard++;
                    ((GuardBot)bot).DoNext(this);
                }
                else if (bot is NeedleBot)
                {
                    numNeedle++;
                    ((NeedleBot)bot).DoNext(this);
                }
                else if (bot is UniqueNavigationExplorerBot)
                {
                    numUNavExpBot++;
                    ((UniqueNavigationExplorerBot)bot).DoNext(this);
                }
                else if (bot is UniqueNavigationCollectorBot)
                {
                    numUNavColBot++;
                    ((UniqueNavigationCollectorBot)bot).DoNext(this);
                }
                else if (bot is NavigationExplorerBot)
                {
                    numNavExpBot++;
                    ((NavigationExplorerBot)bot).DoNext(this);
                }
                else if (bot is NavigationCollectorBot)
                {
                    numNavColBot++;
                    ((NavigationCollectorBot)bot).DoNext(this);
                }
                else if (bot is BlockerBot)
                {
                    numBlocker++;
                }
            }

            Point prevDest = this.AI.PointInfo;
            bool stop = false;
            if (firstInjected2)
            {
                bool taken = false;
                foreach (NanoBotInfo bot in OtherNanoBotsInfo)
                {
                    if ((bot.Location == AI.Location) && ((bot.NanoBotType == NanoBotType.NanoNeedle) || (bot.NanoBotType == NanoBotType.NanoBlocker)))
                    {
                        taken = true;
                        break;
                    }
                }
                if (!taken)
                {
                    AI.Build(typeof(NeedleBot), "Needle");
                    //firstNeedleBuilt = true;
                }
                firstInjected2 = false;
            }
            if (numHunters < Global.NBHUNTERBOTTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(HunterBot), "Hunter");
                //return;
            }
            if (numDefender < Global.NBDEFENDERTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(DefenderBot), "Defender");
                //return;
            }
            if (numScanners < Global.NBSCANNERTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(ScannerBot), "Scanner");
                //return;
            }
            if (numGuard < Global.NBGUARDTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(GuardBot), "Guard");
                //return;
            }
            if (numContainers < Global.NBCONTAINERTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(ContainerBot), "Containr");
                //return;
            }
            if (numUNavExpBot < Global.NBUNAVEXPTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(UniqueNavigationExplorerBot), "UNavExplr");
                foreach (UniqueNavigationStruct o in MyAI.listUNavExpObj)
                {
                    o.isAssigned = false;
                }
                foreach (NanoBot bot in NanoBots)
                {
                    if (bot is UniqueNavigationExplorerBot)
                        ((UniqueNavigationExplorerBot)bot).dapet = false;
                }
                //return;
            }
            if (numUNavColBot < Global.NBUNAVCOLTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(UniqueNavigationCollectorBot), "UNavColtr");
                foreach (UniqueNavigationStruct o in MyAI.listUNavColObj)
                {
                    o.isAssigned = false;
                }
                foreach (NanoBot bot in NanoBots)
                {
                    if (bot is UniqueNavigationCollectorBot)
                        ((UniqueNavigationCollectorBot)bot).dapet = false;
                }
                //return;
            }
            if (numNavExpBot < Global.NBNAVEXPTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(NavigationExplorerBot), "NavExplr");
                // reset semua Navigasi
                foreach (NavNode node in NavigationExplorerBot.allNodes)
                {
                    node.Bot = null;
                    node.Done = false;
                }
                //foreach (NavigationStruct ns in MyAI.listNavExpObj)
                //{
                //    if ((ns.isActive) && (ns.NavigationObj.Status == ObjectiveStatus.ToBeDone))
                //    {
                //        ns.isActive = false;
                //    }
                //}
                //return;
            }
            if (numNavColBot < Global.NBNAVCOLTOBUILD)
            {
                AI.StopMoving();
                stop = true;
                AI.Build(typeof(NavigationCollectorBot), "NavColtr");
                // reset semua Navigasi
                foreach (NavNode node in NavigationCollectorBot.allNodes)
                {
                    node.Bot = null;
                    node.Done = false;
                }
                //foreach (NavigationStruct ns in MyAI.listNavColObj)
                //{
                //    if ((ns.isActive) && (ns.NavigationObj.Status == ObjectiveStatus.ToBeDone))
                //    {
                //        ns.isActive = false;
                //    }
                //}
                //return;
            }
            if ((numBlocker < Global.NBBLOCKERTOBUILD) &&
                    (((!firstInjected) &&
                        (AI.Location != InjectionPointWanted) && (!skipped)))) //||
                //    (((this.CurrentTurn % 500) == 0) &&
                //        (this.NanoBots.Count < Utils.NbrMaxBots))
                //                                                   ))
            {
                //bool isOnHP = false;
                //foreach (Entity e in Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint))
                //{
                //    if ((e.X == AI.Location.X) && (e.Y == AI.Location.Y))
                //    {
                //        isOnHP = true;
                //        break;
                //    }
                //}
                //if (!isOnHP)
                //{
                bool isOnHP = false;
                foreach (Entity e in Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint))
                {
                    if (AI.Location.X == e.X && AI.Location.Y == e.Y)
                    {
                        isOnHP = true;
                        break;
                    }
                }
                if (!isOnHP)
                {
                    skipped = true;
                    AI.StopMoving();
                    stop = true;
                    AI.Build(typeof(BlockerBot), "Blockr");
                }
                //}
                //return;
            }
            //if ((this.CurrentTurn >= 500) && (!Global.AllHPBuilt) && (NanoBots.Count < Utils.NbrMaxBots))
            //{
            //    AI.StopMoving();
            //    stop = true;
            //    AI.Build(typeof(DummyCrapBot), "DummySht");
            //    //return;
            //}
            if (stop)
                this.AI.MoveTo(Global.PF.FindWay(AI.Location, prevDest).Points);

            // Filter Nanobots
            ArrayList hunters = new ArrayList(Global.NBHUNTERBOTTOBUILD);
            ArrayList defenders = new ArrayList(Global.NBDEFENDERTOBUILD);
            ArrayList containers = new ArrayList(Global.NBCONTAINERTOBUILD);
            ArrayList blockers = new ArrayList(Global.NBBLOCKERTOBUILD);
            ArrayList scanners = new ArrayList(Global.NBSCANNERTOBUILD);
            ArrayList unavcol = new ArrayList(Global.NBUNAVCOLTOBUILD);
            ArrayList unavexp = new ArrayList(Global.NBUNAVEXPTOBUILD);
            ArrayList navcol = new ArrayList(Global.NBNAVCOLTOBUILD);
            ArrayList navexp = new ArrayList(Global.NBNAVEXPTOBUILD);
            ArrayList guards = new ArrayList(Global.NBGUARDTOBUILD);
            foreach (NanoBot bot in NanoBots)
            {
                if (bot is HunterBot)
                {
                    hunters.Add((HunterBot)bot);
                }
                else if (bot is DefenderBot)
                {
                    defenders.Add((DefenderBot)bot);
                }
                else if (bot is ContainerBot)
                {
                    containers.Add((ContainerBot)bot);
                }
                else if (bot is BlockerBot)
                {
                    blockers.Add((BlockerBot)bot);
                }
                else if ((bot is ScannerBot) || (bot is DummyCrapBot))
                {
                    scanners.Add((ScannerBot)bot);
                }
                else if (bot is UniqueNavigationCollectorBot)
                {
                    unavcol.Add((UniqueNavigationCollectorBot)bot);
                }
                else if (bot is UniqueNavigationExplorerBot)
                {
                    unavexp.Add((UniqueNavigationExplorerBot)bot);
                }
                else if (bot is NavigationCollectorBot)
                {
                    navcol.Add((NavigationCollectorBot)bot);
                }
                else if (bot is NavigationExplorerBot)
                {
                    navexp.Add((NavigationExplorerBot)bot);
                }
                else if (bot is GuardBot)
                {
                    guards.Add((GuardBot)bot);
                }
            }

            #endregion

            // update NeedleGrid Status
            foreach (Entity e in this.Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint))
            {
                foreach (NanoBotInfo bot in OtherNanoBotsInfo)
                {
                    if ((bot.Location.X == e.X) &&
                        (bot.Location.Y == e.Y) && 
                        (bot.NanoBotType == NanoBotType.NanoNeedle) &&
                        (Global.NeedleGrid[e.X][e.Y] == null)
                       )
                    {
                        Global.NeedleGrid[e.X][e.Y] = new NeedleInfo();
                        Global.NeedleGrid[e.X][e.Y].Skipped = true;
                    }
                }
                
                // cek apakah ada NeedleBot yang dihancurkan WhiteCell
                if ((Global.NeedleGrid[e.X][e.Y] != null) && (!Global.NeedleGrid[e.X][e.Y].Skipped))
                {
                    // cek semua NeedleBot yang ada
                    bool found = false;
                    foreach (NanoBot bot in NanoBots)
                    {
                        if (bot is NeedleBot)
                        {
                            if ((bot.Location.X == e.X) && (bot.Location.Y == e.Y))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                        Global.NeedleGrid[e.X][e.Y] = null;
                }
            }
            
            // cek apakah semua HP sudah penuh
            if (!Global.AllHPBuilt)
            {
                bool adaYangKosong = false;
                foreach (Entity e in Tissue.GetEntitiesByType(EntityEnum.HoshimiPoint))
                {
                    Point pt = new Point(e.X, e.Y);
                    if (Global.NeedleGrid[pt.X][pt.Y] == null)
                    {
                        adaYangKosong = true;
                        break;
                    }
                }
                if (!adaYangKosong)
                    Global.AllHPBuilt = true;
            }

            #region AI logic

            foreach (NanoBot bot in NanoBots)
            {
                if ((bot is DefenderBot) && (bot.State == NanoBotState.Defending))
                {
                    this.AI.StopMoving();   // AI stop while Defender defends
                    return;
                }
            }

            // cek tujuan, apakah sudah dibuat Needle oleh lawan
            if (AI.State == NanoBotState.Moving)
            {
                if (Global.NeedleGrid[AI.PointInfo.X][AI.PointInfo.Y] != null)
                {
                    AI.StopMoving();
                    return;
                }
                if (this.CurrentTurn % 10 == 0)
                {
                    Point nearHP = Global.FindClosest(EntityEnum.HoshimiPoint, this.AI.Location, this.AI.NanoBotType);
                    if  ((this.AI.PointInfo != nearHP) &&
                         (Global.GetPathLength(this.AI.Location, nearHP) < Global.GetPathLength(this.AI.Location, this.AI.PointInfo)))
                    {
                        this.AI.StopMoving();
                        this.AI.MoveTo(Global.PF.FindWay(this.AI.Location, nearHP).Points);
                        return;
                    }
                }
            }

            if (this.AI.State == NanoBotState.WaitingOrders)
            {
                Random r = new Random();
                Point dest = Point.Empty;
                Point tempatBlocker = Point.Empty;
                
                if ((Global.AllHPBuilt) && (NanoBots.Count < Utils.NbrMaxBots))
                {
                    // penuhi batas maksimum bot
                    if (Global.NeedleGrid[AI.Location.X][AI.Location.Y] == null)
                    {
                        this.AI.Build(typeof(DummyNeedle), "Dummy");
                        Global.NeedleGrid[AI.Location.X][AI.Location.Y] = new NeedleInfo();
                        return;
                    }
                    else
                    {
                        dest = new Point(this.AI.Location.X + r.Next(-5, 5), this.AI.Location.Y + r.Next(-5, 5));
                        this.AI.MoveTo(Global.PF.FindWay(AI.Location, dest).Points);
                        return;
                    }
                }

                if (firstInjected)
                {
                    dest = this.InjectionPointWanted;
                    firstInjected = false;
                    return;
                }
                else
                {
                    if (!Global.AllHPBuilt)
                        dest = Global.FindClosest(EntityEnum.HoshimiPoint, this.AI.Location, NanoBotType.NanoAI);
                    else
                    {
                        dest = new Point(this.AI.Location.X + r.Next(-5, 5), this.AI.Location.Y + r.Next(-5, 5));
                        return;
                    }
                }

                if (dest == Point.Empty)    // semua HP sudah diisi
                {
                    Global.AllHPBuilt = true;
                }

                if ((AI.Location.X == tempatBlocker.X && AI.Location.Y == tempatBlocker.Y) && (firstInjected))
                {
                    firstInjected = false;
                    AI.Build(typeof(BlockerBot));
                    return;
                }

                if (this.AI.Location != dest)
                {
                    this.AI.MoveTo(Global.PF.FindWay(this.AI.Location, dest).Points);
                    return;
                }
                else
                {
                    // sebelum buat NeedleBot, cek apakah jumlah NanoBot sudah sampai batas max
                    if (NanoBots.Count >= Utils.NbrMaxBots)
                    {
                        bool destroyOne = false;

                        // cek apakah PierreAI dead -> bisa destroy hunters, scanners dan defenders
                        if (Global.isPierreAIDead)
                        {
                            if (hunters.Count > 0)
                            {
                                ((HunterBot)hunters[0]).ForceAutoDestruction();
                                Global.NBHUNTERBOTTOBUILD -= 1;
                                destroyOne = true;
                            }
                            if ((!destroyOne) && (defenders.Count > 0))
                            {
                                ((DefenderBot)defenders[0]).ForceAutoDestruction();
                                Global.NBDEFENDERTOBUILD -= 1;
                                destroyOne = true;
                            }
                            if ((!destroyOne) && (scanners.Count > 0))
                            {
                                ((ScannerBot)scanners[0]).ForceAutoDestruction();
                                Global.NBSCANNERTOBUILD -= 1;
                                destroyOne = true;
                            }
                        }
                        if (!destroyOne)
                        {
                            bool selesai = true;
                            foreach (BaseObjective b in Mission.Objectives)
                            {
                                if ((b is NavigationObjective) && (b.Status == ObjectiveStatus.ToBeDone))
                                {
                                    selesai = false;
                                    break;
                                }
                            }
                            if (selesai)    // jika semua NavigationObjective selesai -> bisa destroy NavigationBots
                            {
                                if (navexp.Count > 0)
                                {
                                    ((NavigationExplorerBot)navexp[0]).ForceAutoDestruction();
                                    Global.NBNAVEXPTOBUILD -= 1;
                                    destroyOne = true;
                                }
                                if ((!destroyOne) && (navcol.Count > 0))
                                {
                                    ((NavigationCollectorBot)navcol[0]).ForceAutoDestruction();
                                    Global.NBNAVCOLTOBUILD -= 1;
                                    destroyOne = true;
                                }
                            }
                        }
                        if (!destroyOne)
                        {
                            bool selesai = true;
                            foreach (BaseObjective b in Mission.Objectives)
                            {
                                if ((b is UniqueNavigationObjective) && (b.Status == ObjectiveStatus.ToBeDone))
                                {
                                    selesai = false;
                                    break;
                                }
                            }
                            if (selesai)    // jika semua UniqueNavigationObjective selesai 
                            {               // -> bisa destroy UniqueNavigationBots
                                if (navexp.Count > 0)
                                {
                                    ((UniqueNavigationExplorerBot)unavexp[0]).ForceAutoDestruction();
                                    Global.NBUNAVEXPTOBUILD -= 1;
                                    destroyOne = true;
                                }
                                if ((!destroyOne) && (navcol.Count > 0))
                                {
                                    ((UniqueNavigationCollectorBot)unavcol[0]).ForceAutoDestruction();
                                    Global.NBUNAVCOLTOBUILD -= 1;
                                    destroyOne = true;
                                }
                            }
                        }
                        // navigation dan defender udah abis -> blocker dan containers
                        if (!destroyOne)
                        {
                            if (blockers.Count > 0)
                            {
                                ((BlockerBot)blockers[0]).ForceAutoDestruction();
                                Global.NBBLOCKERTOBUILD -= 1;
                                destroyOne = true;
                            }
                            if ((!destroyOne) && (guards.Count > 0))
                            {
                                ((GuardBot)guards[0]).ForceAutoDestruction();
                                Global.NBGUARDTOBUILD -= 1;
                                destroyOne = true;
                            }
                            if ((!destroyOne) && (containers.Count > 4))
                            {
                                ((ContainerBot)containers[0]).ForceAutoDestruction();
                                Global.NBCONTAINERTOBUILD -= 1;
                                destroyOne = true;
                            }
                        }
                    }
                    else
                    {
                        this.AI.Build(typeof(NeedleBot), "Needle");
                        Global.NbNeedle++;
                        Global.NeedleGrid[this.AI.Location.X][this.AI.Location.Y] = new NeedleInfo();
                        Global.NeedleGrid[this.AI.Location.X][this.AI.Location.Y].NeedleBuilt = true;
                        //Global.NeedleGrid[this.AI.Location.X][this.AI.Location.Y].NeedleTargetted = false;
                        if (nextGroup < arrGroup.Length)
                        {
                            arrGroup[nextGroup].NeedleLocation = this.AI.Location;
                            arrGroup[nextGroup].IsSet = true;
                            arrGroup[nextGroup].Done = false;
                            //Global.NeedleGrid[this.AI.Location.X][this.AI.Location.Y].NeedleTargetted = true;
                            nextGroup++;
                        }
                        else
                        {
                            //kalo semua udah di set, cari yang terdekat
                            int jmin = int.MaxValue;
                            Group sel = null;
                            foreach (NanoBot bot in NanoBots)
                            {
                                if (bot is ContainerBot)
                                {
                                    ContainerBot b = (ContainerBot)bot;
                                    if ((b.group.Done) && (b.Stock != 0) && (!b.group.IsSet))
                                    {
                                        //int jarak = MyAI.squareDist(this.AI.Location, b.Location);
                                        int jarak = Global.GetPathLength(this.AI.Location, b.Location);
                                        if (jarak < jmin)
                                        {
                                            jmin = jarak;
                                            sel = b.group;
                                        }
                                    }
                                }
                            }
                            if (sel != null)
                            {
                                sel.NeedleLocation = this.AI.Location;
                                sel.IsSet = true;
                                sel.Done = false;
                                //Global.NeedleGrid[this.AI.Location.X][this.AI.Location.Y].NeedleTargetted = true;
                            }
                            else
                            {
                                // semua grup sudah diAssign, masukkan ke HPList
                                Global.HPList.Add(this.AI.Location);
                            }
                        }
                        return;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Flag

        public override Bitmap Flag
        {
            get
            {
                return MyResource.merahputih;
            }
        }

        #endregion

        #region static methods

        private static int squareDist(Point a, Point b)
        {
            return (((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
        }

        #endregion
    }
}
