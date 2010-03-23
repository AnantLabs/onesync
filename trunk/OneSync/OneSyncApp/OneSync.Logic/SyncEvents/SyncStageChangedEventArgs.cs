/*
 $Id: SyncStatusChangedEventArgs.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncStageChangedEventArgs:EventArgs
    {
        private Stage stage;

        public enum Stage
        {
            VERIFY_PATCH,
            APPLY_PATCH,
            GENERATE_PATCH,
            UPDATE_DATA
        }

        public SyncStageChangedEventArgs(Stage stage):base()
        {
            this.stage = stage;
        }

        public Stage Stage
        {
            get { return this.stage; }
        }
    }
}
