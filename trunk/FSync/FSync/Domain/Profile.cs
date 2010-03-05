using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
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
        private MetaDataSource mdSource;
        public Profile(string id , string name , SyncSource syncSource, MetaDataSource metaDataSource)
        {
            this.profileId = id;
            this.profileName = name;
            this.syncSource = syncSource;
            this.mdSource = metaDataSource;
        }
        
        public string Name
        {
            get
            {
                return this.profileName;
            }
        }

        public string ID
        {
            get
            {
                return this.profileId;
            }
        }

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

        public MetaDataSource MetaDataSource
        {
            set
            {
                this.mdSource = value;
            }
            get
            {
                return this.mdSource;
            }
        }        
    }
}
