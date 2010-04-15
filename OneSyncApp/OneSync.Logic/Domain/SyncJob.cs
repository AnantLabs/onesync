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
        private SyncSource _syncSource;       
        private IntermediaryStorage _iStorage;
        
        public SyncJob(string name , SyncSource syncSource, IntermediaryStorage iStorage)
            : this(System.Guid.NewGuid().ToString(), name, syncSource, iStorage)
        {
        }

        public SyncJob(string id, string name, SyncSource syncSource, IntermediaryStorage iStorage)
        {
            this._jobId = id;
            this.Name = name;
            this._syncSource = syncSource;
            this._iStorage = iStorage;
        }
        
        
        /// <summary>
        /// Returns the SyncJob name. Each PC will have unique Job name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the unique ID of this SyncJob.
        /// </summary>
        public string ID
        {
            get { return this._jobId; }
        }

        /// <summary>
        /// Gets information regarding the local folder that is to be
        /// synchronized for this SyncJob.
        /// </summary>
        public SyncSource SyncSource
        {
            get { return this._syncSource; }
        }

        /// <summary>
        /// Get information regarding the intermediary storage for this SyncJob.
        /// </summary>
        public IntermediaryStorage IntermediaryStorage
        {
            get { return this._iStorage; }
        }

        /// <summary>
        /// SyncActions already generated for this SyncJob
        /// </summary>
        public SyncPreviewResult SyncPreviewResult { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
