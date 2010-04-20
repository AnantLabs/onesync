//Coded by Nguyen van Thuat
/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OneSync.Synchronization;
using System.Xml;
using System.Xml.Serialization;

namespace OneSync.Synchronization
{
    
    public class FileMetaData
    {
        private string sourceId = "";
        private string sourcePath = "";
        protected IList<FileMetaDataItem> items = new List<FileMetaDataItem>();

        /// <summary>
        /// Collection of attributes
        /// </summary>
        #region list of column names in METADATA TABLE
        public const string METADATA_TABLE = "METADATA_TABLE";
        public const string SOURCE_ID = "SOURCE_ID";
        public const string FULL_NAME = "FULL_NAME";
        public const string RELATIVE_PATH = "RELATIVE_PATH";
        public const string HASH_CODE = "HASH_CODE";
        public const string LAST_MODIFIED_TIME = "LAST_MODIFIED_TIME";
        public const string NTFS_ID1 = "NTFS_ID1";
        public const string NTFS_ID2 = "NTFS_ID2";
        #endregion list of column names in METADATA TABLE
        
        
        public FileMetaData(string sourceId, string sourcePath):base()
        {
            this.sourceId = sourceId;
            this.sourcePath = sourcePath;
        }

        /// <summary>
        /// Absolute root directory folder to be synchronized.
        /// </summary>
        public string RootDir
        {
            get { return this.sourcePath; }
        }

        public string SourceId
        {
            get { return this.sourceId; }
        }

        public IList<FileMetaDataItem> MetaDataItems
        {
            get { return this.items; }
        }
    }
}
