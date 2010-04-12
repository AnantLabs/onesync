using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using OneSync.Synchronization;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;

namespace OneSync.UI
{
	public partial class WinSyncPreview
	{
        SyncJob _job;
        private BackgroundWorker previewWorker;
        IEnumerable<SyncAction> allActions = null; /* used for filtering */

        TaskbarManager tbManager = TaskbarManager.Instance;

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

            if (tbManager != null) tbManager.SetProgressState(TaskbarProgressBarState.Indeterminate);
            pb.Visibility = Visibility.Visible;

            // Check job parameters
            if (jobValid(_job))
                previewWorker.RunWorkerAsync();
		}

        void previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_job == null) return;

			this.Dispatcher.Invoke((Action)delegate
			{
				previewUIUpdate(false);
			});
            FileSyncAgent agent = new FileSyncAgent(_job);
            _job.SyncPreviewResult = agent.GenerateSyncPreview();

        }

        private bool jobValid(SyncJob job)
        {
            if (!Directory.Exists(job.IntermediaryStorage.Path))
            {
                showErrorMsg("Intermediary storage path: \"" + job.IntermediaryStorage.Path +
                             "\" not found.");
                return false;
            }
            return true;
        }

        void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (tbManager != null) tbManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            pb.Visibility = Visibility.Hidden;

			previewUIUpdate(true);

            if (e.Error != null)
            {
                showErrorMsg("Unable to generate preview: " + e.Error.Message);
                return;
            }

            if (_job == null) return;

            // Save results
            SyncPreviewResult result = _job.SyncPreviewResult;
            this.allActions = result.GetAllActions();
            lvPreview.ItemsSource = this.allActions;

        }

        private void showErrorMsg(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                txtError.Visibility = Visibility.Hidden;
            else
            {
                txtError.Text = errorMsg;
                txtError.Visibility = Visibility.Visible;
                if (tbManager != null) tbManager.SetProgressState(TaskbarProgressBarState.NoProgress);
                pb.Visibility = Visibility.Hidden;
            }
        }
		
		void previewUIUpdate(bool isEnable)
		{
			cmbFilter.IsEnabled = isEnable;
			txtFilter.IsEnabled = isEnable;
			txtBlkDone.IsEnabled = isEnable;
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
            if (txtFilter == null || allActions == null) return; 
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
            else if (cmbFilter.SelectedIndex == 4)
            {
                lvPreview.ItemsSource =
                    from a in allActions
                    where a.ChangeType == ChangeType.RENAMED
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
        }

        private void txtFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            txtFilter.SelectAll();
        }

        private void txtBlkDone_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
	}
}