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
using OneSync.UI;
using System.Diagnostics;
using System.Resources;

namespace OneSync
{
	/// <summary>
	/// Interaction logic for DotNetFrameworkErrorWindow.xaml
	/// </summary>
	public partial class DotNetFrameworkErrorWindow : Window
	{
        // Start: Global variables.
        ResourceManager m_ResourceManager = new ResourceManager(Properties.Settings.Default.LanguageResx,
                                    System.Reflection.Assembly.GetExecutingAssembly());
        string[] dotNetFrameworkInfo;
        // End: Global variables.

		public DotNetFrameworkErrorWindow()
		{
            this.InitializeComponent();

            this.Title = m_ResourceManager.GetString("win_dotNetFrameworkRequirement");

            lblDescription1.Content = m_ResourceManager.GetString("lbl_dotNetFrameworkDescriptionText1");
            lblDescription2.Content = m_ResourceManager.GetString("lbl_dotNetFrameworkDescriptionText2");

            ProgramUpdateChecker dotNetFrameworkUpdateChecker = new ProgramUpdateChecker();
            dotNetFrameworkInfo = dotNetFrameworkUpdateChecker.GetRequiredDotNetInfo();
            lblDownloadLink.Content = m_ResourceManager.GetString("lbl_download") + " " + dotNetFrameworkInfo[0];
		}

		private void lblDownloadLink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            Process.Start(dotNetFrameworkInfo[1]);
            //Close OneSync using brute force because there is a hidden window (the main window) running.
            Process.GetCurrentProcess().Kill();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			//Close OneSync using brute force because there is a hidden window (the main window) running.
			Process.GetCurrentProcess().Kill();
		}
	}
}