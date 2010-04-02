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
        private IList<SyncAction> renameItems = new List<SyncAction>();
        private IList<SyncAction> actions = null;
        private FileMetaData currentMetadata = null, oldCurrentMetadata, oldOtherMetadata;       

        public SyncPreviewResult(IList<SyncAction> actions, FileMetaData currentMetadata,
            FileMetaData oldCurrentMetadata, FileMetaData oldOtherMetadata)
        {
            this.actions = actions;
            this.currentMetadata = currentMetadata;
            this.oldCurrentMetadata = oldCurrentMetadata;
            this.oldOtherMetadata = oldOtherMetadata;
            GeneratePreview();
        }

        private void GeneratePreview()
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
            ItemsToDelete = itemsToDelete.ToList();

            // Detect rename
            IEnumerable<SyncAction> itemsToRename = GenerateRenameActions(ConflictItems, ItemsToCopyOver, ItemsToDelete);
            ((List<SyncAction>)renameItems).AddRange(itemsToRename);
            ((List<SyncAction>)this.actions).AddRange(itemsToRename);
        }

        # region Rename Detection

        private IEnumerable<SyncAction> GenerateRenameActions(IList<SyncAction> conflictItems,
                                                              IList<SyncAction> itemsToCopyOver,
                                                              IList<SyncAction> itemsToDelete)
        {
            List<SyncAction> renameActions = new List<SyncAction>();

            ProcessRenameActions(conflictItems, itemsToDelete, renameActions);
            ProcessRenameActions(itemsToCopyOver, itemsToDelete, renameActions);

            return renameActions;
        }

        private void ProcessRenameActions(IList<SyncAction> createActions, IList<SyncAction> itemsToDelete, IList<SyncAction> renameActions)
        {
            // To keep track of actions that is now converted to rename operations;
            List<SyncAction> originalActionsToRemove = new List<SyncAction>();
            List<SyncAction> deleteActionsToRemove = new List<SyncAction>();

            // Create a list to store conflict renames
            List<SyncAction> tempList = new List<SyncAction>();

            // Get all conflict rename actions
            // i.e. There exist a dirty file having same name as newly renamed file
            foreach (SyncAction delAction in deleteItems)
            {

                foreach (SyncAction a in createActions)
                {
                    if (delAction.FileHash == a.FileHash)
                    {
                        deleteActionsToRemove.Add(delAction);
                        originalActionsToRemove.Add(a);
                        RenameAction renameAction = new RenameAction(0, a.SourceID,
                                        a.RelativeFilePath, delAction.RelativeFilePath, a.FileHash);
                        renameAction.ConflictResolution = a.ConflictResolution;
                        renameAction.OriginalCreateAction = a;
                        renameAction.OriginalDeleteAction = delAction;

                        // Handle conflict actions separately
                        if (a.ConflictResolution != ConflictResolution.NONE)
                            tempList.Add(renameAction);
                        else
                            renameActions.Add(renameAction);
                        break;
                    }
                }
            }

            // Remove actions converted to rename actions
            foreach (SyncAction a in originalActionsToRemove)
            {
                this.actions.Remove(a);  /* removed from global list of actions */
                createActions.Remove(a); /* removed from filtered list of actions */
            }

            foreach (SyncAction a in deleteActionsToRemove)
            {
                this.actions.Remove(a);  /* removed from global list of actions */
                itemsToDelete.Remove(a); /* removed from filtered list of actions */
            }

            // Add conflicted rename actions back to conflict action list
            foreach (SyncAction conflict in tempList)
            {
                this.actions.Add(conflict); /* add to global list of actions */
                createActions.Add(conflict);   /* add to filtered list of actions */
            }
        }

        #endregion

        public IList<SyncAction> GetAllActions()
        {
            return this.actions;
        }

        public IList<SyncAction> ConflictItems
        {
            set { conflictItems = value; }
            get { return this.conflictItems; }
        }

        public IList<SyncAction> ItemsToDelete
        {
            set { deleteItems = value; }
            get { return this.deleteItems; }
        }

        public IList<SyncAction> ItemsToCopyOver
        {
            set { this.copyItems = value; }
            get { return this.copyItems; }
        }

        public IList<SyncAction> ItemsToRename
        {
            set { renameItems = value; }
            get { return this.renameItems; }
        }
    }
}
