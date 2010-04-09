using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class FolderMetadata
    {      
        public IList<FolderMetadataItem> folderItems = new List<FolderMetadataItem>();
        public FolderMetadata (string sourceId, string baseFolder)
        {
            SourceId = sourceId;
            BaseFolder = baseFolder;
        }
        public string SourceId
        { set;get; }

        public string BaseFolder
        { set;  get; }
        public IList<FolderMetadataItem> FolderMetadataItems
        {
            set { folderItems = value; }
            get { return this.folderItems; }
        }
    }
}
