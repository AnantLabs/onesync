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
            //Check the three required inputs for a new sync job:
            // 1. Sync Job Name;
            // 2. Sync Source Folder Directory;
            // 3. Intermediate Storage Location.
            string syncJobName = txtSyncJobName.Text.Trim();
            string syncSourceDir = txtSource.Text.Trim();
            string intStorageDir = txtIntStorage.Text.Trim();

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

            //Store the original info of the job first.
            //If there is an exception thrown later, the system will be able to reset.
            string originalSyncJobName = editingSyncJob.SyncJob.Name;
            string originalSyncSourceDir = editingSyncJob.SyncJob.SyncSource.Path;
            string originalIntStorageDir = editingSyncJob.SyncJob.IntermediaryStorage.Path;

            //Update the job if all three inputs mentioned above are valid.
            editingSyncJob.SyncJob.Name = syncJobName;
            editingSyncJob.SyncJob.SyncSource.Path = syncSourceDir;
            editingSyncJob.SyncJob.IntermediaryStorage.Path = intStorageDir;

            try
            {
                jobManager.Update(editingSyncJob.SyncJob);
                editingSyncJob.InfoChanged();
                this.Close();
            }
            catch (ProfileNameExistException)
            {
                showErrorMsg("A Sync Job with the same name already exists.");

                //Reset.
                editingSyncJob.SyncJob.Name = originalSyncJobName;
                editingSyncJob.SyncJob.SyncSource.Path = originalSyncSourceDir;
                editingSyncJob.SyncJob.IntermediaryStorage.Path = originalIntStorageDir;
                editingSyncJob.InfoChanged();
            }
            catch (Exception)
            {
                showErrorMsg("Unable to update sync job at this moment.");

                //Reset.
                editingSyncJob.SyncJob.Name = originalSyncJobName;
                editingSyncJob.SyncJob.SyncSource.Path = originalSyncSourceDir;
                editingSyncJob.SyncJob.IntermediaryStorage.Path = originalIntStorageDir;
                editingSyncJob.InfoChanged();
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
                    this.Close();
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