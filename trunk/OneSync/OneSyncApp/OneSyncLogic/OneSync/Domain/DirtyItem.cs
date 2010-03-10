using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class DirtyItem
    {
        private string pathInSyncSource = "";
        private string pathInMetaDataStorage = "";
        private string hash = "";
        public DirtyItem(string pathInSyncSource, string pathInMetaDataStorage, string hash)
        {
            this.pathInSyncSource = pathInSyncSource;
            this.pathInMetaDataStorage = pathInMetaDataStorage;
            this.hash = hash;
        }

        public string PathInSyncSource
        {
            get { return this.pathInSyncSource; }
        }

        public string PathInMetaDataStorage
        {
            get { return this.pathInMetaDataStorage; }
        }

        public string HashCode
        {
            get { return this.hash; }
        }
    }
}
