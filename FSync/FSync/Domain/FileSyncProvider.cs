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

        // Intermediary storage information used for sync
        protected IntermediaryStorage iStorage;


        /// <summary>
        /// Provides the ...
        /// </summary>
        /// <param name="current">Metadata of current PC</param>
        /// <param name="stored">Metadata of other PC</param>
        public FileSyncProvider(FileMetaData current, FileMetaData stored, IntermediaryStorage iStorage)
        {
            this.current = current;
            this.stored = stored;
            this.iStorage = iStorage;
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
                CreateAction createAction = new CreateAction(current.RootDir, current.SourceId,
                                                             left.RelativePath, left.HashCode);
                actions.Add(createAction);
            }

            //Get deleted items 
            IEnumerable<FileMetaDataItem> rightOnly = from right in stored.MetaDataItems
                                                  where !current.MetaDataItems.Contains(right, new FileMetaDataItemComparer())
                                                  select right;
            foreach (FileMetaDataItem right in rightOnly)
            {
                DeleteAction deleteAction = new DeleteAction(current.SourceId, right.RelativePath, right.HashCode);
                actions.Add(deleteAction);
            }
            
            // Since patch is already applied, newly updated files (according using hash)
            // on current PC (left) should be enumerated to generate the patch
            IEnumerable<FileMetaDataItem> bothModified =   from right in stored.MetaDataItems
                                                     from left in current.MetaDataItems
                                                     where right.RelativePath.Equals(left.RelativePath)
                                                     && !right.HashCode.Equals(left.HashCode)
                                                        select left;
            
            foreach (FileMetaDataItem left in bothModified)
            {

                CreateAction a = new CreateAction(current.RootDir, current.SourceId, left.RelativePath,
                                                  left.HashCode);
                actions.Add(a);
            }

            return  actions;
        }
    }
}
