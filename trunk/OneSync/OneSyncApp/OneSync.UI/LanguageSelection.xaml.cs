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
using System.Diagnostics;

namespace OneSync
{
	/// <summary>
	/// Interaction logic for LanguageSelection.xaml
	/// </summary>
	public partial class LanguageSelection : Window
	{
		public LanguageSelection()
		{
			this.InitializeComponent();
		}

		private void btnEnglish_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Properties.Settings.Default.LanguageResx = "OneSync.LocalizationOneSync";
            
            MessageBoxResult gettingNewVersion = MessageBox.Show("Press OK to save the setting and shut down OneSync. Press Cancel to undo.", "OneSync - English", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (gettingNewVersion == MessageBoxResult.OK)
            {
                Properties.Settings.Default.Save();
                //Close OneSync using brute force because there is a hidden window (the main window) running.
                Process.GetCurrentProcess().Kill();
            }
		}

		private void btnChinese_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Properties.Settings.Default.LanguageResx = "OneSync.LocalizationOneSyncChinese";
            Properties.Settings.Default.Save();
            MessageBoxResult gettingNewVersion = MessageBox.Show("请在接受后重启OneSync。", "OneSync - 新马华文", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (gettingNewVersion == MessageBoxResult.OK)
            {
                Properties.Settings.Default.Save();
                //Close OneSync using brute force because there is a hidden window (the main window) running.
                Process.GetCurrentProcess().Kill();
            }
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            //Close OneSync using brute force because there is a hidden window (the main window) running.
            Process.GetCurrentProcess().Kill();
		}
	}
}