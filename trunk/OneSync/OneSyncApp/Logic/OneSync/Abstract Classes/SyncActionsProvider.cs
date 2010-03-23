using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Class to manage sync actions.
    /// </summary>
    public abstract class SyncActionsProvider
    {

        // Path to where Actions are saved.
        protected string _storagePath;

        /// <summary>
        /// Creates a SyncSource Provider
        /// </summary>
        /// <param name="storagePath">Location where all Actions are saved to or loaded from.</param>
        public SyncActionsProvider(string storagePath)
        {
            _storagePath = storagePath;   
        }

        
        /// <summary>
        /// Load all actions based specified SourceID.
        /// </summary>
        /// <param name="srcActionID">SourceID to be based on.</param>
        /// <param name="loadOther">
        /// true to load Actions with different SourceID as argument.
        /// false to load all Actions with same SourceID as argument.</param>
        /// <returns></returns>
        public abstract IList<SyncAction> Load(string sourceID, SourceOption option);


        /// <summary>
        /// Adds a SyncAction.
        /// </summary>
        /// <param name="action">SyncAction to be added</param>
        /// <returns>true if successfully added.</returns>
        public abstract bool Add(SyncAction action);

        /// <summary>
        /// Adds a list of SyncActions.
        /// </summary>
        /// <param name="actions">Actions to be added.</param>
        /// <returns>true if all actions successfully added.</returns>
        public abstract bool Add(IList<SyncAction> actions);

        
        /// <summary>
        /// Delete saved actions with the same ActionID as action argument.
        /// </summary>
        /// <param name="action">Saved actions which has the same ActionID as this action will be deleted.</param>
        /// <returns>true if successfully deleted.</returns>
        public abstract bool Delete(SyncAction action);

        
        /// <summary>
        /// Delete all saved actions which has the same ActionID as any of the actions
        /// in the actions argument.
        /// </summary>
        /// <param name="actions">Saved actions that has the same ActionID as any actions in this list will be deleted.</param>
        /// <returns>true if deleted successfully.</returns>
        public abstract bool Delete(IList<SyncAction> actions);


        /// <summary>
        /// Deletes actions based on specified SourceID.
        /// </summary>
        /// <param name="srcID">SourceID to be based on.</param>
        /// <param name="deleteOther">
        /// true to delete actions with different SourceID as argument.
        /// false to delete actions with srcActionID as argument.
        /// </param>
        /// <returns></returns>
        public abstract bool Delete(string sourceID, SourceOption option);

        /// <summary>
        /// Create default schema
        /// </summary>
        public abstract void CreateSchema();

        #region Public Properties

        /// <summary>
        /// Location where all SyncSource are saved to or loaded from.
        /// </summary>
        public string StoragePath
        {
            get { return _storagePath; }
        }

        #endregion


    }
}
