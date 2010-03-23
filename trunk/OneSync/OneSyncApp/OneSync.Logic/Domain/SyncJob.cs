/*
 $Id: Profile.cs 306 2010-03-23 10:34:17Z deskohet $
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
    public class SyncJob
    {
        
        private string _jobId;
        private string _jobName;
        
        private SyncSource _syncSource;       
        private IntermediaryStorage _iStorage;

        
        public SyncJob(string name , SyncSource syncSource, IntermediaryStorage iStorage)
            : this(System.Guid.NewGuid().ToString(), name, syncSource, iStorage)
        {
        }

        public SyncJob(string id, string name, SyncSource syncSource, IntermediaryStorage iStorage)
        {
            this._jobId = id;
            this._jobName = name;
            this._syncSource = syncSource;
            this._iStorage = iStorage;
        }

        public SyncJob(string jobName, string absoluteSyncPath, string absoluteIntermediatePath)
        {
            SyncSource syncSource = new SyncSource(System.Guid.NewGuid().ToString(), absoluteSyncPath);
            IntermediaryStorage iSource = new IntermediaryStorage(absoluteIntermediatePath);

            this._jobId = System.Guid.NewGuid().ToString();
            this._jobName = jobName;
            this._syncSource = syncSource;
            this._iStorage = iSource;
        }

        

        public SyncJob(SyncSource syncSource, IntermediaryStorage metaDataSource)
        {
            this._syncSource = syncSource;
            this._iStorage = metaDataSource;
        }
        
        /// <summary>
        /// Returns the SyncJob name. Each PC will have unique Job name.
        /// </summary>
        public string Name
        {
            get
            {
                return this._jobName;
            }
            set
            {
                this._jobName = value;
            }
        }

        /// <summary>
        /// Gets the unique ID of this SyncJob.
        /// </summary>
        public string ID
        {
            get
            {
                return this._jobId;
            }
        }

        /// <summary>
        /// Gets information regarding the local folder that is to be
        /// synchronized for this SyncJob.
        /// </summary>
        public SyncSource SyncSource
        {
            get
            {
                return this._syncSource;
            }
        }

        /// <summary>
        /// Get information regarding the intermediary storage for this SyncJob.
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
