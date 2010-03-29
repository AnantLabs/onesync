using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using OneSync.Synchronization;
using System.Collections.Generic;
using System.Linq;

namespace OneSync.UI
{
	public partial class WinSyncPreview
	{
        private IEnumerable<SyncAction> _actions;

        /// <summary>
        /// Instantiate a sync preview window with specified sync actions.
        /// </summary>
        /// <param name="syncActions">All sync actions to be dispayed for preview.</param>
        public WinSyncPreview(IEnumerable<SyncAction> syncActions)
		{
			this.InitializeComponent();
            this._actions = syncActions;

            lvPreview.ItemsSource = _actions;
		}

        void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter();
        }

        private void filter()
        {
            string searchTerm = txtFilter.Text.Trim().ToLower();

            if (cmbFilter.SelectedIndex == 0)
            {
                lvPreview.ItemsSource =
                    from a in _actions
                    where a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
            else if (cmbFilter.SelectedIndex == 1)
            {
                lvPreview.ItemsSource =
                    from a in _actions
                    where a.ConflictResolution != ConflictResolution.NONE
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
            else if (cmbFilter.SelectedIndex == 2)
            {
                lvPreview.ItemsSource =
                    from a in _actions
                    where (a.ChangeType == ChangeType.NEWLY_CREATED || a.ChangeType == ChangeType.MODIFIED)
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
            else if (cmbFilter.SelectedIndex == 3)
            {
                lvPreview.ItemsSource =
                    from a in _actions
                    where a.ChangeType == ChangeType.DELETED
                          && a.RelativeFilePath.ToLower().Contains(searchTerm)
                    select a;
            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
	}
}