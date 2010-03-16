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
        /// <summary>
        /// Collection of attributes
        /// </summary>

        protected IList<FileMetaDataItem> items = new List<FileMetaDataItem>();              
        
        
        public FileMetaData(string sourceId, string sourcePath):base()
        {
            this.sourceId = sourceId;
            this.sourcePath = sourcePath;
        }

        public FileMetaData(IList<FileMetaDataItem> items)
        {
            this.items = items;
        }

        /// <summary>
        /// Absolute root directory folder to be synchronized.
        /// </summary>
        public string RootDir
        {
            get
            {
                return this.sourcePath;
            }
        }
        public string SourceId
        {
            set
            {
                this.sourceId = value;
            }
            get
            {
                return this.sourceId;
            }
        }

        public IList<FileMetaDataItem> MetaDataItems
        {
            set
            {
                items = value;
            }
            get
            {
                return this.items;
            }
        }
    }
}
