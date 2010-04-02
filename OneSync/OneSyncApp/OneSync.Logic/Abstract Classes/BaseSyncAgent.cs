/*
 $Id: BaseSyncAgent.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{    
    
    public abstract class BaseSyncAgent
    {
        protected SyncJob job;

        public BaseSyncAgent()
        {
        }


        public BaseSyncAgent(SyncJob job)
        {
            this.job = job;
        }
        
        public SyncJob Job
        {
            set
            {
                this.job = value;
            }
            get
            {
                return this.job;
            }
        }
        
    }
}
