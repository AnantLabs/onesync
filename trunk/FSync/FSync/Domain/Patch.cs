using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    
    public class Patch
    {
        private SyncSource syncSource = null;
        private MetaDataSource metaDataSource = null;       

        /// <summary>
        /// List of dirty items
        /// </summary>
        private IList<DirtyItem> dirtyItems = new List<DirtyItem>();
        private IList<SyncAction> actions = null;
        public Patch(SyncSource syncSource, MetaDataSource metaDataSource, IList<SyncAction> actions)
        {
            this.syncSource = syncSource;
            this.metaDataSource = metaDataSource;
            this.actions = actions;
            Process(actions);
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
                        dirtyItem = new DirtyItem(syncSource + modifiedAction.ItemPath,
                           metaDataSource.Path + modifiedAction.ItemPath, modifiedAction.NewItemHash);
                        dirtyItems.Add(dirtyItem);
                        break;
                    case ChangeType.NEWLY_CREATED:
                        CreateAction createAction = (CreateAction)action;
                        dirtyItem = new DirtyItem(syncSource + createAction.ItemPath, metaDataSource.Path + createAction.ItemPath, createAction.ItemHash);
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

        public MetaDataSource MetaDataSource
        {
            get { return this.metaDataSource; }
        }

        public SyncSource SyncSource
        {
            get { return this.syncSource; }
        }
    
    }
}
