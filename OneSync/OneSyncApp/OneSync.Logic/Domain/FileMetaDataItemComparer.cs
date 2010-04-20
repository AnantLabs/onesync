//Coded by Nguyen van Thuat
/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class FileMetaDataItemComparer : IEqualityComparer<FileMetaDataItem>, IComparer<FileMetaDataItem>
    {
        public bool Equals(FileMetaDataItem item1, FileMetaDataItem item2)
        {
            return GetHashCode(item1) == GetHashCode(item2);
        }
        public int GetHashCode(FileMetaDataItem item)
        {
            return item.RelativePath.GetHashCode();
        }

        #region IComparer<FileMetaDataItem> Members

        public int Compare(FileMetaDataItem x, FileMetaDataItem y)
        {
            return (x.RelativePath.Equals(y.RelativePath)) ?
                (x.HashCode.CompareTo(y.HashCode)) : x.RelativePath.CompareTo(y.RelativePath);
        }

        #endregion
    }
}
