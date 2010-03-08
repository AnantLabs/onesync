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
        public event SyncStartsHandler SyncStartEvent;
        public event SyncCompletesHandler SyncCompletesEvent;
        public event SyncCancelledHandler SyncCancelledEvent;
        public event SyncStatusChangedHandler SyncStatusChangedEvent;
        public event SyncProgressChangedHandler SyncProgressChangedEvent;

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
            Patch patch = new Patch(profile);
            patch.Apply();                      
            patch.Generate();
        }
    }
}
