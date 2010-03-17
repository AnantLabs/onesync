/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using OneSync.Synchronization;

//The following two are imported for Aero Glass effect (Aero Glass Part 1/3).
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;
using System.ComponentModel;



namespace OneSync
{
	/// <summary>
	/// This is the class containing the interaction logic for the GUI.
    /// The definition of the controls can be found in the MainWindow.xaml.
	/// </summary>
	public partial class MainWindow : Window
	{
        //This is the logs collection for current sync session.
		//These logs will be displayed on the GUI.
		//They will be recorded in the Log class as well.
		ObservableCollection<UILogEntry> _LogsCollection = new ObservableCollection<UILogEntry>();
		
		//Start: For Aero Glass effect (Part 2/3).
		[StructLayout(LayoutKind.Sequential)]
		private struct Margins
		{
			public int cxLeftWidth;
			public int cxRightWidth;
			public int cyTopHeight;
			public int cyBottomHeight;
		}

		[DllImport("DwmApi.dll")]
		private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

		[DllImport("dwmapi.dll", PreserveSig = false)]
		static extern bool DwmIsCompositionEnabled();
        //End: For Aero Glass effect (Part 2/3).

        //The enum of status in log.
        //The status of the process:
        //  0 - Completely processed;
        //  1 - Conflict.
        enum LogStatus
        {
            Completed = 0,
            Conflict
        };
		
		//Start: Global variables.
        string profile_name; //The name of the profile defined by the user.
        Synchronization.Profile current_profile = null; //Current processing sync job profile object.
		string current_syncing_dir; //The directory of the chosen source folder.
        string storage_dir; //The storage directory.
        bool is_sync_job_created_previously = false; //The value is true if the sync job is not created currently, but previously.
        bool is_sync_job_ongoing = false; //This value is true if the sync job is being done now.
        FileSyncAgent currentAgent; //The file sync agent.
        DoubleAnimation instantNotificationAnimation = new DoubleAnimation(); //The animation of label_notification.
        Storyboard myStoryboard = new Storyboard(); //The storyboard of label_notification.
        //End: Global variables.
		
		
		/*========================================================
		PART 1: MAIN WINDOW GUI SETUP WHEN THE PROGRAM RUNS
		========================================================*/
		
        /// <summary>
        /// This is the code responsible for displaying the main window.
        /// </summary>
		public MainWindow()
		{
            this.InitializeComponent(); //Initialize the controls and components. It is auto-generated.
            //System.Windows.Forms.MessageBox.Show(System.Windows.Forms.VisualStyles.VisualStyleInformation.ColorScheme);
			//Start: Initiate the animation of label_notification.
			instantNotificationAnimation.AutoReverse = true;
			instantNotificationAnimation.From = 1.0;
			instantNotificationAnimation.To = 0.0;
			instantNotificationAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(800));
			myStoryboard.Children.Add(instantNotificationAnimation);
			Storyboard.SetTargetName(instantNotificationAnimation, label_notification.Name);
			Storyboard.SetTargetProperty(instantNotificationAnimation, new PropertyPath("(UIElement.Opacity)"));
			myStoryboard.Begin(this);
			//End: Initiate the animation of label_notification.
			
			testingCode(0); //Testing code number 0.

            //The following line of code is to handle the problem of running this OneSync without using Windows Shell.
            //If the user runs the .exe file directly in the installtion folder of OneSync, then basically when the
            //program tries to extract the source folder directory information from Windows Shell, there will be exception.
            //Hence, the current directory is assigned to the current_syncing_dir variable first. If later the Windows
            //Shell is able to get the information, then it will be replaced by the correct source folder directory.
			current_syncing_dir = Directory.GetCurrentDirectory().ToString();
			try
			{
				//Retrieve the commandline, including all args.
				List<string> lststrCommandLineArgs = System.Environment.GetCommandLineArgs().ToList();
				if(lststrCommandLineArgs.Count > 1)
				{
					//Remove the first arg (which is the installation location of OneSync executable).
					lststrCommandLineArgs.RemoveAt(0);
					current_syncing_dir = lststrCommandLineArgs[0];
				}
			}
			finally
			{
				//Do nothing. =)
			}

            //Display the source folder directory on the windows.
            //If the directory is too long, then it will hide part of it.
			string displaying_current_syncing_dir = current_syncing_dir;
			const int MAX_DIR_STRING_LENGTH = 70;
			const int DIR_STRING_PREFIX_LENGTH = 20;
			if(displaying_current_syncing_dir.Length > MAX_DIR_STRING_LENGTH)
			{
				displaying_current_syncing_dir = displaying_current_syncing_dir.Substring(0, DIR_STRING_PREFIX_LENGTH) + "..."
					+ displaying_current_syncing_dir.Substring(displaying_current_syncing_dir.Length - (MAX_DIR_STRING_LENGTH - DIR_STRING_PREFIX_LENGTH), (MAX_DIR_STRING_LENGTH - DIR_STRING_PREFIX_LENGTH));
			}
			lblSyncDir.Content = displaying_current_syncing_dir;

            //Import all the previous created existing sync job profiles.
            //Note that only the profile having the directory which is the current directory will be imported.
			//TO BE DISCUSSED: Note that for those profile has Sync Source Directory not exist anymore will be deleted.
            try
            {
                IList<Synchronization.Profile> profileItemsCollection = SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles();

                foreach (Synchronization.Profile profileItem in profileItemsCollection)
                {
                    //Retrieve.
                    if(profileItem.SyncSource.Path.Equals(current_syncing_dir))
                    {
                        cmbProfiles.Items.Add(profileItem.Name);
                    }
                }
            }catch(Exception ex)
            {
                InstantNotification("Oops... " + ex.Message);
            }
			
            //Show the log of current/just-finished sync job.
            txtBlkShowLog.Visibility = Visibility.Hidden;
			if(File.Exists(Log.returnLogReportPath(current_syncing_dir, false)))
			{
				txtBlkShowLog.Visibility = Visibility.Visible;
			}
            OneSyncStartingControlsVisibility();
		}
		
		/*========================================================
		PART 1 ENDS HERE
		========================================================*/
		
		
		/*========================================================
		PART 2: SYNC FROM START TO END
		========================================================*/
		/// <summary>
        /// When the user clicks on the Sync button, the sync job will be run.
        /// It will show/hide some controls and enable/disable some of them.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            if(txtIntStorage.Text.Length > 0)
            {
				InstantNotification(""); //Empty the notification message (if any).
                SyncProcessStarted();
            }
            else
            {
                InstantNotification("Please provide the path to your intermediate storage.");
            }
            
		}
		
        /// <summary>
        /// This one will be called internally after the user clicks on the Sync button and before the sync process begins.
        /// It will show/hide some controls and enable/disable some of them.
        /// </summary>
        private void SyncProcessStarted() 
        {
			//Set the storage path.
			storage_dir = txtIntStorage.Text.Trim();

            lblStatus.Content = "You are now syncing";
            if(storage_dir.Length > 0)
            {
                if (directoryVerifier(current_syncing_dir, storage_dir))
                {
                    InstantNotification("");
                    bool should_i_sync = true;

                    //Thuat and Desmond's sync logic takes actions!
                    //Generate a Global Unique Identifier.
                    string name = profile_name;
                    SyncSource syncSource = new SyncSource(System.Guid.NewGuid().ToString(), current_syncing_dir);
                    IntermediaryStorage metaDataSource = new IntermediaryStorage(storage_dir);


                    //Create profile
                    if (!is_sync_job_created_previously)
                    {
                        try
                        {
                            //Synchronization.SyncClient.ProfileProcess.CreateProfile(System.Windows.Forms.Application.StartupPath, profile_name, current_syncing_dir, storage_dir);
                            SyncClient.Initialize(System.Windows.Forms.Application.StartupPath, profile_name, current_syncing_dir, storage_dir);
                        }
                        catch (Exception ee)
                        {
                            InstantNotification("Oops... " + ee.Message);
                            should_i_sync = false;
                        }
                    }
                    else
                    {
                        //Update the profile.
                        try
                        {
                            current_profile.SyncSource.Path = current_syncing_dir;
                            current_profile.IntermediaryStorage.Path = storage_dir;
                            //Synchronization.SyncClient.ProfileProcess.UpdateProfile(System.Windows.Forms.Application.StartupPath, current_profile);
                            SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).Update(current_profile);
                        }
                        catch (Synchronization.DatabaseException de)
                        {
                            InstantNotification("Can't update: " + de.Message);
                            should_i_sync = false;
                        }
                        catch (Exception ex)
                        {
                            InstantNotification("Can't update: " + ex.Message);
                            should_i_sync = false;
                        }
                    }
                    if (should_i_sync)
                    {
                        //foreach (Synchronization.Profile item in (Synchronization.SyncClient.ProfileProcess.GetProfiles(System.Windows.Forms.Application.StartupPath)))
                        foreach (Profile item in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
                        {
                            if (item.Name == profile_name)
                            {
                                currentAgent = new OneSync.Synchronization.FileSyncAgent(item);
                                currentAgent.OnStarted += new OneSync.Synchronization.SyncStartsHandler(SyncProcessNowStarted);
                                currentAgent.OnProgressChanged += new OneSync.Synchronization.SyncProgressChangedHandler(SyncProcessOngoing);
                                currentAgent.OnStatusChanged += new OneSync.Synchronization.SyncStatusChangedHandler(SyncProcessOngoingStatus);
                                currentAgent.OnCompleted += new OneSync.Synchronization.SyncCompletesHandler(SyncProcessCompleted);

                                BackgroundWorker syncWorker = new BackgroundWorker();
                                syncWorker.DoWork += new DoWorkEventHandler(syncWorker_DoWork);
                                syncWorker.RunWorkerAsync();
                            }
                        }
                    }
                }
            }
			
        }

        void syncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SyncPreviewResult previewResult =  currentAgent.PreviewSync();
            currentAgent.Synchronize(previewResult);
        }

        void SyncProcessNowStarted(object sender, Synchronization.SyncStartsEventArgs args) 
        {
            if (this.Dispatcher.CheckAccess())
            {
                is_sync_job_ongoing = true; //Yes, the sync process starts from here!

                //Rotate the OneSync Logo on the button.
                btnSync.Visibility = Visibility.Visible;
                button_sync.Visibility = Visibility.Hidden;
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 360;
                da.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
                da.RepeatBehavior = RepeatBehavior.Forever;
                RotateTransform rt = new RotateTransform();
                btnSync.RenderTransform = rt;
                btnSync.RenderTransformOrigin = new Point(0.5, 0.5);
                rt.BeginAnimation(RotateTransform.AngleProperty, da);

                //Show/hide some controls and enable/disable some of them.
                button_sync.IsEnabled = false;
                txtIntStorage.IsEnabled = false;
                btnBrowse.IsEnabled = false;
                textblock_back_to_home.IsEnabled = false;
                pbSync.Visibility = Visibility.Visible;
                lblStatus.Visibility = Visibility.Visible;
            }
            else
            {
                pbSync.Dispatcher.Invoke((MethodInvoker)delegate { SyncProcessNowStarted(sender, args); });
            }
        }

        void SyncProcessOngoing(object sender, Synchronization.SyncProgressChangedEventArgs args) 
        {
            if (this.Dispatcher.CheckAccess())
            {
                pbSync.Value = args.Value;
            }
            else 
            {
                pbSync.Dispatcher.Invoke((MethodInvoker)delegate { SyncProcessOngoing(sender, args); });
                
            }
        }

        void SyncProcessOngoingStatus(object sender, Synchronization.SyncStatusChangedEventArgs args) 
        {
            //Display some text so that the user knows that what OneSync is doing during the synchronization.
            if (this.Dispatcher.CheckAccess())
            {
                lblStatus.Content = args.Status.ToString();
            }
            else
            {
                lblStatus.Dispatcher.Invoke((MethodInvoker)delegate { SyncProcessOngoingStatus(sender, args); });

            }
        }

        void SyncProcessCompleted(object sender, Synchronization.SyncCompletesEventArgs args)
        {
            if (this.Dispatcher.CheckAccess())
            {
                SyncProcessDone();
            }
            else
            {
                this.Dispatcher.Invoke((MethodInvoker)delegate { SyncProcessCompleted(sender, args); });
            }
        }
		
        /// <summary>
        /// This one will be called internally after the sync process is done.
        /// It will show/hide some controls and enable/disable some of them.
        /// </summary>
		private void SyncProcessDone()
		{
            is_sync_job_ongoing = false; //Okay, the sync job is done.

            //label_message.Content = "You are now selecting";

            //Stop the rotating of the sync logo button.
            btnSync.Visibility = Visibility.Hidden;
            button_sync.Visibility = Visibility.Visible;

            //Show/hide some controls and enable/disable some of them.
            button_sync.IsEnabled = true;
            txtIntStorage.IsEnabled = true;
            btnBrowse.IsEnabled = true;
            textblock_back_to_home.IsEnabled = true;
            Expander.Visibility = Visibility.Hidden;
            lblStatus.Content = "Sync process is successfully done.";

            if (File.Exists(Log.returnLogReportPath(current_syncing_dir, false))) //To be changed. Depends on Naing.
			{
				txtBlkShowLog.Visibility = Visibility.Visible;
			}
		}
		
		/*========================================================
		PART 2 ENDS HERE
		========================================================*/
		
		/*========================================================
		PART 3: SHOW NOTIFICATION MESSAGE TO THE USER
		========================================================*/
		
		/// <summary>
		/// Show the notification message to the user.
		/// </summary>
		/// <param name="message">The notification message</param>
		private void InstantNotification(string message)
		{
			label_notification.Content = message;
		}
		
        /*========================================================
		PART 3 ENDS HERE
		========================================================*/
		
		/*========================================================
		PART 4: LOGS
		========================================================*/
        /// <summary>
        /// When the user clicks on the Show Log button, the .html version of log will be executed.
        /// User is then able to view the log file in the browser.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void txtBlkShowLog_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//View log file (The extension of the file should be .html).
            Process.Start(Log.returnLogReportPath(current_syncing_dir, false));
		}
		
		/*========================================================
		PART 4 ENDS HERE
		========================================================*/
		
		/*========================================================
		PART 5: EXPANDER
		========================================================*/
        /// <summary>
        /// This method will be called when the user clicks on the Expander expand button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void Expander_Expanded(object sender, System.Windows.RoutedEventArgs e)
		{
			//Increase the height of the window to show stuff.
			this.Height += 220;
			Expander.Header = "Hide details";
		}

        /// <summary>
        /// This method will be called when the user clicks on the Expander collapse button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void Expander_Collapsed(object sender, System.Windows.RoutedEventArgs e)
		{
			//Decrease the height of the window.
			this.Height -= 220;
			Expander.Header = "Show details";
		}
		
		/*========================================================
		PART 5 ENDS HERE
		========================================================*/
		
		/*========================================================
		PART 6: OTHER BUTTONS
		========================================================*/
        /// <summary>
        /// This method will be called when the user clicks on the File Browser "..." button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
			
			System.Windows.Forms.DialogResult result = dlg.ShowDialog();
			
			if(result == System.Windows.Forms.DialogResult.OK)
            {
                txtIntStorage.Text = dlg.SelectedPath;
				txtIntStorage.Focus();
            }
		}

        /// <summary>
        /// This method will be called when the user clicks on a button to go back to the home page.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void textblock_back_to_home_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	ProfileCreationControlsVisibility(Visibility.Hidden, Visibility.Visible);
			reloadProfileComboBox();
			Window.Title = "OneSync"; //Change back the menu title.
			cmbProfiles.Text = "";

            Storyboard sb = (Storyboard)Window.Resources["sbHome"];
            sb.Begin(this);
            
        }

        private void button_sync_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            button_sync.Source = new BitmapImage(new Uri("Resource/OneSync Transparent Logo.png", UriKind.Relative));
        }

        private void button_sync_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            button_sync.Source = new BitmapImage(new Uri("Resource/OneSync Transparent Logo (Inactive).png", UriKind.Relative));
        }

        private void txtBlkRenJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	//Rename a profile.
            if(txtRenJob.Text.Trim().Length > 0)
            {
                ProfileCreationControlsVisibility(Visibility.Visible, Visibility.Hidden);

                //foreach (Synchronization.Profile item in (Synchronization.SyncClient.ProfileProcess.GetProfiles(System.Windows.Forms.Application.StartupPath)))
                foreach (Profile item in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
                {
                    if (item.Name.Equals(cmbProfiles.Text))
                    {
                        is_sync_job_created_previously = true;
                        current_profile = item;
                        profile_name = txtRenJob.Text.Trim();
                        txtIntStorage.Text = current_profile.IntermediaryStorage.Path;
                        break;
                    }
                }
                Window.Title = profile_name + " - OneSync";
                //Hide the progress bar.
                pbSync.Visibility = Visibility.Hidden;
                lblStatus.Visibility = Visibility.Hidden;
                try
                {
                    current_profile.Name = txtRenJob.Text.Trim();
                    //SyncClient.ProfileProcess.UpdateProfile(System.Windows.Forms.Application.StartupPath, current_profile);
                    SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).Update(current_profile);

                }
                catch (Synchronization.DatabaseException de)
                {
                    InstantNotification("Can't update: " + de.Message);
                }
                catch (Exception ex)
                {
                    InstantNotification("Can't update: " + ex.Message);
                }
                reloadProfileComboBox();
            }
        }

		/// <summary>
		/// When the user is typing something in the Profile Name combobox. If it detects the name is same as
		/// the name of an existing profile, then it will show the link to allow the user to rename the profile.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
        private void cmbProfiles_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
			if (e.Key == Key.Return)
			{
				do_job();
			}
			else
			{
				showHideProfileEditting(Visibility.Hidden);
				foreach (String item in cmbProfiles.Items)
				{
					//Check to see if the profile is an existing profile or not.
					//If yes, then it will show the rename profile link.
					if (item.Equals(cmbProfiles.Text))
					{
						showHideProfileEditting(Visibility.Visible);
						txtRenJob.Text = cmbProfiles.Text;
						break;
					}
				}
			}
        }

        private void txtBlkDelJob_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	DialogResult result
			= System.Windows.Forms.MessageBox.Show("Are you sure you want to delete " + profile_name + "?", "Job Profile Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if(result == System.Windows.Forms.DialogResult.Yes)
			{
				//Delete the profile.
                try
                {
					//Find out the current profile.
					foreach (Profile item in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
					{
						//Check to see if the profile is an existing profile or not.
						//If yes, then it will show the rename profile link.
						if (item.Name.Equals(cmbProfiles.Text))
						{
							current_profile = item;
							break;
						}
					}
					
                    //Synchronization.SyncClient.ProfileProcess.DeleteProfile(System.Windows.Forms.Application.StartupPath, current_profile.ID);
                    SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).Delete(current_profile);
                    
                    //Go back to the home page.
                    ProfileCreationControlsVisibility(Visibility.Hidden, Visibility.Visible);
                    Window.Title = "OneSync"; //Change back the menu title.
                    cmbProfiles.Text = "";
                    reloadProfileComboBox();
                    
                }
                catch (Synchronization.DatabaseException de)
                {
                    InstantNotification(de.Message);
                }
                catch (Exception ex)
                {
                    InstantNotification(ex.Message);
                }
			}
        }
		
		/// <summary>
		/// This method will be called when the user tries to close the OneSync program.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (is_sync_job_ongoing)
            {
				DialogResult result
				= System.Windows.Forms.MessageBox.Show("Are you sure you want to close the program now? The synchronization is still ongoing.", "Goodbye OneSync", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if(result == System.Windows.Forms.DialogResult.No)
				{
                	e.Cancel = true; //The user cancel the close action.
				}
            }
        }
		
		/// <summary>
        /// This method will be called when the user clicks on the New Job link.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void txtBlkNext_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			do_job();
            //canvasHome.Visibility = Visibility.Hidden;
            //canvasSync.Visibility = Visibility.Visible;
		}
		
		/// <summary>
		/// This event is triggered when the drop-down list of the profiles combobox is closed.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		private void cmbProfiles_DropDownClosed(object sender, System.EventArgs e)
		{
			showHideProfileEditting(Visibility.Hidden);
			try
			{
				foreach (Profile item in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
				{
					//Check to see if the profile is an existing profile or not.
					//If yes, then it will show the rename profile link.
					if (item.Name.Equals(cmbProfiles.Text))
					{
						showHideProfileEditting(Visibility.Visible);
						txtRenJob.Text = cmbProfiles.Text;
						break;
					}
				}
			}
			catch(Exception)
			{
				//Do nothing.
			}
		}
		
		/*========================================================
		PART 6 ENDS HERE
		========================================================*/
		
		/*========================================================================================
		PART 7: FROM SYNC JOB PAGE TO SYNC PAGE AND PROFILE RELOADING
		========================================================================================*/
		
		/// <summary>
		/// This method will be called when the "Do Job" (Chun Lin version) or "Next" (Desmond version) button is clicked.
		/// </summary>
		private void do_job()
		{
			if (cmbProfiles.Text.Trim().Length > 0)
            {
				InstantNotification(""); //Empty the notification message (if any).
                profile_name = cmbProfiles.Text.Trim();
                ProfileCreationControlsVisibility(Visibility.Visible, Visibility.Hidden);
                Window.Title = profile_name + " - OneSync";
                //Hide the progress bar.
                pbSync.Visibility = Visibility.Hidden;
                lblStatus.Visibility = Visibility.Hidden;
                current_profile = null;
                try
                {
                    //foreach (Synchronization.Profile item in (Synchronization.SyncClient.ProfileProcess.GetProfiles(System.Windows.Forms.Application.StartupPath)))
                    foreach (Synchronization.Profile item in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
                    {
                        //Check to see if the profile is an existing profile or not.
                        //If yes, then it will import the storage directory to the program.
						if(item.SyncSource.Path.Equals(current_syncing_dir) && item.Name.Equals(profile_name))
						{
							is_sync_job_created_previously = true;
                            current_profile = item;
                            txtIntStorage.Text = item.IntermediaryStorage.Path;
                            break;
						}
                    }
                    ((Storyboard)Resources["sbNext"]).Begin(this);
                }
                catch (Exception) 
                {
                    //Do nothing
                }
            }
            else 
            {
                InstantNotification("Please provide the name of your sync job.");
            }
		}
		
		/// <summary>
		/// Reload the profiles list in the combo box.
		/// </summary>
		private void reloadProfileComboBox() 
        {
            //Reload the profile combobox.
            cmbProfiles.Items.Clear();
            try
            {
                //IList<Synchronization.Profile> profileItemsCollection = OneSync.Synchronization.SyncClient.ProfileProcess.GetProfiles(System.Windows.Forms.Application.StartupPath);
                IList<Synchronization.Profile> profileItemsCollection = SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles();
                foreach (Synchronization.Profile profileItem in profileItemsCollection)
                {
                    //Retrieve.
                    if (profileItem.SyncSource.Path.Equals(current_syncing_dir))
                    {
                        cmbProfiles.Items.Add(profileItem.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                InstantNotification("Oops... " + ex.Message);
            }
        }
		
		/*========================================================
		PART 7 ENDS HERE
		========================================================*/
		
		/*========================================================
		PART 8: VISIBILITY HANDLING
		========================================================*/
		
		/// <summary>
		/// Change the visibility of the related controls.
		/// </summary>
		/// <param name="visibility">Can be either Visibility.Visible or Visibility.Hidden</param>
		private void showHideProfileEditting(Visibility visibility)
		{
			txtBlkRenJob_label.Visibility = visibility;
            txtRenJob.Visibility = visibility;
            txtBlkRenJob.Visibility = visibility;
			txtBlkDelJob.Visibility = visibility;
			path_profile_operations_1.Visibility = visibility;
			path_profile_operations_2.Visibility = visibility;
			path_profile_operations_3.Visibility = visibility;
		}
		
		 /// <summary>
        /// This methods handle the visibility of some controls in order to make sure that
        /// those controls will not be shown when the program first starts.
        /// </summary>
        private void OneSyncStartingControlsVisibility() 
        {
            //Controls to be hidden when the program first starts.
            lblStatus.Visibility = Visibility.Hidden;
            pbSync.Visibility = Visibility.Hidden;
            Expander.Visibility = Visibility.Hidden;

            ProfileCreationControlsVisibility(Visibility.Hidden, Visibility.Visible);
        }

        /// <summary>
        /// This methods control the visibility of some controls before and after a new sync
        /// job profile is created.
        /// </summary>
        /// <param name="visibility">Can be either visible, hidden or collapsed.</param>
        private void ProfileCreationControlsVisibility(Visibility visibility, Visibility sideVisibility) 
        {
			InstantNotification(""); //Empty the notification message (if any).
			
            //Controls to be hidden/displayed before a sync job profile is created and displayed after the profile is created.
            txtBlkNext.Visibility = sideVisibility;
			cmbProfiles.Visibility = sideVisibility;

            //Reset control's content.
            txtIntStorage.Text = "";

            //Always hidden.
			showHideProfileEditting(Visibility.Hidden);
        }
		
		/*========================================================
		PART 8 ENDS HERE
		========================================================*/
		
		/*========================================================
		PART 9: DIRECTORY VERIFIER
		========================================================*/
		
		/// <summary>
		/// Verify and see whether the directories of Sync Source Folder and Intermediate Storage Folder are valid.
		/// </summary>
		/// <param name="sync_source_dir">Directory of Sync Source Folder</param>
		/// <param name="intermediate_storage_dir">Directory of Intermediate Storage Folder</param>
		/// <returns>True if the both directories are valid.</returns>
		private bool directoryVerifier(string sync_source_dir, string intermediate_storage_dir)
		{
			//Check #1: Check whether the Sync Source Folder and the Intermediate Storage Folder having the same directory.
			if(sync_source_dir.Equals(intermediate_storage_dir))
			{
				InstantNotification("Oops... Your source folder and intermediate storage have the same directory.");
				return false;
			}
			//Check #2a: Check whether the Sync Source Folder exists or not.
			if(!Directory.Exists(sync_source_dir))
			{
				InstantNotification("Oops... I cannot find your Sync Source Folder. You should close this program.");
				return false;
			}
			//Check #2b: Check whether the Intermediate Storage Folder exists or not.
			if(!Directory.Exists(intermediate_storage_dir))
			{
				InstantNotification("Oops... I cannot find your intermediate storage.");
				return false;
			}
			//Check #3: Check whether the Sync Source Folder and the Intermediate Storage Folder are the subdirectory of each other.
			if(sync_source_dir.IndexOf(intermediate_storage_dir) > -1 || intermediate_storage_dir.IndexOf(sync_source_dir) > -1)
			{
				InstantNotification("Oops... Are you doing recursive syncing? Please do not do it here. Thanks.");
				return false;
			}
			
			return true;
		}
		
		/*========================================================
		PART 9 ENDS HERE
		========================================================*/
		
		
		/*========================================================
		PART 10: OTHER CODES FOR FUTURE VERSIONS
		========================================================*/
		/// <summary>
        /// When the window is ready, the Aero Glass effect will be triggered.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void Window_SourceInitialized(object sender, System.EventArgs e)
		{
			//Comment out the following line to hide the Aero Glass effect because
			//the user does not like the Aero Glass effect.
			//AeroGlass();
		}
		
		/// <summary>
        /// For Aero Glass effect (Part 3/3).
        /// </summary>
		private void AeroGlass()
		{
			var originalBackground = this.Background;

			try
			{
				IntPtr hwnd = new WindowInteropHelper(this).Handle;
				HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(hwnd);
				if (!(mainWindowSrc == null || !DwmIsCompositionEnabled()))
				{
					var margins = new Margins { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
					
					this.Background = Brushes.Transparent;
					mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
					DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
				}
			}
			catch (Exception) //If not Vista, paint background normally.
			{
				this.Background = originalBackground;
			}
		}
		
		/// <summary>
		/// This method is for the GUI Designer to test the program.
		/// </summary>
		/// <param name="testingNumber">The number of the testing code.</param>
		private void testingCode(int testingNumber)
		{
			switch(testingNumber)
			{
				case 0:
					_LogsCollection.Add(new UILogEntry{
						ImageSrc = "completed_icon.gif",
						FileName = "CS3215.txt",
						Status = "Completely processed",
						Message = "CS3215.txt has been uploaded to the intermediate storage."
					});
					_LogsCollection.Add(new UILogEntry{
						ImageSrc = "conflicting_icon.gif",
						FileName = "Proposal.pdf",
						Status = "Conflict",
						Message = "There is another different copy of Proposal.pdf found in the patch."
					});
					break;
			}
		}
		
		/// <summary>
        /// Get a collection of log entries.
        /// </summary>
		public ObservableCollection<UILogEntry> LogsCollection
    	{
			get
			{
				return _LogsCollection;
			}
		}
		
        /// <summary>
        /// Once a file in the source/storage directory is processed, a new log will be added.
        /// </summary>
        /// <param name="fileName">The name of the processed file.</param>
        /// <param name="status">The status of the process: 0 - Completely processed; 1 - Conflict.</param>
        /// <param name="message">A detailed message.</param>
		private void AddNewCurrentLog(string fileName, int status, string message)
		{
			string imageSrc = ""; //The source of the small icon appearing next to the filename in each log entry.
			string statusMessage = "";
			switch(status)
			{
                case ((int)LogStatus.Completed):
					imageSrc = "completed_icon.gif";
					statusMessage = "Completely processed";
					break;
                case ((int)LogStatus.Conflict):
					imageSrc = "conflicting_icon.gif";
					statusMessage = "Conflict";
					break;
				default:
                    Debug.Assert(true, "The value cannot be find in the enum LogStatus. Please ask Chun Lin for more info.");
					break;
			}

            //Add a new log entry to the log collection.
			_LogsCollection.Add(new UILogEntry{
				ImageSrc = imageSrc,
				FileName = fileName,
				Status = statusMessage,
				Message = message
			});
		}
		
		/*========================================================
		PART 10 ENDS HERE
		========================================================*/
	}
}