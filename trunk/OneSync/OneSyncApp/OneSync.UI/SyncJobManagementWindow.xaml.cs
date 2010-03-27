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
using System.Windows.Forms;
using OneSync.Synchronization;

namespace OneSync.UI
{
	/// <summary>
	/// Interaction logic for SyncJobManagementWindow.xaml
	/// </summary>
	public partial class SyncJobManagementWindow : Window
	{
        private SyncJob editingSyncJob;
        private SyncJobManager profileManager;

		public SyncJobManagementWindow()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
		}

        public SyncJobManagementWindow(SyncJob selectedSyncJob, SyncJobManager inputProfileManager)
            : this()
		{
            profileManager = inputProfileManager;
            editingSyncJob = selectedSyncJob;
            txtSyncJobName.Text = editingSyncJob.Name;
            txtSource.Text = editingSyncJob.SyncSource.Path;
            txtIntStorage.Text = editingSyncJob.IntermediaryStorage.Path;
		}

		private void btnBrowse_Source_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSource.Text = fbd.SelectedPath;
                txtSource.Focus();
            }
		}

		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtIntStorage.Text = fbd.SelectedPath;
                txtIntStorage.Focus();
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

            //Update the job if all three inputs mentioned above are valid.
            editingSyncJob.Name = syncJobName;
            editingSyncJob.SyncSource.Path = syncSourceDir;
            editingSyncJob.IntermediaryStorage.Path = intStorageDir;

            if (!profileManager.Update(editingSyncJob))
                showErrorMsg("Unable to update sync job at this moment.");

            this.Close();
		}

		private void txtBlkDeleteJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            DialogResult result = System.Windows.Forms.MessageBox.Show(
                            "Are you sure you want to delete " + editingSyncJob.Name + "?", "Job Profile Deletion",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                if (!profileManager.Delete(editingSyncJob))
                    showErrorMsg("Unable to delete sync job at this moment.");
            }
            this.Close();
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