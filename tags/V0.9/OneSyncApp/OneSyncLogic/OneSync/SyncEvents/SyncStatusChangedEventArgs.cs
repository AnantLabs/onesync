/*
 $Id: SyncStatusChangedEventArgs.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncStatusChangedEventArgs:EventArgs
    {
        private SyncStatus status;
        public SyncStatusChangedEventArgs(SyncStatus status):base()
        {
            this.status = status;
        }

        public SyncStatus Status
        {
            get { return this.status; }
        }
    }
}
