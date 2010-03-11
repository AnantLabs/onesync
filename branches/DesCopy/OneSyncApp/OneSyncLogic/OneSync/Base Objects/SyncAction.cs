/*
 $Id$
 */
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
        public const string ACTION_TABLE = "ACTION_TABLE";
        public const string ACTION_ID = "ACTION_ID";
        public const string CHANGE_IN = "CHANGE_IN";
        public const string ACTION_TYPE = "ACTION_TYPE";
        public const string OLD_RELATIVE_PATH = "OLD_RELATIVE_PATH";
        public const string NEW_RELATIVE_PATH = "NEW_RELATIVE_PATH";
        public const string OLD_HASH = "OLD_HASH";
        public const string NEW_HASH = "NEW_HASH";
        #endregion columns of action table

        protected int actionId = 0;
        protected string relFilePath = "";
        protected string fileHash = ""; 
        protected string sourceID = "";
        protected ChangeType changeType = ChangeType.NO_CHANGED;

        /// <summary>
        /// Creates a new SyncAction.
        /// </summary>
        /// <param name="targetAbsRootDir">Target root directory of where this action is should be applied.</param>
        /// <param name="sourceID">Source ID of where this action is generated.</param>
        /// <param name="changeType">Action type.</param>
        /// <param name="relFilePath">Relative path of relevant/dirty file of this action.</param>
        /// <param name="fileHash">File hash of the relevant/dirty file of this action.</param>
        public SyncAction(int actionId, string sourceID, ChangeType changeType,
                          string relFilePath, string fileHash)
        {
            this.actionId = actionId;
            this.sourceID = sourceID;
            this.changeType = changeType;
            this.relFilePath = relFilePath;
            this.fileHash = fileHash;
        }


        /// <summary>
        /// Source ID of where this action is generated.
        /// </summary>
        public string SourceID
        {
            get
            {
                return sourceID;
            }
        }

        /// <summary>
        /// Action Id to uniquely identify action in action table
        /// </summary>
        public int ActionId
        {
            get { return this.actionId; }
        }
        /// <summary>
        /// Relative path of relevant/dirty file of this action.
        /// </summary>
        public string RelativeFilePath
        {
            get
            {
                return this.relFilePath;
            }
        }


        /// <summary>
        /// File hash of the relevant/dirty file of this action.
        /// </summary>
        public string FileHash
        {
            get
            {
                return this.fileHash;
            }
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

    }
}
