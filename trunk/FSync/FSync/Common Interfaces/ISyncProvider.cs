using System;
using System.Collections.Generic;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Sync provider retrieve and update the metadata
    /// </summary>
    public interface ISyncProvider
    {
        /// <summary>
        /// Generates the sync actions required for 2 sync sources to be synchronized.
        /// </summary>
        /// <returns></returns>
        IList<SyncAction> GenerateActions();
    }
}
