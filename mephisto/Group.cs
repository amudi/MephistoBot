using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using mephisto.NanoBots;

namespace mephisto
{
    /// <summary>
    /// Kelas yang merepresentasikan sebuah grup dari NeedleBot dan ContainerBot
    /// </summary>
    public class Group
    {
        #region Private members

        private Point needle = Point.Empty;
        private Point activeDest = Point.Empty;
        private bool done;
        private bool isset;
        public ArrayList Container;
        public bool hasGuard = false;
        public GuardBot Guard;

        #endregion

        public Group()
        {
            done = false;
            isset = false;
            Container = new ArrayList(2);
        }

        #region Public methods
        /*
        /// <summary>
        /// menambahkan container kedalam group
        /// </summary>
        /// <param name="newContainer">ContainerBot baru</param>
        /// <returns>true jika berhasil, false jika gagal</returns>
        public bool AddContainer(ContainerBot newContainer)
        {
            if (container1 == null)
            {
                container1 = newContainer;
                return true;
            }
            else if (container2 == null)
            {
                container2 = newContainer;
                return true;
            }
            else
                return false;
        }
        */

        public bool Done
        {
            get
            {
                return done;
            }
            set
            {
                done = value;
            }
        }

        public bool IsSet
        {
            get
            {
                return isset;
            }
            set
            {
                isset = value;
            }
        }

        public Point NeedleLocation
        {
            get
            {
                return needle;
            }
            set
            {
                needle = value;
            }
        }

        public Point ActiveDestination
        {
            get
            {
                return activeDest;
            }
            set
            {
                activeDest = value;
            }
        }

        public int CountContainer
        {
            get
            {
                return Container.Count;
            }
        }

        #endregion
    }
}
