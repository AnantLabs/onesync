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
using System.Windows.Threading;

namespace OneSync.UI
{

	public partial class MainWindow : Window
	{
		// Start: Global variables.        
        private string syncDir = ""; //The directory of the chosen source folder.
        private string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;
        
        private SyncJobManager jobManager;
        private BackgroundWorker syncWorker;

        private DispatcherTimer timerDropbox; //The time to check the Dropbox status frequently.

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
            }

            // Tag each browse button to corr TextBox
            btnBrowse_Source.Tag = txtSource;
            btnBrowse.Tag = txtIntStorage;

            // Initialize syncWorker 
            syncWorker = new BackgroundWorker();
            syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
            syncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncWorker_RunWorkerCompleted);

            // Starting the timer to check the Dropbox status
            timerDropbox = new DispatcherTimer();
            timerDropbox.Tick += new EventHandler(delegate(object s, EventArgs e)
                {
                    dropboxStatusChecking();
                });
            timerDropbox.Interval = TimeSpan.FromMilliseconds(10000);
            timerDropbox.Start();

            // Set-up data bindings
            listAllSyncJobs.ItemsSource = this.SyncJobEntries;

            LoadSyncJobs();
        }

        private void dropboxStatusChecking()
        {
            foreach (UISyncJobEntry entry in SyncJobEntries)
            {
                entry.InfoChanged();
            }
        }             

        private void txtBlkProceed_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg("");
            IList<UISyncJobEntry> selectedJobs = UISyncJobEntry.GetSelectedJobs(SyncJobEntries);

            if (selectedJobs.Count == 0)
            {
                showErrorMsg("Please select at least one sync job");
                return;
            }

            // Update Sync UI info
            Window.Title = selectedJobs[0].JobName + " - OneSync";
            
            lblSyncJobName.Content = selectedJobs[0].JobName;
            lblSyncJobName.ToolTip = selectedJobs[0].JobName;
            lblSyncJobSource.Content = normalizeString(selectedJobs[0].SyncSource, 20, 20);
            lblSyncJobSource.ToolTip = selectedJobs[0].SyncSource;
            lblSyncJobStorage.Content = normalizeString(selectedJobs[0].IntermediaryStoragePath, 20, 20);
            lblSyncJobStorage.ToolTip = selectedJobs[0].IntermediaryStoragePath;
            lblJobsNumber.Content = selectedJobs.Count.ToString();

            UpdateSyncUI(false, false);
            ((Storyboard)Resources["sbNext"]).Begin(this, true);
        }

		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox tb = ((Button)sender).Tag as TextBox;
            if (tb == null) return;

            try
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tb.Text = fbd.SelectedPath;
                    tb.Focus();
                    tb.Select(tb.Text.Length, 0);
                }
            }
            catch (Exception)
            {
                showErrorMsg("The selected folder path is invalid.");
            }
        }

        private void txtBlkNewJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            showErrorMsg("");
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
                SyncJob job = jobManager.CreateSyncJob(syncJobName, syncSourceDir, intStorageDir);
                SyncJobEntries.Add(new UISyncJobEntry(job));
            }
            catch (ProfileNameExistException)
            {
                showErrorMsg("A sync job with the same name already exists.");
                return;
            }
            catch (Exception ex)
            {
                showErrorMsg(ex.Message);
                return;
            }
        }

        private void textBoxEdit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock clickedBlock = (TextBlock)e.Source;
                UISyncJobEntry entry  = (UISyncJobEntry) clickedBlock.DataContext;                             
                SyncJobManagementWindow syncJobManagementWindow = new SyncJobManagementWindow(entry, jobManager);
                syncJobManagementWindow.ShowDialog();
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
            foreach (UISyncJobEntry entry in SyncJobEntries) if ( entry.IsSelected) selectedJobs.Add(entry.SyncJob);
            Synchronize(selectedJobs);
        }

        private void txtBlkShowLog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); //Clear error msg

            try
            {
                TextBlock clickedBlock = (TextBlock)e.Source;
                UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;

                //View log file (The extension of the file should be .html).
                if (File.Exists(Log.ReturnLogReportPath(entry.SyncSource)))
                {
                    Log.ShowLog(entry.SyncSource);
                }
                else
                {
                    showErrorMsg("There is no log for this job currently.");
                }
            }
            catch (Exception)
            {
                //Do nothing.
            }
        }

        private void txtBlkBackToHome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); //Empty the notification message (if any).

            Window.Title = "OneSync"; //Change back the menu title.

            // Clear any sync preview results previously
            ClearPreviewResults();

            // Animate to start screen
            Storyboard sb = (Storyboard)Window.Resources["sbHome"];
            sb.Begin(this);
        }

        private void ClearPreviewResults()
        {
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.SyncJob.SyncPreviewResult = null;
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
            }
            else
            {
                btnSyncRotating.Visibility = Visibility.Hidden;
                btnSyncStatic.Visibility = Visibility.Visible;
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
            lblSyncJobStorage.Content = normalizeString(p.IntermediaryStoragePath, 20, 30);
            lblSyncJobStorage.ToolTip = p.IntermediaryStoragePath;
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
            
            // TODO: [THUAT] uncomment for sync to work

            if (agent.SyncJob.SyncPreviewResult == null)
                agent.SyncJob.SyncPreviewResult = agent.GenerateSyncPreview();
            agent.Synchronize(agent.SyncJob.SyncPreviewResult);
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
                agent.SyncJob.SyncPreviewResult = null;
                RemoveAgentEventHandler(agent);
                agents.RemoveAt(0);
            }

            // Check for more sync agents to run
            if (agents.Count > 0)
            {
                Window.Title = agents[0].SyncJob.Name + " - OneSync";
                lblSyncJobName.Content = agents[0].SyncJob.Name;
                lblSyncJobName.ToolTip = agents[0].SyncJob.Name;
                lblSyncJobSource.Content = normalizeString(agents[0].SyncJob.SyncSource.Path, 20, 20);
                lblSyncJobSource.ToolTip = agents[0].SyncJob.SyncSource.Path;
                lblSyncJobStorage.Content = normalizeString(agents[0].SyncJob.IntermediaryStorage.Path, 20, 20);
                lblSyncJobStorage.ToolTip = agents[0].SyncJob.IntermediaryStorage.Path;
                lblJobsNumber.Content = agents.Count.ToString();
                syncWorker.RunWorkerAsync(agents);
            }
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

        private void LoadSyncJobs()
        {
            string syncSourceDir = txtSource.Text.Trim();                 
            try
            {
                IList<SyncJob> jobs = jobManager.LoadAllJobs();

                SyncJobEntries.Clear();
                foreach (SyncJob job in jobs)
                    SyncJobEntries.Add(new UISyncJobEntry(job));
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

        private void checkBoxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            CheckSyncJobs(true);
        }

        private void checkBoxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckSyncJobs(false);
        }

        private void CheckSyncJobs(bool selectAll)
        {
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.IsSelected = selectAll;
        }

        private void textBoxPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedBlock = (TextBlock)e.Source;
            UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;
            if (entry == null || entry.SyncJob == null)
            {
                showErrorMsg("Couldn't find selected sync job(s)");
                return;
            }

            WinSyncPreview winPreview = new WinSyncPreview(entry.SyncJob);
            winPreview.ShowDialog();
        }

        private void selectSyncCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = e.Source as CheckBox;
            UISyncJobEntry entry = checkBox.DataContext as UISyncJobEntry;
            entry.IsSelected = true;
        }

        private void selectSyncCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = e.Source as CheckBox;
            UISyncJobEntry entry = checkBox.DataContext as UISyncJobEntry;
            entry.IsSelected = false ;
        }    

	}
}
