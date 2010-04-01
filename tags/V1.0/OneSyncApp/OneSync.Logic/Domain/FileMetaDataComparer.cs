using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class FileMetaDataComparer:ISyncProvider
    {
        List<FileMetaDataItem> leftOnly = new List<FileMetaDataItem>();
        List<FileMetaDataItem> rightOnly = new List<FileMetaDataItem>();
        List<FileMetaDataItem> bothModified = new List<FileMetaDataItem>();
        
        // MetaData of current folder
        protected FileMetaData left;

        // MetaData from storage (database, file)
        protected FileMetaData right;

        /// <summary>
        /// Provides the ...
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stored"></param>
        public FileMetaDataComparer(FileMetaData left, FileMetaData right)
        {
            this.left = left;
            this.right = right;
            Compare();
        }
                
        /// <summary>
        /// Metadata of current PC
        /// </summary>
        public FileMetaData Left
        {
            get
            {
                return this.left;
            }
        }
        
        /// <summary>
        /// Metadata of other PC
        /// </summary>
        public FileMetaData Right
        {
            get
            {
                return this.right;
            }
        }

        public IList<FileMetaDataItem> LeftOnly
        {
            get { return this.leftOnly; }
        }

        public IList<FileMetaDataItem> RightOnly
        {
            get { return this.rightOnly;  }
        }

        public IList<FileMetaDataItem> BothModified
        {
            get { return this.bothModified; }
        }

        private void Compare()
        {
            IEnumerable<FileMetaDataItem> leftOnly = from curr in left.MetaDataItems
                                                     where !right.MetaDataItems.Contains(curr, new FileMetaDataItemComparer())
                                                     select curr;

            this.leftOnly.AddRange(leftOnly);
            
            //Get deleted items 
            IEnumerable<FileMetaDataItem> rightOnly = from store in right.MetaDataItems
                                                      where !left.MetaDataItems.Contains(store, new FileMetaDataItemComparer())
                                                      select store;
            this.rightOnly.AddRange(rightOnly);


            // Since patch is already applied, newly updated files (according using hash)
            // on current PC (left) should be enumerated to generate the patch
            IEnumerable<FileMetaDataItem> bothModified = from _right in right.MetaDataItems
                                                         from _left in left.MetaDataItems
                                                         where _right.RelativePath.Equals(_left.RelativePath)
                                                         && !_right.HashCode.Equals(_left.HashCode)
                                                         select _left;

            this.bothModified.AddRange(bothModified);
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

             
            foreach (FileMetaDataItem item in LeftOnly )
            {
                CreateAction createAction = new CreateAction(0, item.SourceId, item.RelativePath, item.HashCode);
                actions.Add(createAction);
            }

            foreach (FileMetaDataItem item in RightOnly)
            {
                //the source id of this action must be source id of the folder where the item is deleted                 
                DeleteAction deleteAction = new DeleteAction(0, left.SourceId, item.RelativePath, item.HashCode);
                actions.Add(deleteAction);
            }

            foreach (FileMetaDataItem item in BothModified)

            {
                CreateAction createAction = new CreateAction(0, item.SourceId, item.RelativePath, item.HashCode);                   
                actions.Add(createAction);
            }

            return  actions;
        }
    }
}
