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

        private bool _isSelected = false;

        private int _order = 0;

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

        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public string DropboxStatus
        {
            get
            {
                DropboxStatus status = DropboxStatusCheck.ReturnDropboxStatus(this.SyncJob.IntermediaryStorage.Path);
                if (status == OneSync.DropboxStatus.NOT_RUNNING)
                    return "Dropbox not available";
                else if (status == OneSync.DropboxStatus.SYNCHRONIZING)
                    return "Dropbox storage synchronizing";
                else if (status == OneSync.DropboxStatus.UP_TO_DATE)
                    return "Dropbox storage synced";
                return "";
            }
        }

        public void InfoChanged()
        {
            OnPropertyChanged("IntermediaryStoragePath");
            OnPropertyChanged("JobName");
            OnPropertyChanged("SyncSource");
            OnPropertyChanged("DropboxStatus");
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
