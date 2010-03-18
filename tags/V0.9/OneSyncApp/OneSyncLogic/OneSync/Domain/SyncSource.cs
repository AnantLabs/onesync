/*
 $Id: SyncSource.cs 255 2010-03-17 16:08:53Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{

    public enum SourceOption
    {
        SOURCE_ID_EQUALS,
        SOURCE_ID_NOT_EQUALS
    }
    
    /// <summary>
    /// Represents a folder on a PC to be synchronized.
    /// </summary>
    public class SyncSource
    {        
        /// <summary>
        /// Attribute gid to uniquely identify a SyncSource record
        /// </summary>
        private string gid = "";

        /// <summary>
        /// Absolute path to a sync source
        /// </summary>
        private string path = "";

        /// <summary>
        /// column names for sync source table
        /// </summary>
        public const string SOURCE_ABSOLUTE_PATH = "SOURCE_ABSOLUTE_PATH";
        public const string SOURCE_ID = "SOURCE_ID";
        public const string DATASOURCE_INFO_TABLE = "DATASOURCE_INFO_TABLE";


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
