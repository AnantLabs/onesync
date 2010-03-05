using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public enum ChangeType
    {
        NEWLY_CREATED,
        DELETED,
        MODIFIED,
        RENAMED,
        NO_CHANGED
    }

    /// <summary>
    /// This class is the base class for all the sync action (rename, delete, create,...)
    /// </summary>
    public abstract class SyncAction
    {       
        #region columns of action table
        public const string TABLE_NAME = "ACTION_TABLE";
        public const string CHANGE_IN = "CHANGE_IN";
        public const string ACTION = "ACTION";
        public const string OLD = "OLD";
        public const string NEW = "NEW";
        public const string OLD_HASH = "OLD_HASH";
        public const string NEW_HASH = "NEW_HASH";
        #endregion columns of action table



        /// <summary>
        /// Sync source id
        /// </summary>
        protected string changeIn = "";
        
        protected ChangeType changeType = ChangeType.NO_CHANGED;

        protected string syncSource = "";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="change_in"></param>
        /// <param name="changeType"></param>
        public SyncAction(string syncSource,  string change_in, ChangeType changeType)
        {
            this.syncSource = syncSource;            
            this.changeIn = change_in;
            this.changeType = changeType;
        }

        /// <summary>
        /// Sync source id where changes are made
        /// </summary>
        public string ChangeIn
        {
            get
            {
                return changeIn;
            }
        }
        /// <summary>
        /// get the sync source folder
        /// </summary>
        public string SyncSourcePath
        {
            get { return this.syncSource; }
        }

        /// <summary>
        /// Type of change (rename, delete,...)
        /// </summary>
        public ChangeType ChangeType
        {
            get
            {
                return changeType;
            }
        }       

        public abstract void Execute();
        
    }
}
