/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace OneSync.Synchronization
{
    public class FileMetaDataItem: IComparable<FileMetaDataItem>, IEqualityComparer<FileMetaDataItem>

    {
        private string source_id = "";
        private string fullName = "";
        private string relativePath = "";
        private string hashCode = "";
        private DateTime lastModifiedTime;
        private uint ntfs1 = 0;
        private uint ntfs2 = 0;        

        public FileMetaDataItem()
        {
            
        }
        
        public FileMetaDataItem(string source_id , string fullName, string relativePath , string hashCode, DateTime lastModifiedTime, uint ntfs1, uint ntfs2)
        {
            this.source_id = source_id;
            this.fullName = fullName;
            this.relativePath = relativePath;            
            this.hashCode = hashCode;
            this.lastModifiedTime = lastModifiedTime;
            this.ntfs1 = ntfs1;
            this.ntfs2 = ntfs2;            
        }

        public string SourceId
        {
            get
            {
                return this.source_id; 
            }
        }

        public string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        public string RelativePath
        {
            get
            {
                return this.relativePath;
            }
        }


        public string HashCode
        {
            get
            {
                return this.hashCode;
            }
        }

        public DateTime LastModifiedTime
        {
            get
            {
                return this.lastModifiedTime;
            }
        }

        public uint NTFS_ID1
        {
            get
            {
                return this.ntfs1;
            }
        }

        public uint NTFS_ID2
        {
            get
            {
                return this.ntfs2;
            }
        }
        public int CompareTo(FileMetaDataItem item)
        {            
            return this.RelativePath.CompareTo(item.RelativePath);
        }

        public int Compare(FileMetaDataItem item1, FileMetaDataItem item2)
        {
            return item1.RelativePath.CompareTo(item2.RelativePath);
        }

        public bool Equals(FileMetaDataItem item1, FileMetaDataItem item2)
        {
            return GetHashCode(item1) == GetHashCode(item2);
        }

        public int GetHashCode(FileMetaDataItem item)
        {            
            return item.RelativePath.GetHashCode();
        }
    }
}
