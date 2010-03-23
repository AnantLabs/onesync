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
    /// Represents a sync job profile that contains the information
    /// of the local folder to be synchronized as well as the intermediary
    /// storage information
    /// </summary>
    public class Profile
    {
        public const string PROFILE_TABLE =  "PROFILE";
        public const string PROFILE_ID = "PROFILE_ID";
        public const string PROFILE_NAME = "PROFILE_NAME";
        public const string METADATA_SOURCE_LOCATION = "METADATA_SOURCE_LOCATION";
        public const string SYNC_SOURCE_ID = "SYNC_SOURCE_ID";
        
        private string _profileId;
        private string _profileName;
        
        private SyncSource _syncSource;       
        private IntermediaryStorage _iStorage;

        
        public Profile(string name , SyncSource syncSource, IntermediaryStorage iStorage)
            : this(System.Guid.NewGuid().ToString(), name, syncSource, iStorage)
        {
        }

        public Profile(string id, string name, SyncSource syncSource, IntermediaryStorage iStorage)
        {
            this._profileId = id;
            this._profileName = name;
            this._syncSource = syncSource;
            this._iStorage = iStorage;
        }

        public Profile(string profileName, string absoluteSyncPath, string absoluteIntermediatePath)
        {
            SyncSource syncSource = new SyncSource(System.Guid.NewGuid().ToString(), absoluteSyncPath);
            IntermediaryStorage iSource = new IntermediaryStorage(absoluteIntermediatePath);

            this._profileId = System.Guid.NewGuid().ToString();
            this._profileName = profileName;
            this._syncSource = syncSource;
            this._iStorage = iSource;
        }

        

        public Profile(SyncSource syncSource, IntermediaryStorage metaDataSource)
        {
            this._syncSource = syncSource;
            this._iStorage = metaDataSource;
        }
        
        /// <summary>
        /// Returns the profile name. Each PC will have unique profile name.
        /// </summary>
        public string Name
        {
            get
            {
                return this._profileName;
            }
            set
            {
                this._profileName = value;
            }
        }

        /// <summary>
        /// Gets the unique ID of this profile.
        /// </summary>
        public string ID
        {
            get
            {
                return this._profileId;
            }
        }

        /// <summary>
        /// Gets information regarding the local folder that is to be
        /// synchronized for this profile.
        /// </summary>
        public SyncSource SyncSource
        {
            get
            {
                return this._syncSource;
            }
        }

        /// <summary>
        /// Get information regarding the intermediary storage for this profile.
        /// </summary>
        public IntermediaryStorage IntermediaryStorage
        {
            get
            {
                return this._iStorage;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
