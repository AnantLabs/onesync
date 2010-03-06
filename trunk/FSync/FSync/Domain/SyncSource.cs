using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    
    /// <summary>
    /// Represents a folder on a PC to be synchronized.
    /// </summary>
    public class SyncSource
    {        
        private string gid = "";
        private string path = "";

        /// <summary>
        /// column names for sync source table
        /// </summary>
        public const string PATH = "PATH";
        public const string GID = "ID";
        public const string DATASOURCE_INFO_TABLE = "DATASOURCE_INFO";


        public SyncSource(string id, string path)
        {
            this.gid = id;
            this.path = path;
        }

        public string ID
        {
            get
            {
                return this.gid;
            }
        }

        public string Path
        {
            set
            {
                this.path = value;
            }
            get
            {
                return this.path;
            }
        }
    }
}
