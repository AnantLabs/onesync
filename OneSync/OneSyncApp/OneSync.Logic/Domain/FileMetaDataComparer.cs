//Coded by Nguyen van Thuat
using System.Collections.Generic;
using System.Linq;

namespace OneSync.Synchronization
{
    public class FileMetaDataComparer
    {
        
        public FileMetaDataComparer(FileMetaData left, FileMetaData right)
        {
            Compare(left, right);
        }

        public List<FileMetaDataItem> LeftOnly { get; set; }

        public List<FileMetaDataItem> RightOnly { get; set; }

        public List<FileMetaDataItem> BothModified { get; set; }

        private void Compare(FileMetaData left, FileMetaData right)
        {
            FileMetaDataItemComparer comparer = new FileMetaDataItemComparer();

            var rightOnly = from i in right.MetaDataItems
                            where !left.MetaDataItems.Contains(i, comparer)
                            select i;


            var leftOnly = from i in left.MetaDataItems
                           where !right.MetaDataItems.Contains(i, comparer)
                           select i;
            
            var both = from l in left.MetaDataItems
                       join r in right.MetaDataItems
                       on l.RelativePath equals r.RelativePath
                       where l.HashCode != r.HashCode
                       select l;

            this.LeftOnly = new List<FileMetaDataItem>(leftOnly);
            this.RightOnly = new List<FileMetaDataItem>(rightOnly);
            this.BothModified = new List<FileMetaDataItem>(both);
        }
    }
}
