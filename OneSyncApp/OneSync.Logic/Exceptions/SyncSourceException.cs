using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncSourceException: Exception
    {
        public SyncSourceException (string message):base(message){}
        public SyncSourceException ():base(){}
    }
}
