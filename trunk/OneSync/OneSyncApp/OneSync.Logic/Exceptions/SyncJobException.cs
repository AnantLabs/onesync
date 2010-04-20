//Coded by Nguyen van Thuat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    class SyncJobException:Exception
    {
        public SyncJobException (string message):base(message)
        {
        }

        public SyncJobException() : base()
        {
        }

    
    }
}
