using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneSync.Synchronization;

namespace OneSync.UI
{
    public class SyncWorkerArguments
    {
        private IList<UISyncJobEntry> _selectedSyncJobs;

        private List<FileSyncAgent> _agents;

        public SyncWorkerArguments(IList<UISyncJobEntry> selectedSyncJobs, List<FileSyncAgent> agents)
        {
            _selectedSyncJobs = selectedSyncJobs;
            _agents = agents;
        }

        public IList<UISyncJobEntry> SelectedSyncJobs
        {
            get { return _selectedSyncJobs; }
            set { _selectedSyncJobs = value; }
        }

        public List<FileSyncAgent> Agents
        {
            get { return _agents; }
            set { _agents = value; }
        }
    }
}
