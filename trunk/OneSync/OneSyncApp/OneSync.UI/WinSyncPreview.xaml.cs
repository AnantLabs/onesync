using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using OneSync.Synchronization;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace OneSync.UI
{
	public partial class WinSyncPreview
	{
        SyncJob _job;
        private BackgroundWorker previewWorker;
        IEnumerable<SyncAction> allActions = null; /* used for filtering */

        /// <summary>
        /// Instantiate a sync preview window job to be previewed.
        /// </summary>
        /// <param name="syncActions">All sync actions to be dispayed for preview.</param>
        public WinSyncPreview(SyncJob job)
		{
			this.InitializeComponent();
            this._job = job;

            // Initialize previewWorker
            previewWorker = new BackgroundWorker();
            previewWorker.DoWork += new DoWorkEventHandler(previewWorker_DoWork);
            previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewWorker_RunWorkerCompleted); ;
            previewWorker.RunWorkerAsync();
		}

        void previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_job == null) return;

            if (_job.SyncPreviewResult == null)
            {
                FileSyncAgent agent = new FileSyncAgent(_job);
                _job.SyncPreviewResult = agent.GenerateSyncPreview();
            }
        }

        void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Log error
                return;
            }

            if (_job == null) return;

            // Save results
            SyncPreviewResult result = _job.SyncPreviewResult;
            this.allActions = result.GetAllActions();
            lvPreview.ItemsSource = this.allActions;
        }

        void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter();
        }

        private void filter()
        {
            if (txtFilter == null) return; 
            string searchTerm = txtFilter.Text.Trim().ToLower();

            if (cmbFilter.SelectedIndex == 0)
            {
                lvPreview.ItemsSource =
                    from a in allActions
                    where a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
            else if (cmbFilter.SelectedIndex == 1)
            {
                lvPreview.ItemsSource =
                    from a in allActions 
                    where a.ConflictResolution != ConflictResolution.NONE
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
            else if (cmbFilter.SelectedIndex == 2)
            {
                lvPreview.ItemsSource =
                    from a in allActions 
                    where (a.ChangeType == ChangeType.NEWLY_CREATED || a.ChangeType == ChangeType.MODIFIED)
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
            else if (cmbFilter.SelectedIndex == 3)
            {
                lvPreview.ItemsSource =
                    from a in allActions 
                    where a.ChangeType == ChangeType.DELETED
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            txtFilter.SelectAll();
        }
	}
}