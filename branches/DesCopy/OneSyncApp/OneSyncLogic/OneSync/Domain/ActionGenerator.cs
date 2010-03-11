/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class ActionGenerator:ISyncProvider
    {
        // MetaData of current folder
        protected FileMetaData current;

        // MetaData from storage (database, file)
        protected FileMetaData stored;

        /// <summary>
        /// Provides the ...
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stored"></param>
        public ActionGenerator(FileMetaData current, FileMetaData stored)
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
        public  IList<SyncAction> Generate()
        {

            IList<SyncAction> actions = new List<SyncAction>();
            //Get newly created items by comparing relative paths

            IEnumerable<FileMetaDataItem> currentOnly = from curr in current.MetaDataItems
                                                  where !stored.MetaDataItems.Contains(curr, new FileMetaDataItemComparer())
                                                  select curr;          
            foreach (FileMetaDataItem item in currentOnly)
            {
                CreateAction createAction = new CreateAction(0, item.SourceId, item.RelativePath, item.HashCode);
                actions.Add(createAction);
            }

            //Get deleted items 

            IEnumerable<FileMetaDataItem> storedOnly = from store in stored.MetaDataItems
                                                  where !current.MetaDataItems.Contains(store, new FileMetaDataItemComparer())
                                                  select store;

            foreach (FileMetaDataItem item in storedOnly)
            {
                //the source id of this action must be source id of the folder where the item is deleted                 
                DeleteAction deleteAction = new DeleteAction (0, current.SourceId, item.RelativePath, item.HashCode);
                actions.Add (deleteAction);
            }


            // Since patch is already applied, newly updated files (according using hash)
            // on current PC (left) should be enumerated to generate the patch
            IEnumerable<FileMetaDataItem> bothModified =   from right in stored.MetaDataItems
                                                     from left in current.MetaDataItems
                                                     where right.RelativePath.Equals(left.RelativePath)
                                                     && !right.HashCode.Equals(left.HashCode)
                                                        select left;
            
            foreach (FileMetaDataItem item in bothModified)

            {
                CreateAction createAction = new CreateAction(0, item.SourceId, item.RelativePath, item.HashCode);                   
                actions.Add(createAction);
            }

            return  actions;
        }
    }
}
