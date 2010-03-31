using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using OneSync.Synchronization;

namespace OneSync.UI
{
	public partial class SyncActionsListView
	{
        
		public SyncActionsListView()
		{
			this.InitializeComponent();
		}

        public IEnumerable<SyncAction> ItemsSource
        {
            get { return (IEnumerable<SyncAction>)lvActions.ItemsSource; }
            set { lvActions.ItemsSource = value; }
        }     
        
        private void checkBoxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            //List<SyncAction> actions = new List<SyncAction>((IEnumerable<SyncAction>) lvActions.ItemsSource);
            IEnumerable <SyncAction> actions = (IEnumerable <SyncAction>)lvActions.ItemsSource;
            if (actions == null) return;
            int index = 0;
            foreach (SyncAction action in actions)
            {                
                ListViewItem lvItem = UserControl.lvActions.ItemContainerGenerator.ContainerFromIndex(index++) as ListViewItem;
                ContentPresenter lvItemPresenter = UIHelper.FindVisualChild<ContentPresenter>(lvItem);
                DataTemplate template = lvItemPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)template.FindName("checkBoxSkip", lvItemPresenter);
                checkBox.IsChecked = true ;                  
            }
            //lvActions.ItemsSource = actions;
        }

        private void checkBoxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            IEnumerable <SyncAction> actions = (IEnumerable<SyncAction>) lvActions.ItemsSource;
            if (actions == null) return ;
            int index = 0;
            foreach (SyncAction action in actions)
            {
                ListViewItem lvItem = UserControl.lvActions.ItemContainerGenerator.ContainerFromIndex(index++) as ListViewItem;
                ContentPresenter lvItemPresenter = UIHelper.FindVisualChild<ContentPresenter>(lvItem);
                DataTemplate template = lvItemPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)template.FindName("checkBoxSkip", lvItemPresenter);
                checkBox.IsChecked = false;                
            }            
        }
    }

    #region Converters
	
	[ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
    
    [ValueConversion(typeof(ConflictResolution), typeof(Visibility))]
    public class ConflictVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((ConflictResolution)value != ConflictResolution.NONE)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    [ValueConversion(typeof(ConflictResolution), typeof(int))]
    public class ConflictResolutionToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ConflictResolution)value)
            {
                case ConflictResolution.DUPLICATE_RENAME:
                    return 0;
                case ConflictResolution.OVERWRITE:
                    return 1;
                case ConflictResolution.NONE:
                    return -1;
                default:
                    return -1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return ConflictResolution.DUPLICATE_RENAME;
                case 1:
                    return ConflictResolution.OVERWRITE;
                case -1:
                    return ConflictResolution.NONE;
                default:
                    return ConflictResolution.NONE;
            }
        }
    }


    [ValueConversion(typeof(ChangeType), typeof(string))]
    public class ChangeTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ChangeType)value)
            {
                case ChangeType.NEWLY_CREATED:
                    return "Copy";
                case ChangeType.DELETED:
                    return "Delete";
                case ChangeType.MODIFIED:
                    return "Copy";
                case ChangeType.RENAMED:
                    return "Rename";
                default:
                    return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(ChangeType), typeof(string))]
    public class ChangeTypeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ChangeType)value)
            {
                case ChangeType.NEWLY_CREATED:
                    return "Green";
                case ChangeType.DELETED:
                    return "Red";
                case ChangeType.MODIFIED:
                    return "Blue";
                case ChangeType.RENAMED:
                    return "Black";
                default:
                    return "Black";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

}