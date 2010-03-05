using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class RenameAction:SyncAction
    {
        private string oldPath = "";
        private string newPath = "";
        private string hash = "";

        public RenameAction(string syncSource, string changeIn, ChangeType action, string oldPath, string newPath, string hash)
            : base(syncSource, changeIn, action)
        {
            this.oldPath = oldPath;
            this.newPath = newPath;
            this.hash = hash;
        }

        public string NewItemPath
        {
            get
            {
                return this.newPath;
            }                
        }

        public string OldItemPath
        {
            get
            {
                return this.oldPath;
            }
        }

        public string ItemHash
        {
            get
            {
                return this.hash;
            }
        }

        public override void Execute()
        {
            
        }
    }
}
