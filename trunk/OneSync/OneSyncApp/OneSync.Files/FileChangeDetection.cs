/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace OneSync.Files
{ 
    /// <summary>
    /// FileChangeDetection monitor change of a given folder
    /// </summary>
    public class FileChangeDetection : FileSystemWatcher
    {
        private string folderPath = "";
        
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public FileChangeDetection()
            : base()
        {
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <param name="filter"></param>
        public FileChangeDetection(string path, string filter)
            : base(path, filter)
        {
            Initialize();
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the folder</param>
        public FileChangeDetection(string path)
            : base(path)
        {
            this.folderPath = path;
            Initialize();
        }


        private void Initialize()
        {
            //set file attributes to monitor
            this.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime |
                                NotifyFilters.DirectoryName | NotifyFilters.FileName  | 
                                NotifyFilters.Size;            
            this.EnableRaisingEvents = true;
            this.IncludeSubdirectories = true;
            //monitor all the file extensions and names 
            this.Filter = "*.*";

        }

       
        
         
        /// <summary>
        /// Set and get FolderPath
        /// </summary>
        public string FolderPath
        {
            set { this.folderPath = value; }
            get { return this.folderPath; }
        }

    }
}

