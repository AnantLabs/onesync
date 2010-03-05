using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class FileMetaDataItemComparer:IMetaDataItemComparer
    {
        public bool Equals(IMetaDataItem item1, IMetaDataItem item2)
        {
            return GetHashCode (item1) == GetHashCode(item2);
        }
        public int GetHashCode(IMetaDataItem item)
        {
            return ((FileMetaDataItem) item).RelativePath.GetHashCode();
        }
    }
}
