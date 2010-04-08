/*
 $Id: MainWindow.xaml.cs 287 2010-03-18 02:25:56Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OneSync.Synchronization;
using Community.CsharpSqlite.SQLiteClient;
using System.Windows.Data;

namespace OneSync.UI
{

	public partial class MainWindow : Window
	{
		// Start: Global variables.        
        private string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;
        
        private SyncJobManager jobManager;
        private BackgroundWorker syncWorker;
        private UISyncJobEntry currentJobEntry;

        TextBox editTextBox;

        //private DispatcherTimer timerDropbox; //The time to check the Dropbox status frequently.

        private ObservableCollection<UISyncJobEntry> _SyncJobEntries = new ObservableCollection<UISyncJobEntry>();

        TaskbarManager tbManager = TaskbarManager.Instance;
        // End: Global variables.
		
		
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

        private void sync_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg("");
            Queue<UISyncJobEntry> selectedJobs = UISyncJobEntry.GetSelectedJobs(SyncJobEntries);

            if (selectedJobs.Count == 0)
            {
                showErrorMsg("Please select at least one sync job");
                return;
            }

            lblJobsNumber.Content = selectedJobs.Count;

            // Exit edit-mode of all Job Entry
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.EditMode = false;

            Synchronize(selectedJobs);
            //listAllSyncJobs.IsEnabled = false;
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
                    tb.CaretIndex = tb.Text.Length;
                }
            }
            catch (Exception)
            {
                showErrorMsg("The selected folder path is invalid.");
            }
        }

        void editControls_Loaded(object sender, RoutedEventArgs e)
        {
            // Associate Browse button with corresponding textbox
            if (sender.GetType() == typeof(TextBox))
                editTextBox = (TextBox)sender;
            else
                ((Button)sender).Tag = editTextBox;
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

            if (!validateSyncJobParams(syncJobName, syncSourceDir, intStorageDir))
                return;


            //Create new sync job if all three inputs mentioned above are valid.
            try
            {
                SyncJob job = jobManager.CreateSyncJob(syncJobName, syncSourceDir, intStorageDir);
                UISyncJobEntry entry = new UISyncJobEntry(job) { IsSelected = true };
                SyncJobEntries.Add(entry);
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

        private void imgEdit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateTextBoxBindings();
                
            try
            {
                Image img = (Image)e.Source;
                UISyncJobEntry entry  = (UISyncJobEntry)img.DataContext;

                if (entry.EditMode)
                {
                    // Saving
                    if (saveSyncJob(entry)) entry.EditMode = false;
                }
                else
                    entry.EditMode = true;
            }
            catch (Exception)
            {
                //Do nothing.
            }
        }

        private void imgDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image img = (Image)e.Source;
                UISyncJobEntry entry = (UISyncJobEntry)img.DataContext;

                MessageBoxResult result = MessageBox.Show(
                            "Are you sure you want to delete " + entry.SyncJob.Name + "?", "Delete SyncJob",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No) return;

                if (!jobManager.Delete(entry.SyncJob))
                    showErrorMsg("Unable to delete sync job at this moment.");
                else
                    this.SyncJobEntries.Remove(entry);

                // Try to delete Sync Source table from intermediary storage
                // TODO: thuat handled this?
                SyncSourceProvider syncSourceProvider =
                    SyncClient.GetSyncSourceProvider(entry.IntermediaryStoragePath);
                syncSourceProvider.Delete(entry.SyncJob.SyncSource);
            }
            catch (Exception)
            {

            }
        }

        private bool validateSyncJobParams(string jobName, string syncSource, string intStorage)
        {
            string errorMsg = Validator.validateSyncJobName(jobName);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return false;
            }

            try
            {
                if (!Directory.Exists(intStorage))
                    Directory.CreateDirectory(intStorage);
            }
            catch (Exception)
            {
                showErrorMsg("Unable to create directory " + intStorage);
                return false;
            }
            

            errorMsg = Validator.validateSyncDirs(syncSource, intStorage);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
                return false;
            }

            return true;
        }

        void editJobWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UISyncJobEntry entry = e.Argument as UISyncJobEntry;
            if (entry == null) return;

            string oldSyncJobName = entry.SyncJob.Name;
            string oldIStorage = entry.SyncJob.IntermediaryStorage.Path;
            string oldSyncSource = entry.SyncJob.SyncSource.Path;

            try
            {
                entry.SyncJob.Name = entry.NewJobName;
                entry.SyncJob.IntermediaryStorage.Path = entry.NewIntermediaryStoragePath;
                entry.SyncJob.SyncSource.Path = entry.NewSyncSource;
                jobManager.Update(entry.SyncJob);
                entry.InfoChanged(); /* Force databinding to refresh */

                if (!entry.SyncJob.IntermediaryStorage.Path.Equals(oldIStorage))
                {
                    try
                    {
                        if (!Directory.Exists(oldIStorage) ||
                            !Files.FileUtils.MoveFolder(oldIStorage, entry.SyncJob.IntermediaryStorage.Path))
                            throw new Exception();
                    }
                    catch (Exception) {
                        this.Dispatcher.Invoke((Action)delegate
                        {
                            showErrorMsg("Can't move file(s) to new intermediary storage location. Please do it manually");
                        });
                    }
                }
                e.Result = entry;
            }
            catch (Exception)
            {
                entry.SyncJob.Name = oldSyncJobName;
                entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                entry.SyncJob.SyncSource.Path = oldSyncSource;
                entry.InfoChanged(); /* Force databinding to refresh */
            } 
        }

        void editJobWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UISyncJobEntry entry = e.Result as UISyncJobEntry;
            if (entry == null) return;

            entry.EditMode = false;
            
            SetControlsEnabledState(true);
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
            entry.IsSelected = false;
        }

        private void reorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedBlock = (TextBlock)e.Source;
            UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;
            if (entry == null) return;

            int delta = int.Parse(clickedBlock.Tag.ToString());
            MoveJobEntry(entry, delta);
        }

        private void checkBoxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            selectSyncJobs(true);
        }

        private void checkBoxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            selectSyncJobs(false);
        }

        private void listAllSyncJobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Exit edit-mode of all Job Entry
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.EditMode = false;
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


        #region Synchronization

        private void Synchronize(Queue<UISyncJobEntry> selectedEntries)
        {
            if (selectedEntries.Count <= 0) return;

            // Hide progress bar of selected syncjobs
            foreach (UISyncJobEntry jobEntry in selectedEntries)
                jobEntry.ProgressBarVisibility = Visibility.Hidden;

            try
            {
                UISyncJobEntry currentJobEntry = selectedEntries.Peek();

                // Update UI
                UpdateSyncInfoUI(currentJobEntry.SyncJob);
                UpdateSyncUI(true);
                listAllSyncJobs_SelectionChanged(null, null);
                SetControlsEnabledState(false);

                // Run sync
                syncWorker.RunWorkerAsync(selectedEntries);
            }
            catch (Exception)
            {
                //Do nothing.
            }
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Queue<UISyncJobEntry> jobEntries = e.Argument as Queue<UISyncJobEntry>;
            if (jobEntries == null || jobEntries.Count <= 0) return;

            UISyncJobEntry entry = jobEntries.Peek();
            if (entry == null) return;

            // Keep track of current entry that is being synchronized
            currentJobEntry = entry;

            // Update UI
            entry.ProgressBarColor = "#FF01D328";
            entry.ProgressBarVisibility = Visibility.Visible;
            entry.ProgressBarMessage = "Synchronizing...";

            // Add event handler
            AddAgentEventHandler(entry.SyncAgent);

            try
            {
                if (entry.SyncJob.SyncPreviewResult == null)
                    entry.SyncJob.SyncPreviewResult = entry.SyncAgent.GenerateSyncPreview();

                entry.SyncAgent.Synchronize(entry.SyncJob.SyncPreviewResult);
                entry.Error = null;
            }
            catch (Community.CsharpSqlite.SQLiteClient.SqliteSyntaxException ex)
            {
                entry.Error = ex;
                entry.ProgressBarValue = 100;
                entry.ProgressBarColor = "Red";
                entry.ProgressBarMessage = "Error: The intermediate storage not found or data.md is missing in the intermediate storage";
            }
            catch (Exception ex)
            {
                entry.Error = ex;
                entry.ProgressBarValue = 100;
                entry.ProgressBarColor = "Red";
                entry.ProgressBarMessage = "Unknown error occurred during the sync process";
            }
            e.Result = jobEntries;
        }

        void syncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) return;

                Queue<UISyncJobEntry> jobEntries = e.Result as Queue<UISyncJobEntry>;
                if (jobEntries == null) return;

                if (jobEntries.Count > 0 && jobEntries.Peek() != null)
                {
                    //Remove the synced agent and sync job.
                    UISyncJobEntry completedJobEntry = jobEntries.Dequeue();

                    if (completedJobEntry.Error == null)
                        completedJobEntry.ProgressBarMessage = "Sync completed";

                    completedJobEntry.SyncJob.SyncPreviewResult = null;
                    RemoveAgentEventHandler(completedJobEntry.SyncAgent);
                }

                // Check for more sync jobs to run
                if (jobEntries.Count > 0)
                {
                    UpdateSyncInfoUI(jobEntries.Peek().SyncJob);
                    syncWorker.RunWorkerAsync(jobEntries);
                }
                else
                {
                    // Update UI and hide sync progress controls
                    UpdateSyncUI(false);
                    SetControlsEnabledState(true);
                    lblStatus.Content = "All SyncJobs completed succesfully.";
                    lblSubStatus.Content = "";

                    //listAllSyncJobs.IsEnabled = true;
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
            agent.SyncFileChanged += new SyncFileChangedHandler(currAgent_FileChanged);
            agent.StatusChanged += new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);
        }

        private void RemoveAgentEventHandler(FileSyncAgent agent)
        {
            if (agent == null) return;
            agent.ProgressChanged -= new SyncProgressChangedHandler(currAgent_ProgressChanged);
            agent.SyncFileChanged -= new SyncFileChangedHandler(currAgent_FileChanged);
            agent.StatusChanged -= new SyncStatusChangedHandler(syncAgent_SyncStatusChanged);
        }

        #endregion


        #region UI handling code

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

        #endregion


        #region SyncAgent Event Handling Code

        /// <summary>
        /// Update the progress bar.
        /// </summary>
        void currAgent_ProgressChanged(object sender, Synchronization.SyncProgressChangedEventArgs e)
        {   
            currentJobEntry.ProgressBarValue = e.Value;
            tbManager.SetProgressValue(e.Value, 100);
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
                foreach (SyncJob job in jobs)
                    SyncJobEntries.Add(new UISyncJobEntry(job));
            }
            catch (Exception)
            {
                showErrorMsg("Error loading profiles.");
            }
        }

        private void ClearPreviewResults()
        {
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.SyncJob.SyncPreviewResult = null;
        }

        private void MoveJobEntry(UISyncJobEntry entry, int delta)
        {
            int index = SyncJobEntries.IndexOf(entry);
            SyncJobEntries.RemoveAt(index);

            index += delta;

            // Bound index
            if (index < 0) index = 0;
            if (index > SyncJobEntries.Count) index = SyncJobEntries.Count;

            SyncJobEntries.Insert(index, entry);
        }

        private string normalizeString(string str, int prefixLength, int suffixLength)
        {
            if (str.Length > prefixLength + suffixLength)
                return str.Substring(0, prefixLength) + "..." + str.Substring(str.Length - suffixLength);
            else
                return str;
        }

        private void selectSyncJobs(bool selectAll)
        {
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.IsSelected = selectAll;
        }

        private static void UpdateTextBoxBindings()
        {
            TextBox textBox = Keyboard.FocusedElement as TextBox;

            if (textBox != null)
            {
                BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null && !textBox.IsReadOnly && textBox.IsEnabled)
                    be.UpdateSource();
            }
        }

        private bool saveSyncJob(UISyncJobEntry entry)
        {
            entry.NewJobName = entry.NewJobName.Trim();
            entry.NewSyncSource = entry.NewSyncSource.Trim();
            entry.NewIntermediaryStoragePath = entry.NewIntermediaryStoragePath.Trim();

            string syncSourceDir = txtSource.Text;
            string intStorageDir = txtIntStorage.Text;
            if (!validateSyncJobParams(entry.NewJobName, entry.NewSyncSource, entry.NewIntermediaryStoragePath))
                return false;

            BackgroundWorker editJobWorker = new BackgroundWorker();
            editJobWorker.DoWork += new DoWorkEventHandler(editJobWorker_DoWork);
            editJobWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(editJobWorker_RunWorkerCompleted);

            SetControlsEnabledState(false);
            editJobWorker.RunWorkerAsync(entry);

            return true;
        }

        public ObservableCollection<UISyncJobEntry> SyncJobEntries
        {
            get { return _SyncJobEntries; }
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            UISyncJobEntry entry = listAllSyncJobs.SelectedItem as UISyncJobEntry;

            if (e.Key == Key.Escape)
            {
                if (entry != null) entry.EditMode = false; ;
            }
            else if (e.Key == Key.Enter)
            {
                if (entry != null && entry.EditMode)
                {
                    UpdateTextBoxBindings();
                    if (saveSyncJob(entry)) entry.EditMode = false;
                }
            }
        }

        private void SetControlsEnabledState(bool isEnabled)
        {
            listAllSyncJobs.IsEnabled = isEnabled;
            txtSyncJobName.IsEnabled = isEnabled;
            txtSource.IsEnabled = isEnabled;
            txtIntStorage.IsEnabled = isEnabled;
            txtBlkNewJob.IsEnabled = isEnabled;
        }

	}
}