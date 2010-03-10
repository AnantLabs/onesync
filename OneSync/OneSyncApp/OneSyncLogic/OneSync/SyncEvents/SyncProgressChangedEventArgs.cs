/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{     
    public class SyncProgressChangedEventArgs:EventArgs
    {
        private int progress = 0;
        public SyncProgressChangedEventArgs(int progress):base()
        {
            this.progress = progress;
        }
        public int Value
        {
            get { return this.progress; }
        }
    }
}
