/*
 $Id$
 */
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
        /// <summary>
        /// Attribute gid to uniquely identify a SyncSource record
        /// </summary>
        private string gid = "";

        /// <summary>
        /// Absolute path to a sync source
        /// </summary>
        private string path = "";


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
