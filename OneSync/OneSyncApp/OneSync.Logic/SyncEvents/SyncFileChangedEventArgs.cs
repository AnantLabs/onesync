using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncFileChangedEventArgs: EventArgs
    {       
        public SyncFileChangedEventArgs(ChangeType type, string relativePath)
        {
            RelativePath = relativePath;
            ChangeType = type;
        }
        public string RelativePath
        {
            set;
            get;
        }
        public ChangeType ChangeType
        {
            set;
            get;
        }
    }
}
