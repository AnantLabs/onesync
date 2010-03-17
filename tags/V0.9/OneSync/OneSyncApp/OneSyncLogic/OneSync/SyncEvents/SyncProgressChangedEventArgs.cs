/*
 $Id: SyncProgressChangedEventArgs.cs 66 2010-03-10 07:48:55Z gclin009 $
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
