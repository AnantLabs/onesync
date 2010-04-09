using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO ;
namespace OneSync.Synchronization
{
    class FolderMetadataItemComparer : IEqualityComparer<FolderMetadataItem>, IComparer<FolderMetadataItem>
    {

        #region IEqualityComparer<FolderMetadataItem> Members

        public bool Equals(FolderMetadataItem x, FolderMetadataItem y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(FolderMetadataItem obj)
        {
            return obj.RelativePath.GetHashCode();
        }

        #endregion

        #region IComparer<FolderMetadataItem> Members

        public int Compare(FolderMetadataItem x, FolderMetadataItem y)
        {
            return x.RelativePath.CompareTo(y.RelativePath);
        }        
        #endregion
    }
}
