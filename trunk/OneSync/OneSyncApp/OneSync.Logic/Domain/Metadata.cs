//Coded by Nguyen van Thuat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class Metadata
    {
        private Metadata() { }

        public Metadata(FileMetaData fileMetadata, FolderMetadata folderMetadata)
        {
            this.FileMetadata = fileMetadata;
            this.FolderMetadata = folderMetadata;
        }

        public FileMetaData FileMetadata { set; get; }
        public FolderMetadata FolderMetadata { get; set; }
    }
}
