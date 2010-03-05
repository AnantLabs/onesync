using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class ChangeItem
    {
        private FileMetaDataItem oldItem;
        private FileMetaDataItem newItem;
        public ChangeItem(FileMetaDataItem oldItem, FileMetaDataItem newItem)
        {
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        public FileMetaDataItem OldItem
        {
            get { return this.oldItem; }
        }

        public FileMetaDataItem NewItem
        {
            get { return this.newItem;  }
        }
    }
}
