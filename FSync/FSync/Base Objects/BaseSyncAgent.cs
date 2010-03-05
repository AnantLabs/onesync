using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{     
    public abstract class BaseSyncAgent:ISyncAgent
    {
        #region events and delegate declarations
        public delegate void SyncStartsHandler(object sender, SyncStartsEventArgs eventArgs);
        public event SyncStartsHandler SyncStartEvent;

        public delegate void SyncCompletesHandler(object sender, SyncCompletesEventArgs eventArgs);
        public event SyncCompletesHandler SyncCompletesEvent;

        public delegate void SyncCancelledHandler(object sender, SyncCancelledHandler eventArgs);
        public event SyncCancelledHandler SyncCancelledEvent;
        #endregion events and delegate declarations

        protected ISyncProvider provider;
        protected Profile profile;

        public BaseSyncAgent()
        {
        }


        public BaseSyncAgent(Profile profile)
        {
            this.profile = profile;
        }
        
        public Profile Profile
        {
            set
            {
                this.profile = value;
            }
            get
            {
                return this.profile;
            }
        }

        public ISyncProvider SyncProvider
        {
            set
            {
                this.provider = value;
            }
            get
            {
                return this.provider;
            }
        }

        public abstract void Synchronize();    
    }
}
