using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class ChangeItem
    {
        private IMetaDataItem oldItem;
        private IMetaDataItem newItem;
        public ChangeItem(IMetaDataItem oldItem, IMetaDataItem newItem)
        {
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        public IMetaDataItem OldItem
        {
            get { return this.oldItem; }
        }

        public IMetaDataItem NewItem
        {
            get { return this.newItem;  }
        }
    }
}
