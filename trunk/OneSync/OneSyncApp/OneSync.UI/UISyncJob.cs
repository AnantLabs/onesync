using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OneSync.UI
{
    class UISyncJob
    {
        private ObservableCollection<UISyncJobEntry> _SyncJobEntries = new ObservableCollection<UISyncJobEntry>();

        public ObservableCollection<UISyncJobEntry> SyncJobEntries
        {
            get { return _SyncJobEntries; }
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="fileName">The name of the processed file.</param>
        /// <param name="status">The status of the process: 0 - Completely processed; 1 - Conflict.</param>
        /// <param name="message">A detailed message.</param>
        public void Add(string syncJobName, string syncSourceDir, string intStorageDir)
        {
            UISyncJobEntry entry = new UISyncJobEntry();

            entry.JobName = syncJobName;
            entry.SyncSource = syncSourceDir;
            entry.IntStorage = intStorageDir;
            entry.DeleteJob = "";

            //Add a new sync job entry to the sync jobs collection.
            _SyncJobEntries.Add(entry);
        }
    }

    // Private class to represent a sync job entry
    public class UISyncJobEntry
    {
        public string JobName { get; set; }
        public string SyncSource { get; set; }
        public string IntStorage { get; set; }
        public string DeleteJob { get; set; }
    }
}
