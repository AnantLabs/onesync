using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncResult
    {
        private IList<SyncAction> skipped = new List<SyncAction>();
        private IList<SyncAction> errors = new List<SyncAction>();
        private IList<SyncAction> oks = new List<SyncAction> ();
        public SyncResult()
        {
        }
        public IList<SyncAction> Skipped
        {
            set { this.skipped = value; }
            get { return this.skipped; }
        }

        public IList<SyncAction> Errors
        {
            set { this.errors = value; }
            get { return this.errors; }
        }

        public IList<SyncAction> Ok
        {
            set { this.oks = value; }
            get { return this.oks; }
        }
    }
}
