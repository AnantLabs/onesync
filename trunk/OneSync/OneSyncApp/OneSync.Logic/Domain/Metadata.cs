using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class Metadata
    {
        private FileMetaData fileMetadata = null;
        private FolderMetadata folderMetadata = null;

        public Metadata()
        {
        }

        public Metadata(FileMetaData fileMetadata, FolderMetadata folderMetadata)
        {
            this.FileMetadata = fileMetadata;
            this.FolderMetadata = folderMetadata;
        }

        public FileMetaData FileMetadata
        {
            set { this.fileMetadata = value; }
            get { return this.fileMetadata; }
        }

        public FolderMetadata FolderMetadata
        {
            set { this.folderMetadata = value; }
            get { return this.folderMetadata; }
        }
    }
}
