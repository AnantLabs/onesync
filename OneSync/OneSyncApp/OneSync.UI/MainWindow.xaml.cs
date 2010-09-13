//Coded by Goh Chun Lin
/*
 $Id: MainWindow.xaml.cs 287 2010-03-18 02:25:56Z gclin009 $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Community.CsharpSqlite.SQLiteClient;
using OneSync.Files;
using OneSync.Synchronization;
using Microsoft.Win32;
using System.Resources;
using System.Globalization;
using System.Threading;

namespace OneSync.UI
{

	public partial class MainWindow : Window
	{
		// Start: Global variables.        
        private string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;

        //public ResourceManager m_ResourceManager = new ResourceManager(Properties.Settings.Default.LanguageResx,
        //                            System.Reflection.Assembly.GetExecutingAssembly());
        public ResourceManager m_ResourceManager;

        private SyncJobManager jobManager;
        private BackgroundWorker syncWorker;
        private UISyncJobEntry currentJobEntry;

        private System.Windows.Forms.NotifyIcon trayNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip ctxTrayMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuRunProgram;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;

        TextBox editTextBox;

        private DispatcherTimer timerDropbox; //The time to check the Dropbox status frequently.

        private ObservableCollection<UISyncJobEntry> _SyncJobEntries = new ObservableCollection<UISyncJobEntry>();

        TaskbarManager tbManager = TaskbarManager.Instance;
        // End: Global variables.
		
		
		public MainWindow()
		{
            this.InitializeComponent();

            //For debugging purpose: Reset language.
            //Properties.Settings.Default.LanguageResx = "";
            //Properties.Settings.Default.Save();

            if ((Properties.Settings.Default.LanguageResx).Equals(""))
            {
                LanguageSelection languageSelection = new LanguageSelection();
                languageSelection.Show();
                this.Hide();
            }
            else
            {
                m_ResourceManager = new ResourceManager(Properties.Settings.Default.LanguageResx,
                                        System.Reflection.Assembly.GetExecutingAssembly());

                string RunningProcess = Process.GetCurrentProcess().ProcessName;
                Process[] processes = Process.GetProcessesByName(RunningProcess);

                RegistryKey installed_versions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
                string[] version_names = installed_versions.GetSubKeyNames();
                //version names start with 'v', eg, 'v3.5' which needs to be trimmed off before conversion
                double Framework = Convert.ToDouble(version_names[version_names.Length - 1].Remove(0, 1));
                int SP = Convert.ToInt32(installed_versions.OpenSubKey(version_names[version_names.Length - 1]).GetValue("SP", 0));

                if (processes.Length > 1)
                {
                    //Make sure that there is only one OneSync application running for all the time.
                    MessageBox.Show(m_ResourceManager.GetString("msg_oneInstance"), m_ResourceManager.GetString("msg_oneInstanceTitle"), MessageBoxButton.OK, MessageBoxImage.Stop);
                    Application.Current.Shutdown();
                }
                else if (Framework < 3.5 || (Framework == 3.5 && SP < 1))
                {
                    //Check for the .NET Framework requirement.
                    //Requirement is .NET 3.5 SP1 or later.
                    DotNetFrameworkErrorWindow dotNetFrameworkErrorWindow = new DotNetFrameworkErrorWindow();
                    dotNetFrameworkErrorWindow.Show();
                    this.Hide();
                }
                else
                {
                    trayNotifyIcon = new System.Windows.Forms.NotifyIcon();
                    trayNotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
                    trayNotifyIcon.Text = m_ResourceManager.GetString("not_name");
                    trayNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(trayNotifyIcon_MouseDoubleClick);
                    trayNotifyIcon.Visible = true;

                    //For Run Program context menu item
                    mnuRunProgram = new System.Windows.Forms.ToolStripMenuItem(m_ResourceManager.GetString("not_openName"));
                    mnuRunProgram.Click += new EventHandler(mnuRunProgram_Click);

                    //For Help context menu item
                    mnuHelp = new System.Windows.Forms.ToolStripMenuItem(m_ResourceManager.GetString("not_helpName"));
                    mnuHelp.Click += new EventHandler(mnuHelp_Click);

                    //For Exit context menu item
                    mnuExit = new System.Windows.Forms.ToolStripMenuItem();
                    mnuExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    //mnuExit.Size = new System.Drawing.Size(117, 22);
                    mnuExit.Text = m_ResourceManager.GetString("not_exitName");
                    mnuExit.Click += new EventHandler(mnuExit_Click);

                    ctxTrayMenu = new System.Windows.Forms.ContextMenuStrip();
                    ctxTrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuRunProgram, mnuHelp, mnuExit });

                    trayNotifyIcon.ContextMenuStrip = ctxTrayMenu;

                    // Get current synchronization directory from command line arguments.
                    // Default sync directory is current directory of app.
                    string[] args = System.Environment.GetCommandLineArgs();

                    jobManager = SyncClient.GetSyncJobManager(STARTUP_PATH);

                    if (args.Length > 1 && Validator.validateDirPath(args[1]) == null)
                    {
                        txtSyncJobName.Text = System.IO.Path.GetFileName(args[1]);
                        txtSource.Text = args[1];
                        txtSource.Focus();
                    }

                    // Tag each browse button to corr TextBox
                    btnBrowse_Source.Tag = txtSource;
                    btnBrowse.Tag = txtIntStorage;

                    // Initialize syncWorker 
                    syncWorker = new BackgroundWorker();
                    syncWorker.WorkerSupportsCancellation = true;
                    syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
                    syncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncWorker_RunWorkerCompleted);

                    // Configure the timer to check the Dropbox status
                    timerDropbox = new DispatcherTimer();
                    timerDropbox.Tick += new EventHandler((sender, e) => dropboxStatusChecking());
                    timerDropbox.Interval = TimeSpan.FromMilliseconds(1000);

                    _SyncJobEntries.CollectionChanged += (sender, e) => refreshCombobox();

                    // Do not show the help button if the help file is not there
                    if (!File.Exists(STARTUP_PATH + @"\OneSync.chm"))
                        btnHelp.Visibility = Visibility.Hidden;

                    // Set-up data bindings
                    listAllSyncJobs.ItemsSource = this.SyncJobEntries;
                    LoadSyncJobs();

                    //Check for latest update.
                    //MessageBox.Show(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    try
                    {
                        ProgramUpdateChecker updateChecker = new ProgramUpdateChecker();
                        string updateURL = updateChecker.GetNewVersion();
                        if (!string.IsNullOrEmpty(updateURL))
                        {
                            MessageBoxResult gettingNewVersion = MessageBox.Show(
                                    m_ResourceManager.GetString("msg_newOneSyncVersion"), m_ResourceManager.GetString("msg_newOneSyncVersionTitle"),
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (gettingNewVersion == MessageBoxResult.Yes)
                                Process.Start(updateURL);
                        }
                    }
                    catch (Exception)
                    {
                        //Do nothing
                    }
                    language();
                }
            }
        }

        private void language() 
        {
            lblSyncJobNameDesc.Content = m_ResourceManager.GetString("lbl_syncJobName");
            lblSyncSrcFolderDesc.Content = m_ResourceManager.GetString("lbl_syncSourceDir");
            lblIntStorageDirDesc.Content = m_ResourceManager.GetString("lbl_syncIntStorageDir");
            txtBlkNewJob.Text = m_ResourceManager.GetString("btn_addNewJob");
            txtBlkProceed.Text = m_ResourceManager.GetString("btn_syncJobs");
            showPartialSyncJobsCheckBox.Content = m_ResourceManager.GetString("chb_showPartialSyncJobs");
            lblSyncSrcFolderDesc2.Content = m_ResourceManager.GetString("lbl_syncSourceDir2");
            listAllSyncJobs_header_1.Header = m_ResourceManager.GetString("hdr_reorder");
            listAllSyncJobs_header_2.Header = m_ResourceManager.GetString("hdr_jobName");
            listAllSyncJobs_header_3.Header = m_ResourceManager.GetString("hdr_syncSourceDir");
            listAllSyncJobs_header_4.Header = m_ResourceManager.GetString("hrd_syncIntStorageDir");
            listAllSyncJobs_header_5.Header = m_ResourceManager.GetString("hdr_syncStatus");
            txtBlkProceed.ToolTip = m_ResourceManager.GetString("btn_syncJobs");
            btnBrowse_Source.ToolTip = m_ResourceManager.GetString("tot_browseFolder");
            btnBrowse.ToolTip = m_ResourceManager.GetString("tot_browseFolder");
            btnSyncStatic.ToolTip = m_ResourceManager.GetString("btn_syncJobs");
            lbl_description.Content = m_ResourceManager.GetString("lbl_syncDescription");
        }

        private void dropboxStatusChecking()
        {
            try
            {
                bool isStillOneWaitingForDropbox = false;
                foreach (UISyncJobEntry entry in SyncJobEntries)
                {
                    if (entry.DropboxStatus == OneSync.DropboxStatus.SYNCHRONIZING)
                    {
                        isStillOneWaitingForDropbox = true;
                        entry.ProgressBarMessage = m_ResourceManager.GetString("msg_progressBar1");
                        entry.ProgressBarColor = "Yellow";
                    }
                    else if (entry.DropboxStatus == OneSync.DropboxStatus.UP_TO_DATE)
                    {
                        if (entry.Error == null)
                        {
                            entry.ProgressBarMessage = m_ResourceManager.GetString("msg_progressBar2");
                            entry.ProgressBarColor = "#FF01D328";
                        }
                        else
                        {
                            entry.ProgressBarMessage = m_ResourceManager.GetString("msg_progressBar3");
                            entry.ProgressBarColor = "Red";
                        }
                    }
                }
                if (isStillOneWaitingForDropbox)
                    lblStatus.Content = m_ResourceManager.GetString("msg_updatingDropBox");
                else
                    lblStatus.Content = m_ResourceManager.GetString("msg_updatedDropBox");
            }
            catch(Exception) { }            
        }

        private void refreshCombobox() 
        {
            ///TODO: Add Exception
            txtSource.Items.Clear();
            txtIntStorage.Items.Clear();
            foreach (UISyncJobEntry entry in SyncJobEntries)
            {
                if (!txtSource.Items.Contains(entry.SyncSource))
                    txtSource.Items.Add(entry.SyncSource);

                if (!txtIntStorage.Items.Contains(entry.IntermediaryStoragePath))
                    txtIntStorage.Items.Add(entry.IntermediaryStoragePath);
            }
        }

        private void sync_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg("");

            if (syncWorker.IsBusy)
            {
                txtBlkProceed.Text = "Cancelling...";
                btnSyncRotating.ToolTip = "Cancelling...";
                syncWorker.CancelAsync();
                return;
            }
            btnSyncRotating.ToolTip = "Cancel Subsequent Jobs";

            Queue<UISyncJobEntry> selectedJobs = UISyncJobEntry.GetSelectedJobs(SyncJobEntries);

            if (selectedJobs.Count == 0)
            {
                showErrorMsg(m_ResourceManager.GetString("err_noSyncJob"));
                return;
            }

            lblJobsNumber.Content = selectedJobs.Count;

            // Exit edit-mode of all Job Entry
            foreach (UISyncJobEntry entry in listAllSyncJobs.Items)
                entry.EditMode = false;

            try
            {
                Synchronize(selectedJobs);
            }catch(SyncJobException syncJobException)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    showErrorMsg(syncJobException.Message);
                });
            }catch(Exception exception)
            {
                try
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        showErrorMsg(exception.Message);
                    });
                }
                catch(Exception) { } //If you don't have .NET 3.5 SP1.
            }


        }

		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ComboBox cb = ((Button)sender).Tag as ComboBox;
                if (cb == null)
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
                else
                {
                    System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        cb.Text = fbd.SelectedPath;
                        if (txtSyncJobName.Text.Length <= 0)
                        {
                            try
                            {
                                txtSyncJobName.Text = System.IO.Path.GetFileName(txtSource.Text.Trim());
                            }
                            catch (Exception) { }
                        }
                        cb.Focus();
                    }
                }
            }
            catch (Exception) { }
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

            //Create new sync job if all three inputs mentioned above are valid.
            try
            {
                if (!Validator.SyncJobParamsValidated(syncJobName, syncSourceDir, intStorageDir, jobManager.LoadAllJobs()))
                {
                    showErrorMsg(m_ResourceManager.GetString("err_invalidInput"));
                    return;
                }

                SyncJob job = jobManager.CreateSyncJob(syncJobName, syncSourceDir, intStorageDir);
                UISyncJobEntry entry = new UISyncJobEntry(job) { IsSelected = true };
                SyncJobEntries.Add(entry);
                txtSyncJobName.Text = "";
                txtSource.Text = "";
                txtIntStorage.Text = "";
            }
            catch (SyncJobNameExistException ex)
            {
                showErrorMsg(ex.Message);
            }
            catch(SyncJobException sje)
            {
                showErrorMsg(sje.Message);
            }
            catch(Exception ex)
            {
                showErrorMsg(ex.Message);
            }
        }

        private void edit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //TODO: Add try catch
            showErrorMsg("");
            if (sender.GetType() == typeof(TextBlock) && e.ClickCount == 1)
                return;

            UpdateTextBoxBindings();

            FrameworkElement img = (FrameworkElement)e.Source;
            UISyncJobEntry entry  = (UISyncJobEntry)img.DataContext;

            entry.EditMode = !(entry.EditMode && saveSyncJob(entry));
        }

        private void imgDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg("");
            try
            {
                Image img = (Image)e.Source;
                UISyncJobEntry entry = (UISyncJobEntry)img.DataContext;

                if (!Directory.Exists(entry.IntermediaryStoragePath))
                {
                    MessageBoxResult intermediateStorageAvailability = MessageBox.Show(
                            String.Format(m_ResourceManager.GetString("err_deleteJobWithNoIntermediateStorage"), entry.SyncJob.Name), m_ResourceManager.GetString("err_deleteJobWithNoIntermediateStorageTitle"),
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (intermediateStorageAvailability == MessageBoxResult.No) return;
                }

                MessageBoxResult result = MessageBox.Show(
                            String.Format(m_ResourceManager.GetString("err_deleteJob"), entry.SyncJob.Name), m_ResourceManager.GetString("err_deleteJobTitle"),
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No) return;

                if (!jobManager.Delete(entry.SyncJob))
                    showErrorMsg(m_ResourceManager.GetString("err_unableDeleteJob"));
                else
                    this.SyncJobEntries.Remove(entry);

                // Try to delete Sync Source table from intermediary storage
                // TODO: thuat handled this?
                SyncSourceProvider syncSourceProvider =
                    SyncClient.GetSyncSourceProvider(entry.IntermediaryStoragePath);

                syncSourceProvider.DeleteSyncSourceInIntermediateStorage(entry.SyncJob.SyncSource);
            }
            catch (MetadataFileException)
            {
                showErrorMsg(m_ResourceManager.GetString("err_metadataMissing"));
            }
            catch(SqliteSyntaxException)
            {
                showErrorMsg(m_ResourceManager.GetString("err_metadataMissing"));
            }
            catch(Exception exception)
            {
                showErrorMsg(exception.Message);
            }
        }
        /*
        private bool validateSyncJobParams(string jobName, string syncSource, string intStorage)
        {
            string errorMsg = Validator.validateSyncJobName(jobName);
            if (errorMsg != null)
            {
                showErrorMsg(errorMsg);
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
        */

        
        void editJobWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                showErrorMsg("");
                lblSyncJobEditing.Visibility = Visibility.Visible;
            });

            UISyncJobEntry entry = e.Argument as UISyncJobEntry;
            if (entry == null) return;
            
            string oldIStorage = entry.SyncJob.IntermediaryStorage.Path;
            string oldSyncSource = entry.SyncJob.SyncSource.Path;
            string oldSyncJobName = entry.SyncJob.Name;
            
            entry.SyncJob.Name = entry.NewJobName;
            entry.SyncJob.IntermediaryStorage.Path = entry.NewIntermediaryStoragePath;
            entry.SyncJob.SyncSource.Path = entry.NewSyncSource;
           try
           {
               if (!Validator.SyncJobParamsValidated(entry.NewJobName, entry.NewSyncSource, entry.NewIntermediaryStoragePath, jobManager.LoadAllJobs())) 
               {
                   this.Dispatcher.Invoke((Action)delegate
                   {
                       entry.SyncJob.Name = oldSyncJobName;
                       entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                       entry.SyncJob.SyncSource.Path = oldSyncSource;
                       entry.InfoChanged();
                       showErrorMsg(m_ResourceManager.GetString("err_invalidInput"));
                   });
               }
               if (!Validator.IntermediaryMovable (entry.IntermediaryStoragePath, entry.SyncJob.SyncSource.ID))
                   throw new SyncJobException(String.Format(m_ResourceManager.GetString("err_metadataMissingSource"), oldSyncSource));

               jobManager.Update(entry.SyncJob);
               entry.InfoChanged(); /* Force databinding to refresh */

               if (!entry.SyncJob.IntermediaryStorage.Path.Equals(oldIStorage))
               {
                   try
                   {
                       if (!Files.FileUtils.MoveFolder(oldIStorage, entry.SyncJob.IntermediaryStorage.Path))
                           throw new SyncJobException(String.Format(m_ResourceManager.GetString("err_filesCannotBeMoved"), entry.SyncJob.IntermediaryStorage.Path));
                   }
                   catch (SyncJobException ex)
                   {
                       this.Dispatcher.Invoke((Action)delegate
                       {
                           entry.SyncJob.Name = oldSyncJobName;
                           entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                           entry.SyncJob.SyncSource.Path = oldSyncSource;
                           entry.InfoChanged();
                           showErrorMsg(ex.Message);
                       });
                   }
                   catch (DirectoryNotFoundException directoryNotFoundException)
                   {
                       this.Dispatcher.Invoke((Action)delegate
                       {
                           entry.SyncJob.Name = oldSyncJobName;
                           entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                           entry.SyncJob.SyncSource.Path = oldSyncSource;
                           entry.InfoChanged();
                           showErrorMsg(m_ResourceManager.GetString("err_filesCannotBeMoved2"));
                       });
                   }
                   catch (UnauthorizedAccessException unauthorizedAccessException)
                   {
                       this.Dispatcher.Invoke((Action)delegate
                       {
                           entry.SyncJob.Name = oldSyncJobName;
                           entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                           entry.SyncJob.SyncSource.Path = oldSyncSource;
                           entry.InfoChanged();
                           showErrorMsg(m_ResourceManager.GetString("err_filesCannotBeMoved2"));
                       });
                   }
                   catch (SyncSourceException sourceException)
                   {
                       this.Dispatcher.Invoke((Action)delegate
                       {
                           entry.SyncJob.Name = oldSyncJobName;
                           entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                           entry.SyncJob.SyncSource.Path = oldSyncSource;
                           entry.InfoChanged();
                           showErrorMsg(sourceException.Message);
                       });
                   }
                   catch (OutOfDiskSpaceException)
                   {
                       this.Dispatcher.Invoke((Action)delegate
                       {
                           entry.SyncJob.Name = oldSyncJobName;
                           entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                           entry.SyncJob.SyncSource.Path = oldSyncSource;
                           entry.InfoChanged();
                           showErrorMsg(m_ResourceManager.GetString("err_intermediateStorageTooSmall"));
                       });
                   }
                   catch(Exception)
                   {
                       this.Dispatcher.Invoke((Action)delegate
                       {
                           
                       });
                   }
               }

               
               e.Result = entry;
           }
            catch(SyncJobNameExistException profileNameExistException)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    entry.SyncJob.Name = oldSyncJobName;
                    entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                    entry.SyncJob.SyncSource.Path = oldSyncSource;
                    entry.InfoChanged();
                    showErrorMsg(profileNameExistException.Message);
                    SetControlsEnabledState(false, true);
                    lblSyncJobEditing.Visibility = Visibility.Hidden;
                });
            }
            catch (Exception ee)
            {
                entry.SyncJob.Name = oldSyncJobName;
                entry.SyncJob.IntermediaryStorage.Path = oldIStorage;
                entry.SyncJob.SyncSource.Path = oldSyncSource;
                entry.InfoChanged();
                this.Dispatcher.Invoke((Action)delegate
                {
                    showErrorMsg(ee.Message);
                    SetControlsEnabledState(false, true);
                    lblSyncJobEditing.Visibility = Visibility.Hidden;
                });
            }
        }

        void editJobWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UISyncJobEntry entry = e.Result as UISyncJobEntry;
            if (entry == null) return;

            entry.EditMode = false;

            refreshCombobox();
            
            SetControlsEnabledState(false, true);

            lblSyncJobEditing.Visibility = Visibility.Hidden;
        }

        private void txtBlkShowLog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showErrorMsg(""); //Clear error msg

                TextBlock clickedBlock = (TextBlock)e.Source;
                UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;

                //View log file (The extension of the file should be .html).
                if (File.Exists(Log.ReturnLogReportPath(entry.SyncSource)))
                    Log.ShowLog(entry.SyncSource);
                else
                    showErrorMsg(m_ResourceManager.GetString("err_noLogFile"));
            
            
        }

        private void textBoxPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedBlock = (TextBlock)e.Source;
            UISyncJobEntry entry = clickedBlock.DataContext as UISyncJobEntry;
            if (entry == null || entry.SyncJob == null)
            {
                showErrorMsg(m_ResourceManager.GetString("err_syncJobNotFound"));
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
            {
                if (e != null && e.AddedItems.Count > 0 && e.AddedItems[0] != entry)
                    entry.EditMode = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (trayNotifyIcon != null)
            {
                e.Cancel = true;

                this.Hide();

                trayNotifyIcon.ShowBalloonTip(3000, m_ResourceManager.GetString("not_messageTitle1"),
                    m_ResourceManager.GetString("not_message1"), System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        void mnuRunProgram_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        void mnuHelp_Click(object sender, EventArgs e)
        {
            if (File.Exists(STARTUP_PATH + @"\OneSync.chm"))
                Process.Start(STARTUP_PATH + @"\OneSync.chm");
            else
                MessageBox.Show(m_ResourceManager.GetString("err_helpFileNotFound"), m_ResourceManager.GetString("err_helpFileNotFoundTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void mnuExit_Click(object sender, EventArgs e)
        {
            if (syncWorker.IsBusy)
            {
                MessageBoxResult result = MessageBox.Show(
                    m_ResourceManager.GetString("err_closeWhileSyncing"),
                    m_ResourceManager.GetString("err_closeWhileSyncingTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    saveJobsOrder();
                    Process.GetCurrentProcess().Kill();
                }
            }
            else 
            {
                saveJobsOrder();
                Process.GetCurrentProcess().Kill();
            }
        }

        void trayNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
        }

        #region Synchronization

        private void Synchronize(Queue<UISyncJobEntry> selectedEntries)
        {
            if (selectedEntries.Count <= 0) return;

            // Hide all sync job progress bars
            foreach (UISyncJobEntry entry in SyncJobEntries)
                entry.ProgressBarVisibility = Visibility.Hidden;

            UISyncJobEntry currentJobEntry = null;
            try
            {
                currentJobEntry = selectedEntries.Peek();
                // Update UI
                UpdateSyncInfoUI(currentJobEntry.SyncJob);
                SetControlsEnabledState(true, false);
                listAllSyncJobs_SelectionChanged(null, null);

                // Run sync
                syncWorker.RunWorkerAsync(selectedEntries);
            }
            catch(Exception exception)
            {
                string errorMsg = Validator.validateSyncDirs(currentJobEntry.SyncSource, currentJobEntry.IntermediaryStoragePath);
                if (errorMsg != null)
                    showErrorMsg(errorMsg);
                else
                    showErrorMsg(exception.Message);
            }
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Stop the Dropbox checker.
            timerDropbox.Stop();

            Queue<UISyncJobEntry> jobEntries = e.Argument as Queue<UISyncJobEntry>;
            if (jobEntries == null || jobEntries.Count <= 0) return;

            UISyncJobEntry entry = jobEntries.Peek();
            if (entry == null) return;

            // Keep track of current entry that is being synchronized
            currentJobEntry = entry;

            // Update UI
            entry.ProgressBarColor = "#FF01D328";
            entry.ProgressBarValue = 0;
            entry.ProgressBarVisibility = Visibility.Visible;
            entry.ProgressBarMessage = m_ResourceManager.GetString("msg_syncing");

            // Add event handler
            AddAgentEventHandler(entry.SyncAgent);

            try
            {
                if (entry.SyncJob.SyncPreviewResult == null)
                    entry.SyncJob.SyncPreviewResult = entry.SyncAgent.GenerateSyncPreview(null);

                entry.SyncAgent.Synchronize(entry.SyncJob.SyncPreviewResult);
                entry.Error = null;
            }
            catch (Community.CsharpSqlite.SQLiteClient.SqliteSyntaxException ex)
            {
                entry.Error = ex;
                entry.ProgressBarValue = 100;
                entry.ProgressBarColor = "Red";
                
                this.Dispatcher.Invoke((Action)delegate
                {
                    string errorMsg = m_ResourceManager.GetString("err_metadataMissing");
                    entry.ProgressBarMessage = errorMsg;
                    showErrorMsg(errorMsg);
                });                
            }
            catch(System.IO.DirectoryNotFoundException ex)
            {
                entry.Error = ex;
                entry.ProgressBarValue = 100;
                entry.ProgressBarColor = "Red";

                this.Dispatcher.Invoke((Action)delegate
                {
                    string errorMsg = String.Format(m_ResourceManager.GetString("err_directoryNotFound"), ex.Message);
                    entry.ProgressBarMessage = errorMsg;
                    showErrorMsg(errorMsg);
                });                
            }
            catch(SyncJobException syncJobException)
            {
                entry.Error = syncJobException;
                entry.ProgressBarValue = 100;
                entry.ProgressBarColor = "Red";

                this.Dispatcher.Invoke((Action)delegate
                {
                    string errorMsg = syncJobException.Message;
                    entry.ProgressBarMessage = errorMsg;
                    showErrorMsg(errorMsg);
                });
            }
            catch (OutOfDiskSpaceException ex)
            {
                string errorMsg;
                entry.Error = ex;
                entry.ProgressBarValue = 100;
                errorMsg = String.Format(m_ResourceManager.GetString("err_insufficientSpace"), entry.IntermediaryStoragePath);
                this.Dispatcher.Invoke((Action)delegate
                {
                    entry.ProgressBarMessage = errorMsg;
                    showErrorMsg(errorMsg);
                });    
            }
            catch (Exception ex)
            {
                string errorMsg;
                entry.Error = ex;
                entry.ProgressBarValue = 100;
                entry.ProgressBarColor = "Red";
                errorMsg = String.Format(m_ResourceManager.GetString("err_errorReported"), ex.Message);
                this.Dispatcher.Invoke((Action)delegate
                {
                    entry.ProgressBarMessage = errorMsg;
                    showErrorMsg(errorMsg);
                });    
            }

            if (syncWorker.CancellationPending)
            {
                UISyncJobEntry currentSyncJobEntry = jobEntries.Peek();
                jobEntries.Clear();
                jobEntries.Enqueue(currentSyncJobEntry);
            }

            e.Result = jobEntries;
            
        }

        void syncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                if (e.Error != null)
                {
                    return;
                }

                Queue<UISyncJobEntry> jobEntries = e.Result as Queue<UISyncJobEntry>;
                if (jobEntries == null) return;

                if (jobEntries.Count > 0 && jobEntries.Peek() != null)
                {
                    //Remove the synced agent and sync job.
                    UISyncJobEntry completedJobEntry = jobEntries.Dequeue();

                    if (completedJobEntry.Error == null)
                        completedJobEntry.ProgressBarMessage = m_ResourceManager.GetString("msg_syncJobCompleted");

                    completedJobEntry.SyncJob.SyncPreviewResult = null;
                    RemoveAgentEventHandler(completedJobEntry.SyncAgent);
                }

                // Check for more sync jobs to run or whether cancel pending
                if (jobEntries.Count <= 0)
                {
                    // Start the Dropbox checker now.
                    timerDropbox.Start();

                    // Update UI and hide sync progress controls
                    SetControlsEnabledState(false, true);
                    lblStatus.Content = m_ResourceManager.GetString("msg_syncCompleted");
                    lblSubStatus.Content = "";

                    //listAllSyncJobs.IsEnabled = true;
                }
                else
                {
                    UpdateSyncInfoUI(jobEntries.Peek().SyncJob);
                    syncWorker.RunWorkerAsync(jobEntries);
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
            currentJobEntry.ProgressBarValue = e.Progress;
            if (tbManager != null) tbManager.SetProgressValue(e.Progress, 100);
        }

        /// <summary>
        /// Update the status message.
        /// </summary>
        void currAgent_StatusChanged(object sender, SyncStatusChangedEventArgs e)
        {
            //Display some text so that the user knows that what OneSync is doing during the synchronization.
            if (this.Dispatcher.CheckAccess())
                lblStatus.Content = e.Message;
            else
                lblStatus.Dispatcher.Invoke((Action)delegate { lblStatus.Content = e.Message; });
        }

        /// <summary>
        /// Tell the user which file is being processed now.
        /// </summary>
        void currAgent_FileChanged(object sender, Synchronization.SyncFileChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                lblSubStatus.Content = String.Format(m_ResourceManager.GetString("msg_syncingDirectory"), e.RelativePath);
            else
                lblSubStatus.Dispatcher.Invoke((Action)delegate { lblSubStatus.Content = String.Format(m_ResourceManager.GetString("msg_syncingDirectory"), e.RelativePath); });
        }

        void syncAgent_SyncStatusChanged(object sender, SyncStatusChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
                lblStatus.Content = e.Message;
            else
                this.Dispatcher.Invoke((Action)delegate { lblStatus.Content = e.Message; });
        }

        #endregion

        public void LoadSyncJobs()
        {
            string syncSourceDir = txtSource.Text.Trim();
            IList<SyncJob> jobs = jobManager.LoadAllJobs();
            sortJobs(jobs);

            foreach (SyncJob job in jobs)
                SyncJobEntries.Add(new UISyncJobEntry(job));      
        }

        private void MoveJobEntry(UISyncJobEntry entry, int delta)
        {
            int index = SyncJobEntries.IndexOf(entry);
            SyncJobEntries.RemoveAt(index);

            index += delta;

            // Bound index
            if (index < 0) index = (SyncJobEntries.Count + 1) + index;
            if (index > SyncJobEntries.Count) index -= (SyncJobEntries.Count + 1);

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
            
            try
            {
                if (Validator.SyncJobParamsValidated(entry.NewJobName, entry.NewSyncSource, entry.NewIntermediaryStoragePath, jobManager.LoadAllJobs()))
                {
                    BackgroundWorker editJobWorker = new BackgroundWorker();
                    editJobWorker.DoWork += new DoWorkEventHandler(editJobWorker_DoWork);
                    editJobWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(editJobWorker_RunWorkerCompleted);

                    SetControlsEnabledState(false, false);
                    editJobWorker.RunWorkerAsync(entry);
                }
                else
                {
                    showErrorMsg(m_ResourceManager.GetString("err_invalidInput"));
                    entry.EditMode = false;
                    SetControlsEnabledState(false, true);
                }
            }
            catch (SyncJobException syncJobException)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    showErrorMsg(syncJobException.Message);
                    entry.EditMode = false;
                    SetControlsEnabledState(false, true);
                });                
                return false;
            }
            catch(SyncJobNameExistException profileNameExistException)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    showErrorMsg(profileNameExistException.Message);
                    entry.EditMode = false;
                    SetControlsEnabledState(false, true);
                });
                return false;
            }
            catch(Exception exception)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    showErrorMsg(exception.Message);
                });
                return false;
            }
            return true;
        }

        private void saveJobsOrder()
        {
            ArrayList jobs = new ArrayList();

            foreach (UISyncJobEntry entry in SyncJobEntries)
                jobs.Add(entry.JobName);

            Properties.Settings.Default.JobsOrder = jobs;
            Properties.Settings.Default.Save();
        }

        private void sortJobs(IList<SyncJob> jobs)
        {
            // Try to load sync jobs according to previously saved order
            ArrayList orderedJobs = Properties.Settings.Default.JobsOrder;
            if (orderedJobs == null) return;


            List<SyncJob> loadedJobs = new List<SyncJob>(jobs);
            jobs.Clear();

            foreach (string jobName in orderedJobs)
            {
                SyncJob foundJob = loadedJobs.Find(j => j.Name == jobName);
                if (foundJob != null)
                {
                    loadedJobs.Remove(foundJob);
                    jobs.Add(foundJob);
                }
            }

            foreach (SyncJob j in loadedJobs)
                jobs.Add(j);
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
                ComboBox cb = sender as ComboBox;
                if (cb == null)
                {
                    TextBox tb = sender as TextBox;

                    if (tb == null) return;

                    if (Directory.Exists(folders[0]))
                    {
                        tb.Text = folders[0];
                        tb.Focus();
                    }
                }
                else 
                {
                    if (Directory.Exists(folders[0])) 
                    {
                        cb.Text = folders[0];
                        if (txtSyncJobName.Text.Length <= 0)
                        {
                            try
                            {
                                txtSyncJobName.Text = System.IO.Path.GetFileName(txtSource.Text.Trim());
                            }
                            catch (Exception) { }
                        }
                    }
                }
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
			else if (e.Key == Key.F1)
			{
                if (File.Exists(STARTUP_PATH + @"\OneSync.chm"))
                    Process.Start(STARTUP_PATH + @"\OneSync.chm");
			}
        }

        private void SetControlsEnabledState(bool syncInProgress, bool isEnabled)
        {
            listAllSyncJobs.IsEnabled = isEnabled;
            txtSyncJobName.IsEnabled = isEnabled;
            txtSource.IsEnabled = isEnabled;
            txtIntStorage.IsEnabled = isEnabled;
            txtBlkNewJob.IsEnabled = isEnabled;
            btnBrowse.IsEnabled = isEnabled;
            btnBrowse_Source.IsEnabled = isEnabled;
            btnSyncStatic.IsEnabled = isEnabled;

            if (syncInProgress)
            {
				txtBlkProceed.IsEnabled = true;
                btnSyncRotating.Visibility = Visibility.Visible;
                btnSyncStatic.Visibility = Visibility.Hidden;
                lbl_description.Visibility = Visibility.Visible;
                txtBlkProceed.Text = m_ResourceManager.GetString("tot_cancelJobs");
            }
            else
            {
                btnSyncRotating.Visibility = Visibility.Hidden;
                btnSyncStatic.Visibility = Visibility.Visible;
                txtBlkProceed.Text = m_ResourceManager.GetString("btn_syncJobs");
            }
        }

        private void btnHelp_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	showErrorMsg("");
			if (File.Exists(STARTUP_PATH + @"\OneSync.chm"))
          		Process.Start(STARTUP_PATH + @"\OneSync.chm");
			else
                showErrorMsg(m_ResourceManager.GetString("err_helpFileNotFound"));
        }

        private void showPartialSyncJobsCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            String selectedSyncSource = txtSource.Text.Trim();

            List<UISyncJobEntry> tempSyncJobEntries = new List<UISyncJobEntry>();

            foreach (UISyncJobEntry entry in SyncJobEntries) 
            {
                if (entry.SyncSource.Equals(selectedSyncSource))
                {
                    tempSyncJobEntries.Add(entry);
                }
            }

            SyncJobEntries.Clear();

            foreach (UISyncJobEntry entry in tempSyncJobEntries)
            {
                if (entry.SyncSource.Equals(selectedSyncSource))
                {
                    SyncJobEntries.Add(entry);
                }
            }

            txtSource.SelectedIndex = 0;
        }

        private void showPartialSyncJobsCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            SyncJobEntries.Clear();
            
            LoadSyncJobs();
        }

	}
}