using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncPreviewResult
    {
        private IList<SyncAction> conflictItems = new List<SyncAction>();
        private IList<SyncAction> copyItems = new List<SyncAction>();
        private IList<SyncAction> deleteItems = new List<SyncAction>();
        private IList<SyncAction> actions = null;
        private FileMetaData currentMetadata = null, oldCurrentMetadata, oldOtherMetadata;       

        public SyncPreviewResult(IList<SyncAction> actions, FileMetaData currentMetadata,
            FileMetaData oldCurrentMetadata, FileMetaData oldOtherMetadata)
        {
            this.actions = actions;
            this.currentMetadata = currentMetadata;
            this.oldCurrentMetadata = oldCurrentMetadata;
            this.oldOtherMetadata = oldOtherMetadata;
            RunPreview();
        }

        private void RunPreview()
        {
            //By comparing current metadata and the old metadata from previous sync of a sync folder
            //we could know the dirty items of the folder.
            FileMetaDataComparer comparerCurrent = new FileMetaDataComparer(currentMetadata, oldCurrentMetadata);
            List<FileMetaDataItem> dirtyItemsInCurrent = new List<FileMetaDataItem>();
            dirtyItemsInCurrent.AddRange(comparerCurrent.LeftOnly);
            dirtyItemsInCurrent.AddRange(comparerCurrent.BothModified);

            //Compare the actions and dirty items in current folder. Collision is detected when 
            //2 items in 2 collection have same relative path but different hashes. 
            IEnumerable<SyncAction> conflictItems = from action in actions
                                                    from dirtyInCurrent in dirtyItemsInCurrent
                                                    where action.RelativeFilePath.Equals(dirtyInCurrent.RelativePath)
                                                    && action.ChangeType == ChangeType.NEWLY_CREATED
                                                    && !action.FileHash.Equals(dirtyInCurrent.HashCode)
                                                    select action;

            foreach (SyncAction action in conflictItems)
            {
                action.ConflictResolution = ConflictResolution.DUPLICATE_RENAME;
            }

            IEnumerable<SyncAction> itemsToDelete = from action in actions
                                                    where action.ChangeType == ChangeType.DELETED
                                                    select action;


            IEnumerable<SyncAction> itemsToCopyOver = from action in actions
                                                      where action.ChangeType == ChangeType.NEWLY_CREATED
                                                      && !conflictItems.Contains(action)
                                                      select action;


            ConflictItems = conflictItems.ToList();
            ItemsToCopyOver = itemsToCopyOver.ToList();
            itemsToDelete = itemsToDelete.ToList();
        }

        public IList<SyncAction> GetAcionList()
        {
            return this.actions;
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
