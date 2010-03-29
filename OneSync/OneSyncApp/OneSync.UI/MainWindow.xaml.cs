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
        private string syncDir = ""; //The directory of the chosen source folder.
        private string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;
        
        private SyncJobManager profileManager;
        private FileSyncAgent syncAgent; //The file sync agent.
        private BackgroundWorker syncWorker;
        private BackgroundWorker previewWorker;

        IList<FileSyncAgent> syncAgents = new List<FileSyncAgent>();

        private UILog uiLog = new UILog();
        private UISyncJob uiSyncJob = new UISyncJob();
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

            profileManager = SyncClient.GetSyncJobManager(STARTUP_PATH);

            if (args.Length > 1 && Validator.validateDirPath(args[1]) == null)
            {
                txtSource.Text = args[1];
                reloadProfile();
            }

            // Initialize syncWorker 
            syncWorker = new BackgroundWorker();
            syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
            syncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncWorker_RunWorkerCompleted);

            // Initialize previewWorker
            previewWorker = new BackgroundWorker();
            previewWorker.DoWork += new DoWorkEventHandler(previewWorker_DoWork);
            previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewWorker_RunWorkerCompleted);
        }

        private void txtBlkProceed_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg("");
            try
            {
                //Show the info of the first Sync Job first.
                //The rest will be updated later during the sync.
                UISyncJobEntry p = listAllSyncJobs.SelectedItems[0] as UISyncJobEntry;

                lblSyncJobName.Content = normalizeString(p.JobName, 20, 50);
                lblSyncJobName.ToolTip = p.JobName;
                lblSyncJobSource.Content = normalizeString(p.SyncSource, 20, 30);
                lblSyncJobSource.ToolTip = p.SyncSource;
                lblSyncJobStorage.Content = normalizeString(p.IntStorage, 20, 30);
                lblSyncJobStorage.ToolTip = p.IntStorage;
                reloadProfile();
                listLog.ItemsSource = uiLog.LogEntries;

                Window.Title = p.JobName + " - OneSync";
                UpdateSyncUI(false, false);
                ((Storyboard)Resources["sbNext"]).Begin(this);
            }
            catch (Exception)
            {
                showErrorMsg("Please select an existing sync job.");
            }
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

        private void txtBlkNewJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

            //Create new sync job if all three inputs mentioned above are valid.
            try
            {
                SyncJob p = profileManager.CreateSyncJob(syncJobName, syncSourceDir, intStorageDir);

                //Reload the list of sync jobs.
                reloadProfile();
            }
            catch (ProfileNameExistException)
            {
                showErrorMsg("A sync job with the same name already exists.");
                return;
            }
            catch (Exception ee)
            {
                showErrorMsg(ee.Message);
                return;
            }
        }
		
		private void txtSource_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            showErrorMsg("");
            string errorMsg = Validator.validateDirPath(txtSource.Text.Trim());
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return;
            }

            reloadProfile();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UISyncJobEntry p = listAllSyncJobs.SelectedItem as UISyncJobEntry;
                SyncJob editingJob = (new SQLiteSyncJobManager(STARTUP_PATH)).GetProfileByName(p.JobName);
                SyncJobManagementWindow syncJobManagementWindow = new SyncJobManagementWindow(editingJob, profileManager);
                syncJobManagementWindow.Owner = this;
                syncJobManagementWindow.ShowDialog();
                reloadProfile();
            }
            catch (Exception)
            {
                //Do nothing.
            }
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

            IList<SyncJob> jobsToBeSynced = new List<SyncJob>();
            foreach (UISyncJobEntry p in listAllSyncJobs.SelectedItems)
            {
                jobsToBeSynced.Add((new SQLiteSyncJobManager(STARTUP_PATH)).GetProfileByName(p.JobName));                
            }
            Synchronize(jobsToBeSynced);
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
            reloadProfile();

            // Show log will be shown again after sync is complete.
            txtBlkShowLog.Visibility = Visibility.Hidden;

            // Animate to start screen
            Storyboard sb = (Storyboard)Window.Resources["sbHome"];
            sb.Begin(this);
        }
        		
		
		/*========================================================
		PART 3: UI handling code
		========================================================*/

        private void reloadProfile()
        {
            //Reload the profile combobox.
            string syncSourceDir = txtSource.Text.Trim();

            uiSyncJob.SyncJobEntries.Clear();
            try
            {
                IList<SyncJob> profileItemsCollection = SyncClient.GetSyncJobManager(STARTUP_PATH).LoadAllJobs();
                //System.Windows.Forms.MessageBox.Show(profileItemsCollection.Count.ToString());
                foreach (SyncJob profileItem in profileItemsCollection)
                {
                    if (profileItem.SyncSource.Path.Equals(syncSourceDir))
                        uiSyncJob.Add(profileItem.Name, profileItem.SyncSource.Path, profileItem.IntermediaryStorage.Path);
                    else if(syncSourceDir.Length == 0)
                        uiSyncJob.Add(profileItem.Name, profileItem.SyncSource.Path, profileItem.IntermediaryStorage.Path);
                }
                uiSyncJob.Add("Testing 1", "Testing Path 11", "Testing Path 12");
                uiSyncJob.Add("Testing 2", "Testing Path 21", "Testing Path 22");
                listAllSyncJobs.ItemsSource = uiSyncJob.SyncJobEntries;
                if(profileItemsCollection.Count > 0)
                    listAllSyncJobs.SelectedIndex = 0;
            }
            catch (Exception)
            {
                showErrorMsg("Error loading profiles.");
            }
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

        private void Synchronize(IList<SyncJob> syncJobs)
        {
            foreach(SyncJob syncJob in syncJobs)
            {
                syncAgent = new FileSyncAgent(syncJob);

                syncAgent.ProgressChanged += new SyncProgressChangedHandler(currAgent_ProgressChanged);
                syncAgent.StageChanged += new SyncStageChangedHandler(currAgent_StatusChanged);
                syncAgent.SyncCompleted += new SyncCompletedHandler(currAgent_SyncCompleted);
                syncAgent.SyncFileChanged += new SyncFileChangedHandler(currAgent_FileChanged);
                syncAgent.SyncStatusChanged += new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);

                syncAgents.Add(syncAgent);
                
            }

            UpdateSyncUI(true, true);

            syncWorker.RunWorkerAsync(syncAgents);
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IList<FileSyncAgent> agents = e.Argument as IList<FileSyncAgent>;
            if (agents == null) return;

            FileSyncAgent agent = agents[0];
            SyncPreviewResult previewResult = agent.GenerateSyncPreview();
            agent.Synchronize(previewResult);
            /*
            foreach (FileSyncAgent syncAgent in agents)
            {
                try
                {
                    SyncPreviewResult previewResult = syncAgent.GenerateSyncPreview();
                    syncAgent.Synchronize(previewResult);
                    
                }
                catch (Exception)
                {
                    showErrorMsg("Synchronization cannot be done. Please inform the administrator.");
                }
            }*/
            e.Result = agents;
        }

        void syncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // exception thrown during sync
                // Log error
                // Notify user
            }

            IList<FileSyncAgent> syncAgents = e.Result as IList<FileSyncAgent>;

            
            if (syncAgents.Count > 0 && syncAgents[0] != null)
            {
                syncAgent = syncAgents[0];

                syncAgent.ProgressChanged -= new SyncProgressChangedHandler(currAgent_ProgressChanged);
                syncAgent.StageChanged -= new SyncStageChangedHandler(currAgent_StatusChanged);
                syncAgent.SyncCompleted -= new SyncCompletedHandler(currAgent_SyncCompleted);
                syncAgent.SyncFileChanged -= new SyncFileChangedHandler(currAgent_FileChanged);
                syncAgent.SyncStatusChanged -= new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);

                syncAgent = null;

                // remove completed sync job
                syncAgents.RemoveAt(0);
            }

            // Check if there are more sync jobs to be run
            if (syncAgents.Count > 0)
                syncWorker.RunWorkerAsync(syncAgents);
            else
                // Update UI and hide sync progress controls
                UpdateSyncUI(false, true);
        }

        #endregion

        #region Sync Preview

        void previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = syncAgent.GenerateSyncPreview();
        }

        void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                SyncPreviewResult syncResult = (SyncPreviewResult)e.Result;

                
            }
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