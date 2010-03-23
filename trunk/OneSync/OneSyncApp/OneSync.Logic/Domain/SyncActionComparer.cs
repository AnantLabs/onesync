using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncActionComparer:IEqualityComparer<SyncAction>
    {
        #region IEqualityComparer<SyncAction> Members

        public bool Equals(SyncAction x, SyncAction y)
        {
            return x.ActionId == y.ActionId;
        }

        public int GetHashCode(SyncAction obj)
        {
            return obj.RelativeFilePath.GetHashCode();
        }

        #endregion
    }
}
