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

        private UISyncJobEntry currentSyncJob = null;

        //private DispatcherTimer timerDropbox; //The time to check the Dropbox status frequently.

        private ObservableCollection<UISyncJobEntry> _SyncJobEntries = new ObservableCollection<UISyncJobEntry>();

        TaskbarManager tbManager = TaskbarManager.Instance;
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
                txtSource.Focus();
                txtSource.Select(txtSource.Text.Length, 0);
            }

            // Tag each browse button to corr TextBox
            btnBrowse_Source.Tag = txtSource;
            btnBrowse.Tag = txtIntStorage;

            // Initialize syncWorker 
            syncWorker = new BackgroundWorker();
            syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
            syncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncWorker_RunWorkerCompleted);

            /*
            // Starting the timer to check the Dropbox status
            timerDropbox = new DispatcherTimer();
            timerDropbox.Tick += new EventHandler(delegate(object s, EventArgs e)
                {
                    dropboxStatusChecking();
                });
            timerDropbox.Interval = TimeSpan.FromMilliseconds(10000);
            timerDropbox.Start();
             */

            // Set-up data bindings
            listAllSyncJobs.ItemsSource = this.SyncJobEntries;
            LoadSyncJobs();
        }

        /*
        private void dropboxStatusChecking()
        {
            foreach (UISyncJobEntry entry in SyncJobEntries)
            {
                entry.InfoChanged();
            }
        }*/            

        private void txtBlkProceed_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SyncSelectedJobs();
        }

        private void btnSyncStatic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SyncSelectedJobs();
        }

        private void SyncSelectedJobs()
        {
            showErrorMsg("");
            IList<UISyncJobEntry> selectedJobs = UISyncJobEntry.GetSelectedJobs(SyncJobEntries);

            if (selectedJobs.Count == 0)
            {
                showErrorMsg("Please select at least one sync job");
                return;
            }

            lblJobsNumber.Content = selectedJobs.Count;

            try
            {
                foreach(UISyncJobEntry uiSyncJobEntry in selectedJobs)
                {
                    uiSyncJobEntry.ProgressBarVisibility = Visibility.Hidden;
                    uiSyncJobEntry.InfoChanged();
                }
                Synchronize(selectedJobs);
                UpdateSyncInfoUI(selectedJobs[0].SyncJob);
                UpdateSyncUI(true);
            }
            catch (Exception)
            {
                //Do nothing.
            }

            //if (tbManager != null) tbManager.SetProgressState(TaskbarProgressBarState.Normal);

            //((Storyboard)Resources["sbNext"]).Begin(this, true);
        }

		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
        {           
            try
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
                SyncJobEntries[SyncJobEntries.Count - 1].Order = SyncJobEntries.Count;
                SyncJobEntries[SyncJobEntries.Count - 1].IsSelected = true;
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
                syncJobManagementWindow.Owner = this;
                syncJobManagementWindow.ShowDialog();

                // DialogResult will be true if SyncJob is deleted
                if (syncJobManagementWindow.DialogResult.HasValue && syncJobManagementWindow.DialogResult.Value)
                    this.SyncJobEntries.Remove(entry);


            }
            catch (Exception)
            {
                //Do nothing.
            }
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

        private void ClearPreviewResults()
        {
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.SyncJob.SyncPreviewResult = null;
        }

        /*========================================================
        PART 2: Sync Screen
        ========================================================*/
		
		/*========================================================
		PART 3: UI handling code
		========================================================*/

        private void UpdateSyncUI(bool syncInProgress)
        {
            // Set Visibility of common controls
            btnSyncStatic.IsEnabled = !syncInProgress;
            txtIntStorage.IsEnabled = !syncInProgress;
            btnBrowse.IsEnabled = !syncInProgress;

            if (syncInProgress)
            {
                btnSyncRotating.Visibility = Visibility.Visible;
                btnSyncStatic.Visibility = Visibility.Hidden;
                lbl_description.Visibility = Visibility.Visible;
            }
            else
            {
                btnSyncRotating.Visibility = Visibility.Hidden;
                btnSyncStatic.Visibility = Visibility.Visible;
            }
        }

        private void UpdateSyncInfoUI(SyncJob p)
        {
            // Update Sync UI info
            lblSyncJobName.Content = p.Name;
            lblSyncJobName.ToolTip = p.Name;
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

        private void Synchronize(IList<UISyncJobEntry> selectedSyncJobs)
        {
            try
            {
                if (selectedSyncJobs.Count < 0) return;

                // Create a list of SyncJobs
                List<FileSyncAgent> agents = new List<FileSyncAgent>(selectedSyncJobs.Count);
                foreach (UISyncJobEntry uiSyncJob in selectedSyncJobs)
                    agents.Add(new FileSyncAgent(uiSyncJob.SyncJob));

                //UpdateSyncUI(true, true);

                SyncWorkerArguments arguments = new SyncWorkerArguments(selectedSyncJobs, agents);

                syncWorker.RunWorkerAsync(arguments);
            }
            catch (Exception)
            {
                //Do nothing.
            }            
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {            
            try
            {
                //IList<FileSyncAgent> agents = e.Argument as IList<FileSyncAgent>;
                //if (agents == null || agents.Count < 0) return;
                SyncWorkerArguments arguments = e.Argument as SyncWorkerArguments;
                if (arguments == null) return;

                if (arguments.Agents[0] == null) return;

                //Renew the currentSyncJob variable.
                currentSyncJob = arguments.SelectedSyncJobs[0];

                currentSyncJob.ProgressBarVisibility = Visibility.Visible;
                currentSyncJob.ProgressBarColor = "Green";
                currentSyncJob.ProgressBarMessage = "Syncing...";
                currentSyncJob.InfoChanged();

                AddAgentEventHandler(arguments.Agents[0]);

                try
                {
                    if (arguments.Agents[0].SyncJob.SyncPreviewResult == null)
                        arguments.Agents[0].SyncJob.SyncPreviewResult = arguments.Agents[0].GenerateSyncPreview();
                    arguments.Agents[0].Synchronize(arguments.Agents[0].SyncJob.SyncPreviewResult);
                }
                catch (Community.CsharpSqlite.SQLiteClient.SqliteSyntaxException syntax)
                {
                    currentSyncJob.ProgressBarColor = "Red";
                    currentSyncJob.ProgressBarMessage = "Error: The intermediate storage not found or data.md is missing in the intermediate storage";
                    currentSyncJob.InfoChanged();
                }
                catch (Exception)
                {
                    currentSyncJob.ProgressBarColor = "Red";
                    currentSyncJob.ProgressBarMessage = "Unknown error occurred during the sync process";
                    currentSyncJob.InfoChanged();
                }
                e.Result = arguments;
            }
            catch (Exception ex)
            {            
                
            }
        }

        void syncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    // exception thrown during sync
                    // Log error
                    // Notify user
                    return;
                }

                //IList<FileSyncAgent> agents = e.Result as IList<FileSyncAgent>;
                //if (agents == null) return;
                SyncWorkerArguments arguments = e.Result as SyncWorkerArguments;
                if (arguments == null) return;
                if (arguments.Agents == null) return;

                if (arguments.Agents.Count > 0 && arguments.Agents[0] != null)
                {
                    currentSyncJob.ProgressBarMessage = "Sync completed";
                    currentSyncJob.InfoChanged();

                    //Remove the synced agent and sync job.
                    arguments.Agents[0].SyncJob.SyncPreviewResult = null;
                    RemoveAgentEventHandler(arguments.Agents[0]);
                    arguments.Agents.RemoveAt(0);
                    arguments.SelectedSyncJobs.RemoveAt(0);
                }

                // Check for more sync jobs to run
                if (arguments.Agents.Count > 0)
                {
                    UpdateSyncInfoUI(arguments.Agents[0].SyncJob);
                    syncWorker.RunWorkerAsync(arguments);
                }
                else
                {
                    currentSyncJob = null;
                    // Update UI and hide sync progress controls
                    UpdateSyncUI(false);
                    lblStatus.Content = "Synchronization of all jobs is successfully done.";
                    lblSubStatus.Content = "";
                }
            }
            catch (Exception)
            {
            }
            
        }

        private void AddAgentEventHandler(FileSyncAgent agent)
        {
            if (agent == null) return;
            agent.ProgressChanged += new SyncProgressChangedHandler(currAgent_ProgressChanged);
            agent.SyncCompleted += new SyncCompletedHandler(currAgent_SyncCompleted);
            agent.SyncFileChanged += new SyncFileChangedHandler(currAgent_FileChanged);
            agent.StatusChanged += new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);
        }

        private void RemoveAgentEventHandler(FileSyncAgent agent)
        {
            if (agent == null) return;
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
            currentSyncJob.ProgressBarValue = e.Value;
            currentSyncJob.InfoChanged();
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
                lblSubStatus.Content = "Synchronizing: " + e.RelativePath;
            else
                lblSubStatus.Dispatcher.Invoke((Action)delegate { currAgent_FileChanged(sender, e); });
        }

        /// <summary>
        /// Trigerred when the sync process is done.
        /// </summary>
        void currAgent_SyncCompleted(object sender, Synchronization.SyncCompletedEventArgs e)
        {
            
            if (this.Dispatcher.CheckAccess())
            {
                lblStatus.Content = "Synchronization of " + currentSyncJob.JobName + " is successfully done.";
                lblSubStatus.Content = "";
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

        public void LoadSyncJobs()
        {
            string syncSourceDir = txtSource.Text.Trim();                 
            try
            {
                IList<SyncJob> jobs = jobManager.LoadAllJobs();

                SyncJobEntries.Clear();
                int i = 1;
                foreach (SyncJob job in jobs)
                {
                    SyncJobEntries.Add(new UISyncJobEntry(job));
                    SyncJobEntries[SyncJobEntries.Count - 1].Order = i;
                    i++;
                }
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

        private void TextBlock_MouseDown_Plus(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedBlock = (TextBlock)e.Source;
            UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;
            if (entry == null || entry.SyncJob == null)
            {
                showErrorMsg("Couldn't find selected sync job(s)");
                return;
            }

            if (entry.Order > 1)
                entry.Order -= 1;

            OrderSyncJobs(entry.JobId, true, entry.Order);
        }

        private void TextBlock_MouseDown_Minus(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedBlock = (TextBlock)e.Source;
            UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;
            if (entry == null || entry.SyncJob == null)
            {
                showErrorMsg("Couldn't find selected sync job(s)");
                return;
            }

            if (entry.Order < SyncJobEntries.Count)
                entry.Order += 1;

            OrderSyncJobs(entry.JobId, false, entry.Order);
        }

        private void OrderSyncJobs(string jobId, bool isPlus, int order)
        {
            ObservableCollection<UISyncJobEntry> tempSyncJobEntries = new ObservableCollection<UISyncJobEntry>();
            foreach (UISyncJobEntry entry in SyncJobEntries)
            {
                tempSyncJobEntries.Add(entry);
            }

            //Do the reordering here.
            foreach (UISyncJobEntry entry in tempSyncJobEntries)
            {
                if (isPlus)
                {
                    if (entry.Order == order && !entry.JobId.Equals(jobId))
                        entry.Order++;
                }
                else
                {
                    if (entry.Order == order && !entry.JobId.Equals(jobId))
                        entry.Order--;
                }
            }

            //Do the reloading here according to the new order.
            try
            {
                SyncJobEntries.Clear();
                for (int i = 1; i < tempSyncJobEntries.Count + 1; i++)
                {
                    foreach (UISyncJobEntry entry in tempSyncJobEntries)
                    {
                        if (entry.Order == i)
                            SyncJobEntries.Add(entry);
                    }
                }
            }
            catch (Exception)
            {
                showErrorMsg("Error loading profiles.");
            }
        }


        #region Drag-Drop

        private void txtDir_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] folders = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (folders == null) return;

            if (folders.Length > 0)
            {
                TextBox tb = sender as TextBox;
                if (tb == null) return;

                if (Directory.Exists(folders[0]))
                    tb.Text = folders[0];
            }

            e.Handled = true;
        }

        private void txtDir_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
        }

        private void txtDir_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        #endregion

        
	}
}
