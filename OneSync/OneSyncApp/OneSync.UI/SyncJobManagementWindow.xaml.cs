using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OneSync.Synchronization;
using System.ComponentModel;

namespace OneSync.UI
{
	/// <summary>
	/// Interaction logic for SyncJobManagementWindow.xaml
	/// </summary>
	public partial class SyncJobManagementWindow : Window
	{
        private UISyncJobEntry editingSyncJob;
        private SyncJobManager jobManager;

		public SyncJobManagementWindow()
		{
			this.InitializeComponent();
		}
            
        public SyncJobManagementWindow(UISyncJobEntry selectedSyncJob, SyncJobManager jobManager)
            : this()
		{
            this.jobManager = jobManager;
            editingSyncJob = selectedSyncJob;
            txtSyncJobName.Text = editingSyncJob.SyncJob.Name;
            txtSource.Text = editingSyncJob.SyncJob.SyncSource.Path;
            txtIntStorage.Text = editingSyncJob.SyncJob.IntermediaryStorage.Path;
		}

		private void btnBrowse_Source_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            try
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtSource.Text = fbd.SelectedPath;
                    txtSource.Focus();
                }
            }
            catch (Exception)
            {
                showErrorMsg("The selected folder path is invalid.");
            }
		}

		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            try
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtIntStorage.Text = fbd.SelectedPath;
                    txtIntStorage.Focus();
                }
            }
            catch (Exception)
            {
                showErrorMsg("The selected folder path is invalid.");
            }
		}

		private void txtBlkEditJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            string[] args = {txtSyncJobName.Text.Trim(), 
                            txtSource.Text.Trim(),
                            txtIntStorage.Text.Trim()
                            };
            BackgroundWorker _changeJobWorker = new BackgroundWorker();
            _changeJobWorker.DoWork += new DoWorkEventHandler(_changeJobWorker_DoWork);
            _changeJobWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_changeJobWorker_RunWorkerCompleted);
            _changeJobWorker.RunWorkerAsync(args);            
		}

        void _changeJobWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        void _changeJobWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Check the three required inputs for a new sync job:
            // 1. Sync Job Name;
            // 2. Sync Source Folder Directory;
            // 3. Intermediate Storage Location.
            string[] args = (string[])e.Argument;

            string syncJobName = args[0];
            string syncSourceDir = args[1];
            string intStorageDir = args[2];

            string errorMsg = Validator.validateSyncJobName(syncJobName);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return;
            }

            errorMsg = Validator.validateSyncDirs(syncSourceDir, intStorageDir);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return;
            }

            string oldSyncJobName = editingSyncJob.SyncJob.Name;
            string oldIStorage = editingSyncJob.SyncJob.IntermediaryStorage.Path;
            string oldSyncSource = editingSyncJob.SyncJob.SyncSource.Path;

            try
            {
                editingSyncJob.SyncJob.Name = syncJobName;
                editingSyncJob.SyncJob.IntermediaryStorage.Path = intStorageDir;
                editingSyncJob.SyncJob.SyncSource.Path = syncSourceDir;
                jobManager.Update(editingSyncJob.SyncJob);
                editingSyncJob.InfoChanged();
                if (!editingSyncJob.SyncJob.IntermediaryStorage.Path.Equals(oldIStorage))
                {
                    try
                    {
                        if (!Files.FileUtils.MoveFolder(oldIStorage,
                            editingSyncJob.SyncJob.IntermediaryStorage.Path)) throw new Exception();
                        if (this.Dispatcher.CheckAccess()) this.Close();
                        else  this.Dispatcher.Invoke((Action)delegate { this.Close(); });                        
                    }
                    catch (Exception ex) { showErrorMsg("Can't move file(s) to new location. Please do it manually"); }
                }
                if (this.Dispatcher.CheckAccess()) this.Close();
                else this.Dispatcher.Invoke((Action)delegate { this.Close(); });
            }
            catch (Exception)
            {
                editingSyncJob.SyncJob.Name = oldSyncJobName;
                editingSyncJob.SyncJob.IntermediaryStorage.Path = oldIStorage;
                editingSyncJob.SyncJob.SyncSource.Path = oldSyncSource;
            }            
        }

		private void txtBlkDeleteJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            MessageBoxResult result = MessageBox.Show(
                            "Are you sure you want to delete " + editingSyncJob.SyncJob.Name + "?", "Job Profile Deletion",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                if (!jobManager.Delete(editingSyncJob.SyncJob))
                    showErrorMsg("Unable to delete sync job at this moment.");
                else
                {
                    // Indicate to owner that SyncJob deleted
                    this.DialogResult = true;
                    this.Close();
                }

                // Try to delete Sync Source table from intermediary storage
                try
                {
                    SyncSourceProvider syncSourceProvider = 
                        SyncClient.GetSyncSourceProvider(editingSyncJob.IntermediaryStoragePath);
                    syncSourceProvider.Delete(editingSyncJob.SyncJob.SyncSource);
                }
                catch (Exception) {}
            }
		}

        private void showErrorMsg(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                label_notification.Visibility = Visibility.Hidden;
                label_notification.Content = "";
            }
            else
            {
                label_notification.Content = message;
                label_notification.Visibility = Visibility.Visible;
            }
        }
	}
}