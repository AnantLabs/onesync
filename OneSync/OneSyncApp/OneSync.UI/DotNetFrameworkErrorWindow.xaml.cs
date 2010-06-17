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

namespace OneSync
{
	/// <summary>
	/// Interaction logic for DotNetFrameworkErrorWindow.xaml
	/// </summary>
	public partial class DotNetFrameworkErrorWindow : Window
	{
        // Start: Global variables.
        string[] dotNetFrameworkInfo;
        // End: Global variables.

		public DotNetFrameworkErrorWindow()
		{
			this.InitializeComponent();
			
            ProgramUpdateChecker dotNetFrameworkUpdateChecker = new ProgramUpdateChecker();
            dotNetFrameworkInfo = dotNetFrameworkUpdateChecker.GetRequiredDotNetInfo();
            lblDownloadLink.Content = "Download " + dotNetFrameworkInfo[0];
		}

		private void lblDownloadLink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            Process.Start(dotNetFrameworkInfo[1]);
            //Close OneSync
            Process.GetCurrentProcess().Kill();
		}
	}
}