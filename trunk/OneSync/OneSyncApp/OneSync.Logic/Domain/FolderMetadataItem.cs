using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneSync.Files;

namespace OneSync.Synchronization
{   
    public class FolderMetadataItem
    {
        public FolderMetadataItem (string sourceId, string absolutePath, string baseFolder)
        {
            SourceId = sourceId;
            AbsolutePath = absolutePath;
            RelativePath = FileUtils.GetRelativePath (baseFolder, absolutePath);
        }

        public FolderMetadataItem(string sourceId, string relativePath)
        {
            SourceId = sourceId;
            RelativePath = relativePath;
        }

        public string SourceId
        {set; get;}

        public string RelativePath
        { set;get;}

        public string AbsolutePath
        { set; get; }
    }
}
