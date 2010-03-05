using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class FileSyncAgent:BaseSyncAgent
    {
        IList<SyncAction> syncActions = new List<SyncAction>();
        public FileSyncAgent():base()
        {            
        }

        public FileSyncAgent(Profile profile)
            : base(profile)
        {            
        }

        public override void Synchronize()
        {  
            #region Apply and Verify Patch Applied
            IList<SyncAction> actions = new SQLiteActionProvider(profile).Load(profile.SyncSource);
            Patch patch = new Patch(profile.SyncSource, profile.MetaDataSource, actions);
            if (patch.Verify(true)) patch.Apply();            
            #endregion Apply and Verify Patch Applied

            #region generate patch            
            //load metadata from database
            FileMetaData storedItems = (FileMetaData) new SQLiteMetaDataProvider(Profile).Load(Profile.SyncSource);
            //generate metadata of a folder
            FileMetaData currentItems =(FileMetaData) new SQLiteMetaDataProvider (Profile).FromPath(Profile.SyncSource);            
            FileSyncProvider syncProvider = new FileSyncProvider(currentItems, storedItems);
            
            //generate list of sync actions by comparing 2 metadata
            IList<SyncAction> newActions =  syncProvider.EnumerateChanges();
            

            /*
            Patch patch = new Patch(Profile.SyncSource.Path, Profile.MetaDataSource.Path, actions);
            FileSyncVerifier verifier = new FileSyncVerifier(Profile);
            verifier.Verify(patch);
            new SQLiteActionProvider(Profile.MetaDataSource.Path).Insert(actions);           
            */
            //Copy dirty items to intermediate drives

            //Update metadata
            //new SQLiteMetaDataProvider(Profile.MetaDataSource)
        #endregion generate patch
        }

        public IList<SyncAction> SyncActions
        {
            get
            {
                return this.syncActions;
            }
        }    

        private void ApplyPatch(Patch patch)
        {
            
        }       
    }
}
