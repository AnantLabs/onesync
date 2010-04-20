//Coded by Nguyen van Thuat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneSync.Files;

namespace OneSync.Synchronization
{   
    public class FolderMetadataItem
    {
        public FolderMetadataItem (string sourceId, string absolutePath, string baseFolder, int isEmpty)
        {
            SourceId = sourceId;
            AbsolutePath = absolutePath;
            RelativePath = FileUtils.GetRelativePath (baseFolder, absolutePath);
            IsEmpty = isEmpty;
        }

        public FolderMetadataItem(string sourceId, string relativePath, int isEmpty)
        {
            SourceId = sourceId;
            RelativePath = relativePath;
            IsEmpty = isEmpty;
        }


        public string SourceId
        {set; get;}

        public string RelativePath
        { set;get;}

        public string AbsolutePath
        { set; get; }

        public int IsEmpty
        {
            set; get;
        }
    }
}
