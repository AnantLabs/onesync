using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class FileSyncProvider:ISyncProvider
    {
        // MetaData of current PC
        protected FileMetaData current;

        // MetaData from other PC
        protected FileMetaData stored;      

        /// <summary>
        /// Provides the ...
        /// </summary>
        /// <param name="current">Metadata of current PC</param>
        /// <param name="stored">Metadata of other PC</param>
        public FileSyncProvider(FileMetaData current, FileMetaData stored)
        {
            this.current = current;
            this.stored = stored;
        }

        /// <summary>
        /// Metadata of current PC
        /// </summary>
        public FileMetaData CurrentMetaData
        {
            get
            {
                return this.current;
            }
        }


        /// <summary>
        /// Metadata of other PC
        /// </summary>
        public FileMetaData StoredMetaData
        {
            get
            {
                return this.stored;
            }
        }
        
        /// <summary>
        /// Generates list of sync actions that will synchronize current PC
        /// and other PC based on the metadata.
        /// </summary>
        /// <returns>List of sync actions</returns>
        public  IList<SyncAction> GenerateActions()
        {
            IList<SyncAction> actions = new List<SyncAction>();

            // Note:
            // Left refers to current PC
            // Right refers to other PC
            
            //Get newly created items by comparing relative paths
            IEnumerable<FileMetaDataItem> leftOnly = from left in current.MetaDataItems
                                                  where !stored.MetaDataItems.Contains(left, new FileMetaDataItemComparer())
                                                  select left;


            foreach (FileMetaDataItem left in leftOnly)
            {
                CreateAction createAction = new CreateAction(
                    current.SourcePath,
                    current.SourceId, left.RelativePath, left.HashCode);
                actions.Add(createAction);
            }

            //Get deleted items 
            IEnumerable<FileMetaDataItem> rightOnly = from right in stored.MetaDataItems
                                                  where !current.MetaDataItems.Contains(right, new FileMetaDataItemComparer())
                                                  select right;
            foreach (FileMetaDataItem right in rightOnly)
            {
                DeleteAction deleteAction = new DeleteAction(
                    current.SourcePath,
                    current.SourceId, right.RelativePath, right.HashCode);
                actions.Add(deleteAction);
            }
            
            //get the items from 2 metadata with same relative paths but different hashes.
            IEnumerable<ChangeItem> bothModified =   from right in stored.MetaDataItems
                                                     from left in current.MetaDataItems
                                                     where ((FileMetaDataItem)right).RelativePath.Equals(((FileMetaDataItem)left).RelativePath)
                                                     && !((FileMetaDataItem)right).HashCode.Equals(((FileMetaDataItem)left).HashCode)
                                                        select new ChangeItem (right, left);
            foreach (ChangeItem item in bothModified) 
            {
                FileMetaDataItem oldItem =(FileMetaDataItem) item.OldItem;
                FileMetaDataItem newItem =(FileMetaDataItem) item.NewItem;
                ModifyAction modifyAction = new ModifyAction(current.SourcePath, current.SourceId, ChangeType.MODIFIED,
                    oldItem.RelativePath, oldItem.HashCode, newItem.HashCode); 
                actions.Add(modifyAction);
            }

            return  actions;
        }
    }
}
