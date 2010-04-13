using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OneSync.Synchronization;

namespace OneSync.UI
{
	public partial class WinSyncPreview
	{
        private SyncJob _job;
        private IEnumerable<SyncAction> allActions = null; /* used for filtering */
        private TaskbarManager tbManager = TaskbarManager.Instance;

        private WinSyncPreview() { }

        /// <summary>
        /// Instantiate a sync preview window job to be previewed.
        /// </summary>
        /// <param name="syncActions">All sync actions to be dispayed for preview.</param>
        public WinSyncPreview(SyncJob job)
		{
			this.InitializeComponent();
            this._job = job;

            // Initialize previewWorker
            BackgroundWorker previewWorker = new BackgroundWorker();
            previewWorker.DoWork += new DoWorkEventHandler(previewWorker_DoWork);
            previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewWorker_RunWorkerCompleted); ;

            if (tbManager != null) tbManager.SetProgressState(TaskbarProgressBarState.Indeterminate);
            pb.Visibility = Visibility.Visible;

            // Check job parameters
            if (jobValid(_job))
            {
                previewUIUpdate(false);
                previewWorker.RunWorkerAsync();
            }
		}

        void previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_job == null) return;

            FileSyncAgent agent = new FileSyncAgent(_job);
            try
            {
                _job.SyncPreviewResult = agent.GenerateSyncPreview();
            }
            catch (DirectoryNotFoundException ex)
            {
                showErrorMsgInvoke("Directory not found: " + ex.Message);
            }
            catch(UnauthorizedAccessException)
            {
                showErrorMsgInvoke("Directory is inaccessible");
            }
            catch (Exception)
            {
                showErrorMsgInvoke("Can't generate sync preview");
            }
        }

        private void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (tbManager != null) tbManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            pb.Visibility = Visibility.Hidden;
			previewUIUpdate(true);

            if (e.Error != null)
            {
                showErrorMsg("Unable to generate preview: " + e.Error.Message);
                return;
            }
            if (_job == null || _job.SyncPreviewResult == null) return;

            // Save results
            this.allActions = _job.SyncPreviewResult.GetAllActions();
            lvPreview.ItemsSource = this.allActions;

        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter();
        }

        private void txtFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            txtFilter.SelectAll();
        }

        private void txtBlkDone_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
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
                lvPreview.ItemsSource = filterActions(a => a.RelativeFilePath.ToLower().Contains(searchTerm));
            else if (cmbFilter.SelectedIndex == 1)
                lvPreview.ItemsSource = filterActions(a => a.ConflictResolution != ConflictResolution.NONE
                                                      && a.RelativeFilePath.ToLower().Contains(searchTerm));
            else if (cmbFilter.SelectedIndex == 2)
                lvPreview.ItemsSource = filterActions(a => (a.ChangeType == ChangeType.NEWLY_CREATED || a.ChangeType == ChangeType.MODIFIED)
                                                      && a.RelativeFilePath.ToLower().Contains(searchTerm));
            else if (cmbFilter.SelectedIndex == 3)
                lvPreview.ItemsSource = filterActions(a => a.ChangeType == ChangeType.DELETED
                                                      && a.RelativeFilePath.ToLower().Contains(searchTerm));
            else if (cmbFilter.SelectedIndex == 4)
                lvPreview.ItemsSource = filterActions(a => a.ChangeType == ChangeType.RENAMED
                                                      && a.RelativeFilePath.ToLower().Contains(searchTerm));
        }

        private IEnumerable<SyncAction> filterActions(Func<SyncAction, bool> predicate)
        {
            var result = allActions.Where(predicate);
            return result;
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

        private void previewUIUpdate(bool isEnable)
        {
            cmbFilter.IsEnabled = isEnable;
            txtFilter.IsEnabled = isEnable;
            txtBlkDone.IsEnabled = isEnable;
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

        private void showErrorMsgInvoke(string errorMsg)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                showErrorMsg(errorMsg);
            });
        }
	}
}