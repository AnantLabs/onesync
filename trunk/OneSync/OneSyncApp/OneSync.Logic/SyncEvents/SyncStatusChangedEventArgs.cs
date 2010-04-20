//Coded by Nguyen van Thuat
/*
 $Id: SyncCancelledEventArgs.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncStatusChangedEventArgs:EventArgs
    {
        private string _msg;

        public SyncStatusChangedEventArgs(string msg) : base()
        { 
            this._msg = msg; 
        }

        public string Message
        {
            get { return _msg; }
        }
    }
}
