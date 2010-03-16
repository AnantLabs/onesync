using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncPreviewResult
    {
        IList<SyncAction> conflictItems = new List<SyncAction>();
        IList<SyncAction> copyItems = new List<SyncAction>();
        IList<SyncAction> deleteItems = new List<SyncAction>();

        public SyncPreviewResult()
        {
        }

        public IList<SyncAction> ConflictItems
        {
            set { conflictItems = value; }
            get
            {
                return this.conflictItems;
            }
        }

        public IList<SyncAction> ItemsToDelete
        {
            set
            {
                deleteItems = value;
            }
            get
            {
                return this.deleteItems;
            }
        }

        public IList<SyncAction> ItemsToCopyOver
        {
            set
            {
                this.copyItems = value;
            }
            get
            {
                return this.copyItems;
            }
        }
    }
}
