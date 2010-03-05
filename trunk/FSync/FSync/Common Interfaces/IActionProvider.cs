using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Interface to define behaviors of ActionProvider
    /// </summary>
    public interface IActionsProvider
    {
        /// <summary>
        /// Get list of actions
        /// </summary>
        /// <returns></returns>
        IList<SyncAction> Load(SyncSource source);        
        /// <summary>
        /// Insert an action to data storage
        /// </summary>
        /// <param name="action"></param>
        void Insert (SyncAction action);
        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="action"></param>
        void Update(SyncAction action);
        /// <summary>
        /// Delete action
        /// </summary>
        /// <param name="action"></param>
        void Delete(SyncAction action);
    }
}
