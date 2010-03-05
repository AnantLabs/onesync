using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OneSync.Synchronization;
using System.IO;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;

namespace OneSync.UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            SyncSource source = new SyncSource(System.Guid.NewGuid().ToString(), textSyncSource.Text.Trim());
            MetaDataSource mdSource = new MetaDataSource(textMdSource.Text.Trim());
            Profile profile = new Profile(System.Guid.NewGuid().ToString(), textProfileName.Text.Trim(), source, mdSource);

            UIProcess.CreateProfile(Application.StartupPath, profile);
        }

        private void buttonCreateSchema_Click(object sender, EventArgs e)
        {
            SyncSource source = new SyncSource(System.Guid.NewGuid().ToString(), textSyncSource.Text.Trim());
            MetaDataSource mSource = new MetaDataSource(textMdSource.Text.Trim());
            UIProcess.CreateDataStore(Application.StartupPath,  source, mSource); 
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            IList<Profile> profiles = new SQLiteProfileManager(Application.StartupPath + @"\profiles").Load();
            if (profiles.Count >= 1)
            {
                FileSyncAgent agent = new FileSyncAgent(profiles[0]);
                agent.Synchronize();
            }
        }

        private void buttonLoadProfile_Click(object sender, EventArgs e)
        {
            IList<Profile> profiles = new SQLiteProfileManager(Application.StartupPath + @"\profiles").Load();
            foreach (Profile profile in profiles)
            {
                Console.WriteLine(profile.ID + "  " + profile.Name + "  " + profile.SyncSource.Path);
            }
        }

        private void buttonInsertMd_Click(object sender, EventArgs e)
        {
            IList<Profile> profiles = new SQLiteProfileManager(Application.StartupPath + @"\profiles").Load();
            if (profiles.Count >= 1)
            {
                FileMetaData currentItems = (FileMetaData)new SQLiteMetaDataProvider() .FromPath(profiles[0].SyncSource);
                new SQLiteMetaDataProvider(profiles[0].MetaDataSource.Path).Insert(currentItems);
            }
        }

        private void buttonLoadMd_Click(object sender, EventArgs e)
        {
            IList<Profile> profiles = new SQLiteProfileManager(Application.StartupPath + @"\profiles").Load();
            if (profiles.Count >= 1)
            {
                FileMetaData data = UIProcess.LoadMetadata(profiles[0]);
                foreach (FileMetaDataItem item in data.MetaDataItems)
                {
                    Console.WriteLine(item.FullName + ": " + item.HashCode + ":  " 
                        + item.LastModifiedTime.ToShortDateString() + ": " + item.RelativePath+
                        ": " + item.SourceId 
                        );
                }
            }
        }

        private void buttonLoadActions_Click(object sender, EventArgs e)
        {
            IList<Profile> profiles = new SQLiteProfileManager(Application.StartupPath + @"\profiles").Load();
            if (profiles.Count >= 1)
            {
                IList<SyncAction> actions =  new SQLiteActionProvider(profiles[0]).Load(profiles[0].SyncSource);
                foreach (SyncAction action in actions)
                {
                    Console.WriteLine(action.TargetAbsoluteRootDir);
                }
            }            
        }

     }
}
