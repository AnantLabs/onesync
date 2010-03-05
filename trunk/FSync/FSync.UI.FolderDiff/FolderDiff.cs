using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OneSync.Files;
using System.IO;

namespace OneSync.UI.FolderDiff
{
    delegate void Comparer ();
    delegate void ClearListItemsHandler ();
    delegate void AddListItemHandler (ListViewItem[] items);
    
    public partial class FolderDiff : Form
    {
        private FileChangeDetection sourceMonitor;
        private FileChangeDetection destinationMonitor; 
        public FolderDiff()
        {
            InitializeComponent();                      
        }
        
        private void buttonBrowseSourceFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog1 = new FolderBrowserDialog();
            dialog1.Description = "Select folder";
            dialog1.RootFolder = Environment.SpecialFolder.Desktop;
            DialogResult result = dialog1.ShowDialog();
            if (result == DialogResult.OK)            {
                
                try
                {
                    sourceMonitor = new FileChangeDetection(dialog1.SelectedPath);
                    textSourceFolder.Text = dialog1.SelectedPath;
                }
                catch (FileNotFoundException ffe)
                {
                    MessageBox.Show("Folder couldn't be read");
                    return;
                }
                
            }
        }

        private void buttonBrowseDestinationFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog1 = new FolderBrowserDialog();
            dialog1.Description = "Select folder";
            dialog1.RootFolder = Environment.SpecialFolder.Desktop;
            DialogResult result = dialog1.ShowDialog();
            if (result == DialogResult.OK)
            {                
                try
                {
                    destinationMonitor = new FileChangeDetection(dialog1.SelectedPath);
                    textDestinationFolder.Text = dialog1.SelectedPath;
                }
                catch (FileNotFoundException ffe)
                {
                    MessageBox.Show("Folder couldn't be read");
                    return;
                }
                
            }
        }

        private void buttonCompare_Click(object sender, EventArgs e)
        {
            if (!( Directory.Exists (textSourceFolder.Text.Trim ()) && Directory.Exists(textDestinationFolder.Text.Trim ())))
            {
                MessageBox.Show("Source or destination folder does not exists");
                return;
            }
            if (checkBoxMonitor.Checked)
            {
                destinationMonitor.Created += new System.IO.FileSystemEventHandler(destination_Created);
                destinationMonitor.Deleted += new System.IO.FileSystemEventHandler(destination_Deleted);
                destinationMonitor.Changed += new System.IO.FileSystemEventHandler(destination_Changed);
                destinationMonitor.Renamed += new System.IO.RenamedEventHandler(destination_Renamed);

                sourceMonitor.Created += new FileSystemEventHandler(sourceMonitor_Created);
                sourceMonitor.Deleted += new FileSystemEventHandler(sourceMonitor_Deleted);
                sourceMonitor.Changed += new FileSystemEventHandler(sourceMonitor_Changed);
                sourceMonitor.Renamed += new RenamedEventHandler(sourceMonitor_Renamed);
                buttonCompare.Enabled = false;
            }
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
            
           
            
        }
        void CompareComplete(IAsyncResult result)
        {
           
        }

        void sourceMonitor_Renamed(object sender, RenamedEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void sourceMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void sourceMonitor_Deleted(object sender, FileSystemEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void sourceMonitor_Created(object sender, FileSystemEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void destination_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void destination_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void destination_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        void destination_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }


        private void Compare()
        {
            long start = System.DateTime.Now.Ticks;
            Console.WriteLine("Start time: " + start );
            List<ListViewItem> items = new List<ListViewItem>();
            FolderComparer comparer = null;
            try
            {
                comparer = new FolderComparer(textSourceFolder.Text, textDestinationFolder.Text);
            }
            catch (FileNotFoundException au)
            {
                MessageBox.Show("Folder couldn't be read");
                return;
            }
            catch (UnauthorizedAccessException ua)
            {
                MessageBox.Show("Access denied");
                return;
            }
            
                                   
            foreach (FileInfo info in comparer.GetDestinationOnly())
            {
                ListViewItem item = new ListViewItem(new string []{ (items.Count + 1).ToString () , info.FullName , "Only in destination folder"  });
                item.BackColor = Color.DarkRed;
                item.ForeColor = Color.White;
                items.Add(item);
            }
            
            foreach (FileInfo info in comparer.GetSourceOnly ())
            {
                ListViewItem item = new ListViewItem(new string[] { (items.Count + 1).ToString(), info.FullName, "Only in source folder" });
                item.BackColor = Color.Yellow;
                item.ForeColor = Color.Black;
                items.Add(item);
            }
                        
            foreach (FileInfo info in comparer.GetCommonFiles())
            {
                
                ListViewItem item = new ListViewItem(new string[] { (items.Count + 1).ToString(),                     
                    FileUtils.GetRelativePath(comparer.Source, info.FullName), "Identical" });
                item.BackColor = Color.AliceBlue;
                item.ForeColor = Color.Black;
                items.Add(item);
            }


            if (checkBoxShowFolders.Checked)
            {
                
                foreach (DirectoryInfo info in comparer.GetCommonFolders())
                {
                    
                    ListViewItem item = new ListViewItem(new string[] { (items.Count + 1).ToString(), info.FullName, "Identical" });
                    item.BackColor = Color.AliceBlue;
                    item.ForeColor = Color.Black;
                    items.Add(item);
                }

                
                foreach (DirectoryInfo info in comparer.GetFoldersInDestinationOnly())
                {

                    ListViewItem item = new ListViewItem(new string[] { (items.Count + 1).ToString(), info.FullName, "Only in destination folder" });
                    item.BackColor = Color.DarkRed;
                    item.ForeColor = Color.White;
                    items.Add(item);
                }

                
                foreach (DirectoryInfo info in comparer.GetFoldersInSourcesOnly())
                {
                    ListViewItem item = new ListViewItem(new string[] { (items.Count + 1).ToString(), info.FullName, "Only in source folder" });
                    item.BackColor = Color.Yellow;
                    item.ForeColor = Color.Black;
                    items.Add(item);
                }
            }

            if (listViewDifference.InvokeRequired)
            {
                listViewDifference.BeginInvoke(new AddListItemHandler(AddListItems),new object []{items.ToArray<ListViewItem>()});

            }
            long end = System.DateTime.Now.Ticks;
            Console.WriteLine("End time: " + end);
            Console.WriteLine("Time: " + (end - start ));
        }

        private void AddListItems(ListViewItem[] items)
        {
            listViewDifference.Items.Clear();
            listViewDifference.Items.AddRange(items); 
        }

      
        
        private void FolderDiff_Load(object sender, EventArgs e)
        {

        }

        private void buttonFileDiff_Click(object sender, EventArgs e)
        {
          
        }

        private void checkBoxMonitor_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxMonitor.Checked)
            {
                sourceMonitor.EnableRaisingEvents = true;
                destinationMonitor.EnableRaisingEvents = true;
                buttonCompare.Enabled = false;
            }
            else
            {
                sourceMonitor.EnableRaisingEvents = false;
                destinationMonitor.EnableRaisingEvents = false;
                buttonCompare.Enabled = true;
            }
        }

        private void checkBoxShowFolders_CheckedChanged(object sender, EventArgs e)
        {
            Comparer comparer = new Comparer(Compare);
            IAsyncResult compareResult = comparer.BeginInvoke(new AsyncCallback(CompareComplete), null);
        }

        private void buttonMirror_Click(object sender, EventArgs e)
        {

        }

    }
}
