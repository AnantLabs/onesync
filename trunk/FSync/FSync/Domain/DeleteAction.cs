using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class DeleteAction:SyncAction
    {
        private string path = "";
        private string hash = "";
        
        public DeleteAction(string syncSource, string changeIn, ChangeType action, string path, string hash)
            : base(syncSource, changeIn, action)
        {            
            this.path = path;
            this.hash = hash;
        }

        public string ItemPath
        {
            get
            {
                return this.path;
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
            string filePath = syncSource + path;
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
