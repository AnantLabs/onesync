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

        MainWindow main = null;
            
        public SyncJobManagementWindow(UISyncJobEntry selectedSyncJob, SyncJobManager jobManager, MainWindow parent)
            : this()
		{
            this.jobManager = jobManager;
            editingSyncJob = selectedSyncJob;
            txtSyncJobName.Text = editingSyncJob.SyncJob.Name;
            txtSource.Text = editingSyncJob.SyncJob.SyncSource.Path;
            txtIntStorage.Text = editingSyncJob.SyncJob.IntermediaryStorage.Path;
            main = parent;
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
                try
                {
                    if (!editingSyncJob.SyncJob.IntermediaryStorage.Path.Equals(
                        oldIStorage))
                    {
                        if (!Files.FileUtils.MoveFolder(oldIStorage,
                        editingSyncJob.SyncJob.IntermediaryStorage.Path)) throw new Exception();
                        this.Close();
                    }                    
                }
                catch (Exception ex) { showErrorMsg("Can't move file(s) to new location. Please do it manually"); }
                this.Close();
            }
            catch (Exception)
            {
                editingSyncJob.SyncJob.Name = oldSyncJobName;
                editingSyncJob.SyncJob.IntermediaryStorage.Path = oldIStorage;
                editingSyncJob.SyncJob.SyncSource.Path = oldSyncSource;
            }
                
            
            /*
            //Store the original info of the job first.
            //If there is an exception thrown later, the system will be able to reset.
            string originalSyncJobName = editingSyncJob.SyncJob.Name;
            string originalSyncSourceDir = editingSyncJob.SyncJob.SyncSource.Path;
            string originalIntStorageDir = editingSyncJob.SyncJob.IntermediaryStorage.Path;           

            try
            {
                jobManager.Update(editingSyncJob.SyncJob);

                //Update the job if all three inputs mentioned above are valid.
                editingSyncJob.SyncJob.Name = syncJobName;
                editingSyncJob.SyncJob.SyncSource.Path = syncSourceDir;
                editingSyncJob.SyncJob.IntermediaryStorage.Path = intStorageDir;
                editingSyncJob.InfoChanged();
                try
                {
                    if (!originalIntStorageDir.Equals(intStorageDir)) Files.FileUtils.MoveFolder(originalIntStorageDir, intStorageDir);
                    this.Close();
                }
                catch (Exception) { showErrorMsg("Error moving file(s) to new intermediate storage"); }                               
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
            */
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
                    this.main.LoadSyncJobs();
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