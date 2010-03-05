using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class Patcher:ISyncVerifier
    {        
        public Patcher()
        {            
        }

        #region ISyncVerifier Members

        public bool Verify(Patch patch)        
        {            
            return true;
        }

        public void Apply(Patch patch)
        {
            foreach (SyncAction action in patch.Actions)
            {
                action.Execute();
            }
        }
        #endregion

       

        #region IFileSyncVerifier Members


        public bool VerifyPatchApplied()
        {
            //check files copied

            //check metadata updated
            return true;
        }

        #endregion
    }
}
