using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    
    public class Patch
    {

        private SyncSource syncSource = null;

        // Refers to absolute folder path of where metadata is stored.
        private IntermediaryStorage metaDataSource = null;

        /// <summary>
        /// List of dirty items
        /// </summary>
        private IList<DirtyItem> dirtyItems = new List<DirtyItem>();
        private IList<SyncAction> actions = null;


        public Patch(SyncSource syncSource, IntermediaryStorage metaDataSource, IList<SyncAction> actions)
        {
            this.syncSource = syncSource;
            this.metaDataSource = metaDataSource;
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
            string rootDir = metaDataSource.DirtyFolderPath;


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
                action.Execute();
            }
        }

        private void Process(IList<SyncAction> actions)
        {
            DirtyItem dirtyItem = null;
            foreach (SyncAction action in actions)
            {
                switch (action.ChangeType)
                {
                    case ChangeType.MODIFIED:
                        ModifyAction modifiedAction = (ModifyAction)action;
                        dirtyItem = new DirtyItem(syncSource + modifiedAction.RelativeFilePath,
                           metaDataSource.Path + modifiedAction.RelativeFilePath, modifiedAction.NewItemHash);
                        dirtyItems.Add(dirtyItem);
                        break;
                    case ChangeType.NEWLY_CREATED:
                        CreateAction createAction = (CreateAction)action;
                        dirtyItem = new DirtyItem(syncSource + createAction.RelativeFilePath, metaDataSource.Path + createAction.RelativeFilePath, createAction.FileHash);
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
            get { return this.metaDataSource; }
        }

        public SyncSource SyncSource
        {
            get { return this.syncSource; }
        }
    
    }
}
