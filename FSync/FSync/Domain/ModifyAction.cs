using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class ModifyAction:SyncAction
    {
        private string itemPath = "";
        private string oldHash = "";
        private string newHash = "";
        public ModifyAction(string syncSource, string changeIn, ChangeType type, string itemPath, string oldHash, string newHash)
            : base(syncSource, changeIn, type)
        {
            this.itemPath = itemPath;
            this.oldHash = oldHash;
            this.newHash = newHash;
        }

        public string ItemPath
        {
            get
            {
                return this.itemPath;
            }
        }

        public string OldItemHash
        {
            get
            {
                return this.oldHash;
            }
        }

        public string NewItemHash
        {
            get
            {
                return this.newHash;
            }
        }

        public override void Execute()
        {
            
        }
    }
}
