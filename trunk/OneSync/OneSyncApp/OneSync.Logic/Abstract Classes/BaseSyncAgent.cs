/*
 $Id: BaseSyncAgent.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{    
    
    public abstract class BaseSyncAgent:ISyncAgent
    {
     
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

        
    }
}
