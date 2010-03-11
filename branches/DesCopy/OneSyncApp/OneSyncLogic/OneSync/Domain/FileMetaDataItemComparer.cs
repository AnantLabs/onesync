/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{   
    public class FileMetaDataItemComparer : IEqualityComparer<FileMetaDataItem>

    {
        public bool Equals(FileMetaDataItem item1, FileMetaDataItem item2)

        {
            return GetHashCode (item1) == GetHashCode(item2);
        }
        public int GetHashCode(FileMetaDataItem item)
        {
            return item.RelativePath.GetHashCode();
        }        
    }
}
