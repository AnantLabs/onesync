using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{

    
    public delegate void SyncStartsHandler(object sender, SyncStartsEventArgs eventArgs);       
    public delegate void SyncCompletesHandler(object sender, SyncCompletesEventArgs eventArgs);        
    public delegate void SyncCancelledHandler(object sender, SyncCancelledEventArgs eventArgs);
    public delegate void SyncStatusChangedHandler(object sender, SyncStatusChangedEventArgs args);
    public delegate void SyncProgressChangedHandler(object sender, SyncProgressChangedEventArgs args);
   

    public enum SyncStatus
    {
        VERIFY_PATCH,
        APPLY_PATH,
        GENERATE_PATCH,
        UPDATE_DATA
    }  

    /// <summary>
    /// 
    /// </summary>    
    public class FileSyncAgent:BaseSyncAgent
    {       
        public event SyncStartsHandler OnStarted ;
        public event SyncCompletesHandler OnCompleted;
        public event SyncCancelledHandler OnCancelled;
        public event SyncStatusChangedHandler OnStatusChanged;
        public event SyncProgressChangedHandler OnProgressChanged;

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
            if (OnStarted != null) OnStarted (this, new SyncStartsEventArgs());
            if (OnProgressChanged != null) OnProgressChanged  (this, new SyncProgressChangedEventArgs(0));
            if (OnStatusChanged != null ) OnStatusChanged (this, new SyncStatusChangedEventArgs (SyncStatus.APPLY_PATH));
            Patch patch = new Patch(profile);
            patch.Apply();
            if (OnProgressChanged!= null) OnProgressChanged  (this, new SyncProgressChangedEventArgs(50)); 
            if (OnStatusChanged != null ) OnStatusChanged (this, new SyncStatusChangedEventArgs (SyncStatus.GENERATE_PATCH));
            patch.Generate();
            if (OnProgressChanged != null) OnProgressChanged (this, new SyncProgressChangedEventArgs (100));
            if (OnCompleted  != null ) OnCompleted (this, new SyncCompletesEventArgs ());
        }
    }
}
