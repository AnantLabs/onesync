//Coded by Nguyen van Thuat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class FolderMetadataComparer
    {
        List<FolderMetadataItem> leftOnly = new List<FolderMetadataItem>();
        List<FolderMetadataItem> rightOnly = new List<FolderMetadataItem>();
        List<FolderMetadataItem> both = new List<FolderMetadataItem>();

        // MetaData of current folder
        protected FolderMetadata left;

        // MetaData from storage (database, file)
        protected FolderMetadata right;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FolderMetadataComparer() { }

        /// <summary>
        /// Provides the ...
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stored"></param>
        public FolderMetadataComparer(FolderMetadata left, FolderMetadata right)
        {
            this.left = left;
            this.right = right;
            Compare(left.FolderMetadataItems, right.FolderMetadataItems);
        }

        public FolderMetadataComparer(IList<FolderMetadataItem> leftItems, IList<FolderMetadataItem> rightItems)
        {
            Compare(leftItems, rightItems);
        }
                
        /// <summary>
        /// Metadata of current PC
        /// </summary>
        public FolderMetadata Left
        {
            get
            {
                return this.left;
            }
        }
        
        /// <summary>
        /// Metadata of other PC
        /// </summary>
        public FolderMetadata Right
        {
            get
            {
                return this.right;
            }
        }

        public IList<FolderMetadataItem> LeftOnly
        {
            get { return this.leftOnly; }
        }

        public IList<FolderMetadataItem> Both
        {
            get { return this.both; }
        }

        public IList<FolderMetadataItem> RightOnly
        {
            get { return this.rightOnly;  }
        }

        /// <summary>
        /// Logic to compare 2 list of folder metadata items
        /// </summary>
        /// <param name="leftItems"></param>
        /// <param name="rightItems"></param>
        private void Compare(IList<FolderMetadataItem> leftItems, IList<FolderMetadataItem> rightItems )
        {
            //sort item list by relative path before proceed to linq search
            leftItems.ToList().Sort(new FolderMetadataItemComparer());
            rightItems.ToList().Sort(new FolderMetadataItemComparer());

            IEnumerable<FolderMetadataItem> leftOnly = from curr in leftItems
                                                     where !rightItems.Contains(curr, new FolderMetadataItemComparer())
                                                     select curr;
            this.leftOnly.AddRange(leftOnly);

            IEnumerable<FolderMetadataItem> rightOnly = from store in rightItems
                                                        where !leftItems.Contains(store, new FolderMetadataItemComparer())
                                                        select store; 
            this.rightOnly.AddRange(rightOnly);
            
            IEnumerable<FolderMetadataItem> both = from _right in rightItems
                                                   from _left in leftItems
                                                   where _right.RelativePath.Equals(_left.RelativePath)
                                                   select _right;
            this.both.AddRange(both);           
        }

       
    }
}
