using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OneSync.Synchronization;

namespace OneSync.UI
{

    // UI Wrapper class for SyncJob
    public class UISyncJobEntry
    {
        public UISyncJobEntry(SyncJob job)
        {
            this.SyncJob = job;
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

    }
}
