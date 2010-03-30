using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OneSync.Synchronization;

namespace OneSync.UI
{

    // UI Wrapper class for SyncJob
    public class UISyncJobEntry: IEquatable<UISyncJobEntry>
    {
        public UISyncJobEntry(SyncJob job)
        {
            this.SyncJob = job;
            this.IsSelected = true;
        }

        public SyncJob SyncJob { get; set; }
        
        public string SyncSource {
            get { return this.SyncJob.SyncSource.Path; }
        }

        public string JobName {
            get { return this.SyncJob.Name; } 
        }

        public string IntStorage {
            get { return this.SyncJob.IntermediaryStorage.Path; } 
        }

        public string JobId
        {
            get { return this.SyncJob.ID; }
        }

        public bool IsSelected { get; set; }


        #region IEquatable<UISyncJobEntry> Members

        public bool Equals(UISyncJobEntry other)
        {
            return (this.JobId == other.JobId) || (this.JobName.Equals(other.JobName));
        }

        #endregion
    }
}
