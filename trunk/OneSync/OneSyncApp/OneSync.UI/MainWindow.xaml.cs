/*
 $Id: MainWindow.xaml.cs 287 2010-03-18 02:25:56Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using OneSync.Synchronization;

namespace OneSync.UI
{

	public partial class MainWindow : Window
	{
		// Start: Global variables.        
        private string syncDir; //The directory of the chosen source folder.
        private string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;
        
        private SyncJobManager profileManager;
        private FileSyncAgent syncAgent; //The file sync agent.
        private BackgroundWorker syncWorker;

        private UILog uiLog = new UILog();
        // End: Global variables.
		
		
		/*========================================================
		PART 1: Home Screen
		========================================================*/
		
		public MainWindow()
		{
            this.InitializeComponent();
			
#if DEBUG
			testingCode(0); //Testing code number 0.
#endif

            // Get current synchronization directory from command line arguments.
            // Default sync directory is current directory of app.
            string[] args = System.Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (Validator.validateDirPath(args[1]) == null)
                    syncDir = args[1];
                else
                    syncDir = Directory.GetCurrentDirectory().ToString();
            }

            profileManager = SyncClient.GetSyncJobManager(STARTUP_PATH);
            lblSyncDir.Content = normalizeString(syncDir, 20, 50);
            reloadProfileComboBox();
            listLog.ItemsSource = uiLog.LogEntries;

            // Intialize syncWorker 
            syncWorker = new BackgroundWorker();
            syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
            syncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncWorker_RunWorkerCompleted);
        }

        private void cmbProfiles_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                txtBlkProceed_MouseDown(null, null);
        }

        private void cmbProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            canvasEditProfile.Visibility = Visibility.Hidden;

            if (cmbProfiles.SelectedItem as SyncJob == null)
                return;
            else
            {
                canvasEditProfile.Visibility = Visibility.Visible;
                txtRenJob.Text = cmbProfiles.SelectedItem.ToString();
            }
        }

        private void txtBlkRenJob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SyncJob p = cmbProfiles.SelectedItem as SyncJob;
            if (p == null) return;

            string errorMsg = Validator.validateSyncJobName(txtRenJob.Text);

            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return;
            }

            showErrorMsg("");
            try
            {
                p.Name = txtRenJob.Text.Trim();
                profileManager.Update(p);
                txtBlkProceed_MouseDown(null, null);
            }
            catch (ProfileNameExistException)
            {
                showErrorMsg("Rename failed: A sync job with the name already exists.");
                return;
            }
            catch (Exception)
            {
                // Log error
                showErrorMsg("Rename failed.");
                throw;
            }
        }

        private void txtBlkDelJob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SyncJob p = cmbProfiles.SelectedItem as SyncJob;
            if (p == null) return;

            DialogResult result = System.Windows.Forms.MessageBox.Show(
                            "Are you sure you want to delete " + p.Name + "?", "Job Profile Deletion",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                if (!profileManager.Delete(p))
                    showErrorMsg("Unable to delete profile.");

                reloadProfileComboBox();
            }
        }

        private void txtBlkProceed_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SyncJob p = cmbProfiles.SelectedItem as SyncJob;

            if (p == null)
            {
                string errorMsg = Validator.validateSyncJobName(cmbProfiles.Text);
                if (errorMsg != null)
                {
                    showErrorMsg(errorMsg);
                    return;
                }
                txtIntStorage.Text = "";
            }
            else
            {
                txtIntStorage.Text = p.IntermediaryStorage.Path;
                Window.Title = p.Name + " - OneSync";
            }

            UpdateSyncUI(false, false);
            ((Storyboard)Resources["sbNext"]).Begin(this);
        }
		
		
		/*========================================================
		PART 2: Sync Screen
		========================================================*/

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtIntStorage.Text = fbd.SelectedPath;
                txtIntStorage.Focus();
            }
        }

        private void btnSyncStatic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); // clear error msg

            /* Validate information/syncParameters */

            string errorMsg = Validator.validateDirPath(txtIntStorage.Text);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return;
            }

            errorMsg = Validator.validateSyncDirs(syncDir, txtIntStorage.Text);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return;
            }


            // Create a new profile if necessary
            SyncJob p = cmbProfiles.SelectedItem as SyncJob;
            if (p == null) /* No saved profile selected */
            {
                // Create a new profile
                try
                {
                    p = profileManager.CreateSyncJob(cmbProfiles.Text, syncDir, txtIntStorage.Text);
                    cmbProfiles.Items.Add(p);
                    cmbProfiles.SelectedItem = p;
                }
                catch (ProfileNameExistException)
                {
                    showErrorMsg("A sync job with the same name already exists.");
                    return;
                }
                catch (Exception)
                {
                    showErrorMsg("Unable to save newly created profile.");
                    return;
                }
            }
            else
            {
                // Update current profile
                p.SyncSource.Path = syncDir;
                p.IntermediaryStorage.Path = txtIntStorage.Text;

                if (!profileManager.Update(p))
                {
                    showErrorMsg("Unable to update profile.");
                    return;
                }
            }

            Synchronize(p);
        }

        private void txtBlkShowLog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //View log file (The extension of the file should be .html).
            Process.Start(Log.returnLogReportPath(syncDir, false));
        }

        private void Expander_Toggle(object sender, RoutedEventArgs e)
        {
            const string COLLAPSED_HEADER = "Show details";
            const string EXPANDED_HEADER = "Hide details";

            if (Expander.Header.ToString() == COLLAPSED_HEADER)
            {
                Expander.Header = EXPANDED_HEADER;
                this.Height += 220;
            }
            else
            {
                Expander.Header = COLLAPSED_HEADER;
                this.Height -= 220;
            }
        }

        private void txtBlkBackToHome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); //Empty the notification message (if any).

            Window.Title = "OneSync"; //Change back the menu title.

            //Import all the previous created existing sync job profiles.
            //Note that only the profile having the directory which is the current directory will be loaded.
            reloadProfileComboBox();

            // Show log will be shown again after sync is complete.
            txtBlkShowLog.Visibility = Visibility.Hidden;

            // Animate to start screen
            Storyboard sb = (Storyboard)Window.Resources["sbHome"];
            sb.Begin(this);
        }
        		
		
		/*========================================================
		PART 3: UI handling code
		========================================================*/

        private void reloadProfileComboBox()
        {
            //Reload the profile combobox.
            cmbProfiles.Items.Clear();
            try
            {
                IList<SyncJob> profileItemsCollection = SyncClient.GetSyncJobManager(STARTUP_PATH).LoadAllJobs();
                foreach (SyncJob profileItem in profileItemsCollection)
                {
                    if (profileItem.SyncSource.Path.Equals(syncDir))
                        cmbProfiles.Items.Add(profileItem);
                }
            }
            catch (Exception)
            {
                // Log error
                showErrorMsg("Error loading profiles.");
            }

            cmbProfiles.SelectedIndex = -1;
        }

        // There are 3 states for visiblity of controls in CanvasSync:
        // 1. syncInProgress = false, syncCompletedBefore = false
        // 2. syncInProgress = true, syncCompletedBefore = true
        // 2. syncInProgress = false, syncCompletedBefore = true
        private void UpdateSyncUI(bool syncInProgress, bool showProgressControls)
        {
                Storyboard sb = (Storyboard)Window.Resources["sbRotateSync"];

            // Set Visibility of common controls
            txtBlkBackToHome.IsEnabled = !syncInProgress;
            btnSyncStatic.IsEnabled = !syncInProgress;
            txtIntStorage.IsEnabled = !syncInProgress;
            btnBrowse.IsEnabled = !syncInProgress;

            if (syncInProgress)
            {
                btnSyncRotating.Visibility = Visibility.Visible;
                btnSyncStatic.Visibility = Visibility.Hidden;
                sb.Begin(this);
            }
            else
            {
                btnSyncRotating.Visibility = Visibility.Hidden;
                btnSyncStatic.Visibility = Visibility.Visible;
                sb.Stop();
            }

            if (showProgressControls)
            {
                pbSync.Visibility = Visibility.Visible;
                lblStatus.Visibility = Visibility.Visible;
            }
            else
            {
                pbSync.Visibility = Visibility.Hidden;
                lblStatus.Visibility = Visibility.Hidden;
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

        #region Synchronization

        private void Synchronize(SyncJob p)
        {
            syncAgent = new FileSyncAgent(p);

            syncAgent.ProgressChanged += new SyncProgressChangedHandler(currAgent_ProgressChanged);
            syncAgent.StageChanged += new SyncStageChangedHandler(currAgent_StatusChanged);
            syncAgent.SyncCompleted += new SyncCompletedHandler(currAgent_SyncCompleted);
            syncAgent.SyncFileChanged += new SyncFileChangedHandler(currAgent_FileChanged);
            syncAgent.SyncStatusChanged += new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);

            UpdateSyncUI(true, true);

            syncWorker.RunWorkerAsync();
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SyncPreviewResult previewResult = syncAgent.GenerateSyncPreview();
            syncAgent.Synchronize(previewResult);
        }

        void syncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // exception thrown during sync
                // Log error
                // Notify user
            }

            // Update UI and hide sync progress controls
            UpdateSyncUI(false, true);
        }

        #endregion

        #region SyncAgent Event Handling Code

        /// <summary>
        /// Update the progress bar.
        /// </summary>
        void currAgent_ProgressChanged(object sender, Synchronization.SyncProgressChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                pbSync.Value = e.Value;
            else
                pbSync.Dispatcher.Invoke((MethodInvoker)delegate { currAgent_ProgressChanged(sender, e); });
        }

        /// <summary>
        /// Update the status message.
        /// </summary>
        void currAgent_StatusChanged(object sender, Synchronization.SyncStageChangedEventArgs e)
        {
            //Display some text so that the user knows that what OneSync is doing during the synchronization.
            if (this.Dispatcher.CheckAccess())
            {
                switch (e.SyncStage)
                {
                    case SyncStageChangedEventArgs.Stage.APPLY_PATCH:
                        lblStatus.Content = "Applying Patch"; break;
                    case SyncStageChangedEventArgs.Stage.GENERATE_PATCH:
                        lblStatus.Content = "Generating Patch"; break;
                    case SyncStageChangedEventArgs.Stage.UPDATE_DATA:
                        lblStatus.Content = "Updating Data"; break;
                    case SyncStageChangedEventArgs.Stage.VERIFY_PATCH:
                        lblStatus.Content = "Verifying Patch"; break;
                }
            }
            else
                lblStatus.Dispatcher.Invoke((MethodInvoker)delegate { currAgent_StatusChanged(sender, e); });
        }

        /// <summary>
        /// Tell the user which file is being processed now.
        /// </summary>
        void currAgent_FileChanged(object sender, Synchronization.SyncFileChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                lblStatus.Content = "Synchronizing: " + e.RelativePath;
            else
                lblStatus.Dispatcher.Invoke((MethodInvoker)delegate { currAgent_FileChanged(sender, e); });
        }

        /// <summary>
        /// Trigerred when the sync process is done.
        /// </summary>
        void currAgent_SyncCompleted(object sender, Synchronization.SyncCompletedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                UpdateSyncUI(false, true);

                lblStatus.Content = "Sync process is successfully done.";

                if (File.Exists(Log.returnLogReportPath(syncDir, false))) //To be changed. Depends on Naing.
                    txtBlkShowLog.Visibility = Visibility.Visible;
                else
                    txtBlkShowLog.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Dispatcher.Invoke((MethodInvoker)delegate { currAgent_SyncCompleted(sender, e); });
            }
        }

        void syncAgent_SyncStatusChanged(object sender, SyncStatusChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                lblStatus.Content = e.Message;
            else
                this.Dispatcher.Invoke((MethodInvoker)delegate { syncAgent_SyncStatusChanged(sender, e); });
        }

        #endregion		

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (syncWorker.IsBusy)
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "Are you sure you want to close the program now? The synchronization is still ongoing.",
                    "Goodbye OneSync", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    e.Cancel = (result == System.Windows.Forms.DialogResult.No);
            }
        }

        private string normalizeString(string str, int prefixLength, int suffixLength)
        {
            if (str.Length > prefixLength + suffixLength)
                return str.Substring(0, prefixLength) + "..." + str.Substring(str.Length - suffixLength);
            else
                return str;
        }
        

#if DEBUG
        /// <summary>
        /// This method is for the GUI Designer to test the program.
        /// </summary>
        /// <param name="testingNumber">The number of the testing code.</param>
        private void testingCode(int testingNumber)
        {
            switch (testingNumber)
            {
                case 0:
                    
                    uiLog.Add("CS3215.txt", UILog.Status.Completed, "CS3215.txt has been uploaded to the intermediate storage.");
                    uiLog.Add("Proposal.pdf", UILog.Status.Conflict, "There is another different copy of Proposal.pdf found in the patch.");
                    break;
            }
        }
#endif

	}
}