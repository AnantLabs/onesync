/*
 Filename: MainWindow.xaml.cs
 Author:   Chun Lin
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
//The following two are imported for Aero Glass effect (Aero Glass Part 1/3).
using System.Runtime.InteropServices;
using System.Windows.Interop;

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
		string current_syncing_dir; //The directory of the chosen source folder.
        string storage_dir; //The storage directory.
        bool is_sync_job_created_previously = false; //The value is true if the sync job is not created currently, but previously.
        Synchronization.SQLiteProfileManager profileManager = new OneSync.Synchronization.SQLiteProfileManager(
            Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf('\\'))); //The SQLite profile manager from Thuat.
		DoubleAnimation instantNotificationAnimation = new DoubleAnimation(); //The animation of label_notification.
        Storyboard myStoryboard = new Storyboard(); //The storyboard of label_notification.
        //End: Global variables.
		
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
			instantNotificationAnimation.RepeatBehavior = RepeatBehavior.Forever;
			//instantNotificationAnimation.DecelerationRatio = 0.1;
			myStoryboard.Children.Add(instantNotificationAnimation);
			Storyboard.SetTargetName(instantNotificationAnimation, label_notification.Name);
			Storyboard.SetTargetProperty(instantNotificationAnimation, new PropertyPath("(UIElement.Opacity)"));
			myStoryboard.Begin(this);
			//End: Initiate the animation of label_notification.
			
			//Testing code (No.: 001) starts here.
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
			//Testing code (No.: 001) ends here.

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
			label_current_syncing_dir.Content = displaying_current_syncing_dir;
			label_current_syncing_dir_frontpage.Content = displaying_current_syncing_dir;

            //Import all the previous created existing sync job profiles.
            //Note that only the profile having the directory which is the current directory will be imported.
			//TO BE DISCUSSED: Note that for those profile has Sync Source Directory not exist anymore will be deleted.
            try
            {
                IList<Synchronization.Profile> profileItemsCollection = new Synchronization.SQLiteProfileManager(current_syncing_dir + @"\profiles").Load();
                foreach (Synchronization.Profile profileItem in profileItemsCollection)
                {
                    //Retrieve.
                    combobox_profile_name.Items.Add(profileItem.Name);
                    //TODO: Delete no longer exist profile.
                    //NOTE: Only not existing profile will be deleted.
                    //WAIT: Thuat is implementing the Delete function in OneSync.Synchronization.SQLiteProfileManager.
                }
            }catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
			
            //Show the log of current/just-finished sync job.
            textblock_show_log.Visibility = Visibility.Hidden;
			if(File.Exists(Log.returnLogReportPath(current_syncing_dir, true))) //To be changed. Depends on Naing.
			{
				textblock_show_log.Visibility = Visibility.Visible;
			}
            OneSyncStartingControlsVisibility();
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
        /// This method is in charge of updating the progress bar and showing the name of the file
        /// which is currently synced.
        /// </summary>
        /// <param name="currentProgress">An integer representing the percentage of work done. So, its value is only from 0 to 1.</param>
        /// <param name="syncingFileName">The name of the file which is currently synced.</param>
		private void CurrentProgress(int currentProgress, string syncingFileName)
		{
			progressbar_sync_progress.Value = currentProgress;
            if (syncingFileName.Length > 0) //Only show the filename if it is not empty.
            {
                label_current_processing_file.Content = syncingFileName + " is now under processing...";
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

        /// <summary>
        /// This methods handle the visibility of some controls in order to make sure that
        /// those controls will not be shown when the program first starts.
        /// </summary>
        private void OneSyncStartingControlsVisibility() 
        {
            //Controls to be hidden when the program first starts.
            label_current_processing_file.Visibility = Visibility.Hidden;
            progressbar_sync_progress.Visibility = Visibility.Hidden;
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
			label_message_frontpage.Visibility = sideVisibility;
			label_current_syncing_dir_frontpage.Visibility = sideVisibility;
			label_profile_message.Visibility = sideVisibility;
			combobox_profile_name.Visibility = sideVisibility;
			button_new_profile.Visibility = sideVisibility;
			rectangle_shadow.Visibility = sideVisibility;

            //Reset control's content.
            textbox_storage_path.Text = "";
        }

        /// <summary>
        /// This one will be called internally after the user clicks on the Sync button and before the sync process begins.
        /// It will show/hide some controls and enable/disable some of them.
        /// </summary>
        private void SyncProcessStarted() 
        {
			//Set the storage path.
			storage_dir = textbox_storage_path.Text;
			if(current_syncing_dir.Equals(storage_dir))
			{
				InstantNotification("Oops...  Your source folder and intermediate storage have the same directory. This is not allowed.");
			}
			else
			{
				InstantNotification("");

				//Rotate the OneSync Logo on the button.
				button_sync_copy.Visibility = Visibility.Visible;
				button_sync.Visibility = Visibility.Hidden;
				DoubleAnimation da = new DoubleAnimation();
				da.From = 0;
				da.To = 360;
				da.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
				da.RepeatBehavior = RepeatBehavior.Forever;
				RotateTransform rt = new RotateTransform();
				button_sync_copy.RenderTransform = rt;
				button_sync_copy.RenderTransformOrigin = new Point(0.5, 0.5);
				rt.BeginAnimation(RotateTransform.AngleProperty, da);
	
				//Show/hide some controls and enable/disable some of them.
				button_sync.IsEnabled = false;
				textbox_storage_path.IsEnabled = false;
				button_file_browser.IsEnabled = false;
				textblock_back_to_home.IsEnabled = false;
				progressbar_sync_progress.Visibility = Visibility.Visible;
				Expander.Visibility = Visibility.Visible;
				label_current_processing_file.Visibility = Visibility.Visible;
	
				//Testing code (No.: 002) starts here.
				label_current_processing_file.Content = "Scenario.docx is now under processing...";
				//Testing code (No.: 002) ends here.
	
				//Change the displaying text so that the user knows that OneSync is now syncing the source folder.
				label_message.Content = "You are now syncing";

                //Thuat and Desmond's sync logic takes actions!
                //Generate a Global Unique Identifier.
                string name = profile_name;
                Synchronization.SyncSource syncSource = new OneSync.Synchronization.SyncSource(System.Guid.NewGuid().ToString(), current_syncing_dir);
                Synchronization.IntermediaryStorage metaDataSource = new OneSync.Synchronization.IntermediaryStorage(storage_dir);
                Synchronization.Profile currentProfile = new OneSync.Synchronization.Profile(System.Guid.NewGuid().ToString(), name, syncSource, metaDataSource);
                Synchronization.FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(currentProfile);
                //Create profile
                if(!is_sync_job_created_previously)
                {
                    OneSync.Synchronization.UIProcess.CreateDataStore(current_syncing_dir, syncSource, metaDataSource);
                    OneSync.Synchronization.UIProcess.CreateProfile(current_syncing_dir, currentProfile);
                }
                currentAgent.Synchronize();
			}
        }
		
        /// <summary>
        /// This one will be called internally after the sync process is done.
        /// It will show/hide some controls and enable/disable some of them.
        /// </summary>
		private void SyncProcessDone()
		{
            if (File.Exists(Log.returnLogReportPath(current_syncing_dir, true))) //To be changed. Depends on Naing.
			{
				textblock_show_log.Visibility = Visibility.Visible;
			}
		}
		
		private void InstantNotification(string message)
		{
			label_notification.Content = message;
		}
		
        /// <summary>
        /// For Aero Glass effect (Part 3/3).
        /// </summary>
		private void AeroGlass()
		{
			var originalBackground = this.Background;

			try
			{
				/*IntPtr hwnd = new WindowInteropHelper(this).Handle;
				HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(hwnd);
				if (!(mainWindowSrc == null || !DwmIsCompositionEnabled()))
				{
					var margins = new Margins { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
					
					this.Background = Brushes.Transparent;
					mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
					DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
				}*/
			}
			catch (Exception) //If not Vista, paint background normally.
			{
				this.Background = originalBackground;
			}
		}

        /// <summary>
        /// When the window is ready, the Aero Glass effect will be triggered.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void Window_SourceInitialized(object sender, System.EventArgs e)
		{
			AeroGlass();
		}

        /// <summary>
        /// When the user mouse-overs the label displaying the source folder directory, a tool tip will appear.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void label_current_syncing_dir_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			label_current_syncing_dir.ToolTip = current_syncing_dir;
		}

        /// <summary>
        /// When the user clicks on the Sync button, the sync job will be run.
        /// It will show/hide some controls and enable/disable some of them.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            if(textbox_storage_path.Text.Length > 0)
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
        /// When the user clicks on the Show Log button, the .html version of log will be executed.
        /// User is then able to view the log file in the browser.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void textblock_show_log_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//View log file (The extension of the file should be .html).
            Process.Start(Log.returnLogReportPath(current_syncing_dir, true));
		}

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

        /// <summary>
        /// This method will be called when the user clicks on the File Browser "..." button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
		private void button_file_browser_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
			
			System.Windows.Forms.DialogResult result = dlg.ShowDialog();
			
			if(result == System.Windows.Forms.DialogResult.OK)
            {
                textbox_storage_path.Text = dlg.SelectedPath;
				textbox_storage_path.Focus();
            }
		}

        /// <summary>
        /// This method will be called when the user clicks on the New Job button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void button_new_profile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (combobox_profile_name.Text.Length > 0)
            {
				textblock_delete_profile.Visibility = Visibility.Hidden;
				InstantNotification(""); //Empty the notification message (if any).
                profile_name = combobox_profile_name.Text;
                ProfileCreationControlsVisibility(Visibility.Visible, Visibility.Hidden);
				Window.Title = combobox_profile_name.Text + " - OneSync";
                foreach (Synchronization.Profile item in (new Synchronization.SQLiteProfileManager(current_syncing_dir + @"\profiles").Load()))
                {
                    //Check to see if the profile is an existing profile or not.
                    //If yes, then it will import the storage directory to the program.
                    if (item.Name.Equals(profile_name))
                    {
                        is_sync_job_created_previously = true;
                        textbox_storage_path.Text = item.IntermediaryStorage.Path;
						textblock_delete_profile.Visibility = Visibility.Visible;
                        break;
                    }
                }
            }
            else 
            {
                InstantNotification("Please select/enter the name of your sync job.");
            }
        }

        private void textblock_back_to_home_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	ProfileCreationControlsVisibility(Visibility.Hidden, Visibility.Visible);
			Window.Title = "OneSync"; //Change back the menu title.
			combobox_profile_name.Text = "";
        }

        private void button_sync_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
        	button_sync.Source = new BitmapImage(new Uri("OneSync Transparent Logo.png", UriKind.Relative));
        }

        private void button_sync_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
        	button_sync.Source = new BitmapImage(new Uri("OneSync Transparent Logo (Inactive).png", UriKind.Relative));
        }

        private void textblock_rename_profile_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	//TODO: When user clicks on this link, the user should be able to rename the profile.
			//NOTE: Only existing profile can be created.
			//WAIT: Thuat is implementing the Update function in OneSync.Synchronization.SQLiteProfileManager.
        }

		/// <summary>
		/// When the user is typing something in the Profile Name combobox. If it detects the name is same as
		/// the name of an existing profile, then it will show the link to allow the user to rename the profile.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
        private void combobox_profile_name_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
        	foreach (String item in combobox_profile_name.Items)
            {
                //Check to see if the profile is an existing profile or not.
                //If yes, then it will show the rename profile link.
                if (item.Equals(combobox_profile_name.Text))
                {
                    textblock_rename_profile.Visibility = Visibility.Visible;
                    break;
                }
            }
			textblock_rename_profile.Visibility = Visibility.Hidden;
        }

        private void textblock_delete_profile_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	System.Windows.Forms.DialogResult result
			= System.Windows.Forms.MessageBox.Show("Are you sure you want to delete " + profile_name + "?", "Job Profile Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if(result == System.Windows.Forms.DialogResult.Yes)
			{
				//TODO: Delete the profile.
				//WAIT: Thuat is implementing the Delete function in OneSync.Synchronization.SQLiteProfileManager.
			}
        }
	}
}