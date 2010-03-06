using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Represents a sync job profile that contains the information
    /// of the local folder to be synchronized as well as the intermediary
    /// storage information
    /// </summary>
    public class Profile
    {
        public const string PROFILE_TABLE =  "PROFILE";
        public const string PROFILE_ID = "ID";
        public const string NAME = "NAME";
        public const string METADATA_SOURCE_LOCATION = "METADATA_SOURCE_LOCATION";
        public const string SYNC_SOURCE_ID = "SYNC_SOURCE_ID";
        
        private string profileId;
        private string profileName;
        private SyncSource syncSource;
        private IntermediaryStorage iStorage;
        
        public Profile(string id , string name , SyncSource syncSource, IntermediaryStorage iStorage)
        {
            this.profileId = id;
            this.profileName = name;
            this.syncSource = syncSource;
            this.iStorage = iStorage;
        }
        
        /// <summary>
        /// Returns the profile name. Each PC will have unique profile name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.profileName;
            }
        }

        /// <summary>
        /// Gets the unique ID of this profile.
        /// </summary>
        public string ID
        {
            get
            {
                return this.profileId;
            }
        }

        /// <summary>
        /// Gets information regarding the local folder that is to be
        /// synchronized for this profile.
        /// </summary>
        public SyncSource SyncSource
        {
            set
            {
                this.syncSource = value;
            }
            get
            {
                return this.syncSource;
            }
        }

        /// <summary>
        /// Get information regarding the intermediary storage for this profile.
        /// </summary>
        public IntermediaryStorage IntermediaryStorage
        {
            set
            {
                this.iStorage = value;
            }
            get
            {
                return this.iStorage;
            }
        }        
    }
}
