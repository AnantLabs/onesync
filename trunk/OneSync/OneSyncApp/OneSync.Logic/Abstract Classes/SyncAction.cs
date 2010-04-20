//Coded by Koh Eng Tat Desmond
/*
 $Id: SyncAction.cs 90 2010-03-15 05:39:17Z deskohet $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

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

    public enum ConflictResolution
    {
        OVERWRITE,
        DUPLICATE_RENAME,
        NONE
    }

    /// <summary>
    /// This class is the base class for all the sync action (rename, delete, create,...)
    /// </summary>
    public abstract class SyncAction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //status of the action
        protected ConflictResolution conflictResolution = ConflictResolution.NONE;
        protected bool _skip = false;

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

        public bool Skip
        {
            get { return _skip; }
            set
            {
                if (value != Skip)
                {
                    _skip = value;
                    OnPropertyChanged("Skip");
                }
            }
        }


        /// <summary>
        /// Source ID of where this action is generated.
        /// </summary>
        public string SourceID
        {
            get { return sourceID; }
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
            get { return this.relFilePath; }
        }


        /// <summary>
        /// File hash of the relevant/dirty file of this action.
        /// </summary>
        public string FileHash
        {
            get { return this.fileHash; }
        }

        /// <summary>
        /// Type of change (rename, delete,...)
        /// </summary>
        public ChangeType ChangeType
        {
            get { return changeType; }
        }

        public ConflictResolution ConflictResolution
        {
            get { return this.conflictResolution; }
            set { conflictResolution = value; }
        }

        protected virtual void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

    }
}
