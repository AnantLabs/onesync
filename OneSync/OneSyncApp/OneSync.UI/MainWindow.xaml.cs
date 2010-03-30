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
using System.Windows.Input;
using System.Windows.Media.Animation;
using OneSync.Synchronization;
using System.Collections.ObjectModel;

namespace OneSync.UI
{

	public partial class MainWindow : Window
	{
		// Start: Global variables.        
        private string syncDir = ""; //The directory of the chosen source folder.
        private string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;
        
        private SyncJobManager jobManager;
        private BackgroundWorker syncWorker;
        private BackgroundWorker previewWorker;

        private ObservableCollection<UISyncJobEntry> _SyncJobEntries = new ObservableCollection<UISyncJobEntry>();
        // End: Global variables.
		
		
		/*========================================================
		PART 1: Home Screen
		========================================================*/
		
		public MainWindow()
		{
            this.InitializeComponent();

            // Get current synchronization directory from command line arguments.
            // Default sync directory is current directory of app.
            string[] args = System.Environment.GetCommandLineArgs();

            jobManager = SyncClient.GetSyncJobManager(STARTUP_PATH);

            if (args.Length > 1 && Validator.validateDirPath(args[1]) == null)
            {
                txtSource.Text = args[1];
                reloadSyncJobs();
            }

            // Tag each browse button to corr TextBox
            btnBrowse_Source.Tag = txtSource;
            btnBrowse.Tag = txtIntStorage;

            // Initialize syncWorker 
            syncWorker = new BackgroundWorker();
            syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
            syncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncWorker_RunWorkerCompleted);

            // Initialize previewWorker
            previewWorker = new BackgroundWorker();
            previewWorker.DoWork += new DoWorkEventHandler(previewWorker_DoWork);
            previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewWorker_RunWorkerCompleted);

            // Set-up data bindings
            listAllSyncJobs.ItemsSource = this.SyncJobEntries;
			
			//Do the sync button rotating here.
			Storyboard sb = (Storyboard)Window.Resources["sbRotateSync"];
			sb.Begin(this);
        }

        private void txtBlkProceed_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg("");
            try
            {
                //Show the info of the first Sync Job first.
                //The rest will be updated later during the sync.
                UISyncJobEntry p = listAllSyncJobs.SelectedItems[0] as UISyncJobEntry;

                UpdateSyncInfoUI(p);
                reloadSyncJobs();

                Window.Title = p.JobName + " - OneSync";
                lblJobsNumber.Content = listAllSyncJobs.SelectedItems.Count.ToString() + "/" + listAllSyncJobs.SelectedItems.Count.ToString();
                UpdateSyncUI(false, false);
                ((Storyboard)Resources["sbNext"]).Begin(this);
            }
            catch (Exception)
            {
                showErrorMsg("Please select an existing sync job.");
            }
        }
		
		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox tb = ((Button)sender).Tag as TextBox;
            if (tb == null) return;

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb.Text = fbd.SelectedPath;
                tb.Focus();
                tb.Select(tb.Text.Length, 0);
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
                SyncJob p = jobManager.CreateSyncJob(syncJobName, syncSourceDir, intStorageDir);

                //Reload the list of sync jobs.
                reloadSyncJobs();
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UISyncJobEntry p = listAllSyncJobs.SelectedItem as UISyncJobEntry;
                SyncJob editingJob = jobManager.Load(p.JobName);
                SyncJobManagementWindow syncJobManagementWindow = new SyncJobManagementWindow(editingJob, jobManager);
                syncJobManagementWindow.Owner = this;
                syncJobManagementWindow.ShowDialog();
                reloadSyncJobs();
            }
            catch (Exception)
            {
                //Do nothing.
            }
        }
		
		/*========================================================
		PART 2: Sync Screen
		========================================================*/


        private void btnSyncStatic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); // clear error msg

            IList<SyncJob> selectedJobs = new List<SyncJob>();
            foreach (UISyncJobEntry j in listAllSyncJobs.SelectedItems)
                selectedJobs.Add(j.SyncJob);

            Synchronize(selectedJobs);
        }

        private void txtBlkShowLog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //View log file (The extension of the file should be .html).
            Process.Start(Log.returnLogReportPath(syncDir, false));
        }

        private void txtBlkBackToHome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); //Empty the notification message (if any).

            Window.Title = "OneSync"; //Change back the menu title.

            //Import all the previous created existing sync job profiles.
            //Note that only the profile having the directory which is the current directory will be loaded.
            reloadSyncJobs();

            // Show log will be shown again after sync is complete.
            txtBlkShowLog.Visibility = Visibility.Hidden;

            // Animate to start screen
            Storyboard sb = (Storyboard)Window.Resources["sbHome"];
            sb.Begin(this);
        }
        		
		
		/*========================================================
		PART 3: UI handling code
		========================================================*/

        // There are 3 states for visiblity of controls in CanvasSync:
        // 1. syncInProgress = false, syncCompletedBefore = false
        // 2. syncInProgress = true, syncCompletedBefore = true
        // 2. syncInProgress = false, syncCompletedBefore = true
        private void UpdateSyncUI(bool syncInProgress, bool showProgressControls)
        {
            // Set Visibility of common controls
            txtBlkBackToHome.IsEnabled = !syncInProgress;
            btnSyncStatic.IsEnabled = !syncInProgress;
            txtIntStorage.IsEnabled = !syncInProgress;
            btnBrowse.IsEnabled = !syncInProgress;

            if (syncInProgress)
            {
                btnSyncRotating.Visibility = Visibility.Visible;
                btnSyncStatic.Visibility = Visibility.Hidden;
                //sb.Begin(this);
            }
            else
            {
                btnSyncRotating.Visibility = Visibility.Hidden;
                btnSyncStatic.Visibility = Visibility.Visible;
                //sb.Stop();
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

        private void UpdateSyncInfoUI(UISyncJobEntry p)
        {
            lblSyncJobName.Content = normalizeString(p.JobName, 20, 50);
            lblSyncJobName.ToolTip = p.JobName;
            lblSyncJobSource.Content = normalizeString(p.SyncSource, 20, 30);
            lblSyncJobSource.ToolTip = p.SyncSource;
            lblSyncJobStorage.Content = normalizeString(p.IntStorage, 20, 30);
            lblSyncJobStorage.ToolTip = p.IntStorage;
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
            if (syncJobs.Count < 0) return;

            // Create a list of SyncJobs
            List<FileSyncAgent> agents = new List<FileSyncAgent>(syncJobs.Count);
            foreach (SyncJob job in syncJobs)
                agents.Add(new FileSyncAgent(job));

            UpdateSyncUI(true, true);
            syncWorker.RunWorkerAsync(agents);
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IList<FileSyncAgent> agents = e.Argument as IList<FileSyncAgent>;
            if (agents == null || agents.Count < 0) return;

            // Get first sync agent to run
            FileSyncAgent agent = agents[0];

            AddAgentEventHandler(agent);
            
            // Note: SyncActions could have been generated during preview
            if (agent.SyncJob.SyncActions == null)
                agent.SyncJob.SyncActions = agent.GenerateSyncPreview().GetAllActions();

            // TODO: [THUAT] uncomment for sync to work
            //agent.Synchronize(agent.SyncJob.SyncActions);

            e.Result = agents;
        }

        void syncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // exception thrown during sync
                // Log error
                // Notify user
                return;
            }

            IList<FileSyncAgent> agents = e.Result as IList<FileSyncAgent>;
            if (agents == null) return;

            if (agents.Count > 0 && agents[0] != null)
            {
                FileSyncAgent agent = agents[0];
                RemoveAgentEventHandler(agent);
                agents.RemoveAt(0);
                lblJobsNumber.Content = agents.Count.ToString() + "/" + listAllSyncJobs.SelectedItems.Count.ToString();
            }

            // Check for more sync agents to run
            if (agents.Count > 0)
                syncWorker.RunWorkerAsync(agents);
            else
                // Update UI and hide sync progress controls
                UpdateSyncUI(false, true);
        }

        private void AddAgentEventHandler(FileSyncAgent agent)
        {
            agent.ProgressChanged += new SyncProgressChangedHandler(currAgent_ProgressChanged);
            agent.SyncCompleted += new SyncCompletedHandler(currAgent_SyncCompleted);
            agent.SyncFileChanged += new SyncFileChangedHandler(currAgent_FileChanged);
            agent.StatusChanged += new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);
        }

        private void RemoveAgentEventHandler(FileSyncAgent agent)
        {
            agent.ProgressChanged -= new SyncProgressChangedHandler(currAgent_ProgressChanged);
            agent.SyncCompleted -= new SyncCompletedHandler(currAgent_SyncCompleted);
            agent.SyncFileChanged -= new SyncFileChangedHandler(currAgent_FileChanged);
            agent.StatusChanged -= new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);
        }

        #endregion

        #region Sync Preview

        private void PreviewSyncJob(object sender, MouseButtonEventArgs e)
        {
            UISyncJobEntry jobEntry = listAllSyncJobs.SelectedItem as UISyncJobEntry;
            if (jobEntry == null) return;

            SyncJob job = jobEntry.SyncJob;
            if (job == null) return;

            if (job.SyncActions != null) /* Job has been previewed before */
            {
                MessageBoxResult result = MessageBox.Show(
                    "This Sync Job has been previewed before. Do you want to re-generate the actions for preview?",
                    "Preview", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                    previewWorker.RunWorkerAsync(jobEntry.SyncJob);
                else
                    new WinSyncPreview(job.SyncActions).ShowDialog();
            }
            else
                previewWorker.RunWorkerAsync(jobEntry.SyncJob);
        }

        void previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SyncJob job = e.Result as SyncJob;
            if (job == null) return;

            FileSyncAgent agent = new FileSyncAgent(job);
            SyncPreviewResult result = agent.GenerateSyncPreview();

            // Save sync actions so it will not be generated again for this job
            job.SyncActions = result.GetAllActions();
            e.Result = job;
        }

        void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                // Log error
                return;
            }

            SyncJob job = e.Result as SyncJob;
            if (job == null) return;

            WinSyncPreview winPreview = new WinSyncPreview(job.SyncActions);
            winPreview.ShowDialog();
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
                pbSync.Dispatcher.Invoke((Action)delegate { currAgent_ProgressChanged(sender, e); });
        }

        /// <summary>
        /// Update the status message.
        /// </summary>
        void currAgent_StatusChanged(object sender, SyncStatusChangedEventArgs e)
        {
            //Display some text so that the user knows that what OneSync is doing during the synchronization.
            if (this.Dispatcher.CheckAccess())
            {
                lblStatus.Content = e.Message;
            }
            else
                lblStatus.Dispatcher.Invoke((Action)delegate { currAgent_StatusChanged(sender, e); });
        }

        /// <summary>
        /// Tell the user which file is being processed now.
        /// </summary>
        void currAgent_FileChanged(object sender, Synchronization.SyncFileChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                lblStatus.Content = "Synchronizing: " + e.RelativePath;
            else
                lblStatus.Dispatcher.Invoke((Action)delegate { currAgent_FileChanged(sender, e); });
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
                this.Dispatcher.Invoke((Action)delegate { currAgent_SyncCompleted(sender, e); });
            }
        }

        void syncAgent_SyncStatusChanged(object sender, SyncStatusChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                lblStatus.Content = e.Message;
            else
                this.Dispatcher.Invoke((Action)delegate { syncAgent_SyncStatusChanged(sender, e); });
        }

        #endregion		


        private void reloadSyncJobs()
        {
            string syncSourceDir = txtSource.Text.Trim();

            this.SyncJobEntries.Clear();
            try
            {
                IList<SyncJob> jobs = jobManager.LoadAllJobs();

                foreach (SyncJob job in jobs)
                {
                    if (job.SyncSource.Path.Equals(syncSourceDir))
                        this.SyncJobEntries.Add(new UISyncJobEntry(job));
                    else if (syncSourceDir.Length == 0)
                        this.SyncJobEntries.Add(new UISyncJobEntry(job)); /* Add all jobs */
                }

                // Select job if there is only 1 job available
                if (jobs.Count == 1)
                    listAllSyncJobs.SelectedIndex = 0;
            }
            catch (Exception)
            {
                showErrorMsg("Error loading profiles.");
            }
        }

        public ObservableCollection<UISyncJobEntry> SyncJobEntries
        {
            get { return _SyncJobEntries; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (syncWorker.IsBusy)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to close the program now? The synchronization is still ongoing.",
                    "Goodbye OneSync", MessageBoxButton.YesNo, MessageBoxImage.Question);

                e.Cancel = (result == MessageBoxResult.No);
            }
        }

        private string normalizeString(string str, int prefixLength, int suffixLength)
        {
            if (str.Length > prefixLength + suffixLength)
                return str.Substring(0, prefixLength) + "..." + str.Substring(str.Length - suffixLength);
            else
                return str;
        }    

	}
}