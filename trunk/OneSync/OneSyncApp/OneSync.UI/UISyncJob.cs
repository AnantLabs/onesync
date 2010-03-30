using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OneSync.Synchronization;
using System.ComponentModel;

namespace OneSync.UI
{

    // UI Wrapper class for SyncJob
    public class UISyncJobEntry : IEquatable<UISyncJobEntry>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected = true;

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

        public string IntermediaryStoragePath {
            get { return this.SyncJob.IntermediaryStorage.Path; } 
        }

        public string JobId
        {
            get { return this.SyncJob.ID; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public void InfoChanged()
        {
            OnPropertyChanged("IntermediaryStoragePath");
            OnPropertyChanged("JobName");
            OnPropertyChanged("SyncSource");
        }

        public static IList<UISyncJobEntry> GetSelectedJobs(IEnumerable<UISyncJobEntry> entries)
        {
            List<UISyncJobEntry> selectedEntries = new List<UISyncJobEntry>();

            foreach (UISyncJobEntry entry in entries)
            {
                if (entry.IsSelected)
                    selectedEntries.Add(entry);
            }

            return selectedEntries;
        }


        #region IEquatable<UISyncJobEntry> Members

        public bool Equals(UISyncJobEntry other)
        {
            return (this.JobId == other.JobId) || (this.JobName.Equals(other.JobName));
        }

        #endregion

        protected virtual void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

    }
}
