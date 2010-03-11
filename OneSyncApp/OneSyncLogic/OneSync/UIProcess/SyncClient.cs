/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;

namespace OneSync.Synchronization
{
    public class SyncClient
    {
        private  static ProfileProcess profileProcess = null;                
        private  static SyncProcess syncProcess = null;        

        public static ProfileProcess ProfileProcess
        {
            get { return (profileProcess == null) ? new ProfileProcess() : profileProcess; }
            
        }

        public static SyncProcess SyncProcess
        {
            get { return (syncProcess == null) ? new SyncProcess() : syncProcess; }
        }
    }
}
