using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    
    public class Patch
    {

        private SyncSource targetSyncSource = null;

        // Refers to absolute folder path of where metadata is stored.
        private IntermediaryStorage iStorage = null;

        /// <summary>
        /// List of dirty items
        /// </summary>
        private IList<DirtyItem> dirtyItems = new List<DirtyItem>();
        private IList<SyncAction> actions = null;


        /// <summary>
        /// Creates a new Patch.
        /// </summary>
        /// <param name="syncSource">SyncSource of other PC where patch is to be applied.</param>
        /// <param name="iStorage">Information about intermediary storage</param>
        /// <param name="actions">Actions to be contained in this patch.</param>
        public Patch(SyncSource targetSyncSource, IntermediaryStorage iStorage, IList<SyncAction> actions)
        {
            this.targetSyncSource = targetSyncSource;
            this.iStorage = iStorage;
            this.actions = actions;
            Process(actions);
        }

        
        /// <summary>
        /// Verify whether all the files contained in this patch are valid by
        /// checking whether it exists and optionally its file hash is the same.
        /// </summary>
        /// <param name="checkHash">Computes hash of files and verify it.</param>
        /// <returns>True if all the files are valid.</returns>
        public bool Verify(bool checkHash)
        {
            // Root directory where all dirty files are stored
            string rootDir = iStorage.DirtyFolderPath;


            foreach (SyncAction a in actions)
            {
                // Skip Delete Action as there is no need to check for
                // if file exists, neither is there a need to compute hash.
                if (a is DeleteAction) continue;

                string filePath = Path.Combine(rootDir, a.RelativeFilePath);

                bool result = File.Exists(filePath);

                // Check whether file hash tally
                if (checkHash)
                    result &= Files.FileUtils.GetFileHash(filePath).Equals(a.FileHash);

                if (result == false) return false;
            }
            return true;
        }

        
        public void Apply()
        {
            foreach (SyncAction action in actions)
            {
                if (action is CreateAction)
                {
                    string srcPath = Path.Combine(iStorage.DirtyFolderPath, action.RelativeFilePath);
                    string destPath = Path.Combine(targetSyncSource.Path, action.RelativeFilePath);

                    // TODO: check if destPath already exists, if it is, check if it's dirty...
                    File.Copy(srcPath, destPath);
                }
                else if (action is DeleteAction)
                {
                    string filePath = Path.Combine(targetSyncSource.Path, action.RelativeFilePath);

                    if (File.Exists(filePath)) File.Delete(filePath);
                }
                else if (action is RenameAction)
                {
                    RenameAction a = (RenameAction)action;

                    string oldPath = Path.Combine(targetSyncSource.Path, a.PreviousRelativeFilePath);
                    string newPath = Path.Combine(targetSyncSource.Path, a.RelativeFilePath);

                    File.Move(oldPath, newPath);
                }
                else
                {
                    //throw action unhandled exception?
                }
            }

            // TODO:
            // Update metadata after patch is applied
            // Delete all dirty files
        }

        private void Process(IList<SyncAction> actions)
        {
            DirtyItem dirtyItem = null;
            foreach (SyncAction action in actions)
            {
                switch (action.ChangeType)
                {
                    /*
                    case ChangeType.MODIFIED:
                        ModifyAction modifiedAction = (ModifyAction)action;
                        dirtyItem = new DirtyItem(syncSource + modifiedAction.RelativeFilePath,
                           metaDataSource.Path + modifiedAction.RelativeFilePath, modifiedAction.NewItemHash);
                        dirtyItems.Add(dirtyItem);
                        break;
                     */
                    case ChangeType.NEWLY_CREATED:
                        CreateAction createAction = (CreateAction)action;
                        dirtyItem = new DirtyItem(targetSyncSource + createAction.RelativeFilePath, iStorage.Path + createAction.RelativeFilePath, createAction.FileHash);
                        break;
                }
            }
        }

        public IList<SyncAction> Actions
        {
            get { return this.actions;  }
        }

        public IList<DirtyItem> DirtyItems
        {
            get { return this.dirtyItems; }
        }

        public IntermediaryStorage MetaDataSource
        {
            get { return this.iStorage; }
        }

        /// <summary>
        /// Gets information regarding the folder (on other PC)
        /// where this patch is to be applied.
        /// </summary>
        public SyncSource TargetSyncSource
        {
            get { return this.targetSyncSource; }
        }
    
    }
}
