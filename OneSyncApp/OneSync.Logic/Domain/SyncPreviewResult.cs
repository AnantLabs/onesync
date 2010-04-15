using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncPreviewResult
    {
        private IList<SyncAction> _conflictItems = new List<SyncAction>();
        private IList<SyncAction> _copyItems = new List<SyncAction>();
        private IList<SyncAction> _deleteItems = new List<SyncAction>();
        private IList<SyncAction> _renameItems = new List<SyncAction>();
        private IList<SyncAction> _actions = null;

        public SyncPreviewResult(IList<SyncAction> actions, FileMetaData currentMetadata,
            FileMetaData oldCurrentMetadata, FileMetaData oldOtherMetadata)
        {
            this._actions = actions;
            GeneratePreview(currentMetadata, oldCurrentMetadata);
        }

        private void GeneratePreview(FileMetaData currentMetadata, FileMetaData oldCurrentMetadata)
        {
            //By comparing current metadata and the old metadata from previous sync of a sync folder
            //we could know the dirty items of the folder.
            var comparerCurrent = new FileMetaDataComparer(currentMetadata, oldCurrentMetadata);
            var dirtyItemsInCurrent = new List<FileMetaDataItem>();
            dirtyItemsInCurrent.AddRange(comparerCurrent.LeftOnly);
            dirtyItemsInCurrent.AddRange(comparerCurrent.BothModified);

            //Compare the _actions and dirty items in current folder. Collision is detected when 
            //2 items in 2 collection have same relative path but different hashes. 
            IEnumerable<SyncAction> conflictItems = from action in _actions
                                                    join dirtyInCurrent in dirtyItemsInCurrent on
                                                        action.RelativeFilePath equals dirtyInCurrent.RelativePath
                                                    where !action.FileHash.Equals(dirtyInCurrent.HashCode)
                                                    select action;

            foreach (SyncAction action in conflictItems)
                action.ConflictResolution = ConflictResolution.DUPLICATE_RENAME;

            IEnumerable<SyncAction> itemsToDelete = from action in _actions
                                                    where action.ChangeType == ChangeType.DELETED
                                                    select action;

            IEnumerable<SyncAction> itemsToCopyOver = from action in _actions
                                                      where action.ChangeType == ChangeType.NEWLY_CREATED
                                                      && !conflictItems.Contains(action)
                                                      select action;

            IEnumerable<SyncAction> itemsToRename = from action in _actions
                                                    where action.ChangeType == ChangeType.RENAMED
                                                    && !conflictItems.Contains(action)
                                                    select action;

            ConflictItems = conflictItems.ToList();
            ItemsToCopyOver = itemsToCopyOver.ToList();
            ItemsToDelete = itemsToDelete.ToList();
            ItemsToRename = itemsToRename.ToList();
        }

        public IList<SyncAction> GetAllActions()
        {
            return this._actions;
        }

        public IList<SyncAction> ConflictItems
        {
            set { _conflictItems = value; }
            get { return this._conflictItems; }
        }

        public IList<SyncAction> ItemsToDelete
        {
            set { _deleteItems = value; }
            get { return this._deleteItems; }
        }

        public IList<SyncAction> ItemsToCopyOver
        {
            set { this._copyItems = value; }
            get { return this._copyItems; }
        }

        public IList<SyncAction> ItemsToRename
        {
            set { _renameItems = value; }
            get { return this._renameItems; }
        }

        public int ItemsCount
        {
            get
            {
                return _copyItems.Count + _conflictItems.Count +
                       _deleteItems.Count + _renameItems.Count;
            }
        }
    }
}
