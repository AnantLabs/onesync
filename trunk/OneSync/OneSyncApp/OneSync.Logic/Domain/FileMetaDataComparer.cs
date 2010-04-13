using System.Collections.Generic;
using System.Linq;

namespace OneSync.Synchronization
{
    public class FileMetaDataComparer
    {
        private List<FileMetaDataItem> leftOnly = new List<FileMetaDataItem>();
        private List<FileMetaDataItem> rightOnly = new List<FileMetaDataItem>();
        private List<FileMetaDataItem> bothModified = new List<FileMetaDataItem>();
        /// <summary>
        /// Provides the ...
        /// </summary>
        public FileMetaDataComparer(FileMetaData left, FileMetaData right)
        {
            Compare(left, right);
        }

        public List<FileMetaDataItem> LeftOnly
        {
            set { leftOnly = value; }
            get { return leftOnly; }
        }

        public List<FileMetaDataItem> RightOnly
        {
            set { rightOnly = value; }
            get { return rightOnly; }
        }

        public List<FileMetaDataItem> BothModified
        {
            set { bothModified = value; }
            get { return bothModified; }
        }

        private void Compare(FileMetaData left, FileMetaData right)
        {
            //Get newly created items by comparing relative paths
            //newOnly is metadata item in current metadata but not in old one
            IEnumerable<FileMetaDataItem> rightMdOnly =
                right.MetaDataItems.Where(
                    value => !left.MetaDataItems.Contains(value, new FileMetaDataItemComparer()));

            //Get deleted items           
            IEnumerable<FileMetaDataItem> leftMdOnly =
                left.MetaDataItems.Where(
                    old => !right.MetaDataItems.Contains(old, new FileMetaDataItemComparer()));

            //get the items from 2 metadata with same relative paths but different hashes.
            IEnumerable<FileMetaDataItem> bothAreModified = from @new in left.MetaDataItems
                                                         join @old in right.MetaDataItems on @new.RelativePath
                                                             equals @old.RelativePath
                                                         where !(@new.HashCode.Equals(@new.HashCode))
                                                         select @new;
            LeftOnly.Clear();
            LeftOnly.AddRange(leftMdOnly);

            RightOnly.Clear();
            RightOnly.AddRange(rightMdOnly);

            BothModified.Clear();
            BothModified.AddRange(bothAreModified);
        }

      

       
    }
}
