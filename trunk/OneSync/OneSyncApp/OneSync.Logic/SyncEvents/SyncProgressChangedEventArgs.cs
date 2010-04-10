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
        private int currentProgress = 0;
        private int totalProgress = 1;

        public SyncProgressChangedEventArgs(int currentProgress, int totalProgress):base()
        {
            this.currentProgress = currentProgress;
            this.totalProgress = totalProgress;
        }

        /// <summary>
        /// Returns current progress which is less than 100.
        /// </summary>
        public int Progress
        {
            get {
                if (totalProgress != 0 && currentProgress < totalProgress)
                    return 100 * currentProgress / totalProgress;
                else
                    return 100;
            }
        }
    }
}
