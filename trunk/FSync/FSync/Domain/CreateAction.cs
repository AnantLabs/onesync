using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class CreateAction:SyncAction
    {
        private string itemPath = "";
        private string itemHash = "";
        public CreateAction(string sourcePath, string changeIn, ChangeType action, string itemPath, string itemHash)
            : base(sourcePath, changeIn, action)
        {
            this.itemPath = itemPath;
            this.itemHash = itemHash;
        }

        public string ItemPath
        {
            get
            {
                return this.itemPath;
            }
        }

        public string ItemHash
        {
            get
            {
                return this.itemHash;
            }
        }

        public override void Execute()
        {
            
        }
    }
}
