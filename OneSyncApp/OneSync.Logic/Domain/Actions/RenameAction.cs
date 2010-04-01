/*
 $Id: RenameAction.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class RenameAction : SyncAction
    {
        private string prevRelFilePath;


        /// <summary>
        /// Creates a Rename Action.
        /// </summary>
        /// <param name="targetAbsRootDir">Source root directory of where this action is created.</param>
        /// <param name="sourceID">Source ID of where this action is generated.</param>
        /// <param name="relFilePath">Relative file path of file after renamed.</param>
        /// <param name="prevRelFilePath">Relative file path before rename.</param>
        /// <param name="fileHash">File hash of file to be renamed.</param>
        public RenameAction(int actionId, string sourceID,
                            string relFilePath, string prevRelFilePath, string fileHash)
            : base(actionId, sourceID, ChangeType.RENAMED, relFilePath, fileHash)
        {
            this.prevRelFilePath = prevRelFilePath;
        }

        // Public Properties

        /// <summary>
        /// Previous relative file path of renamed file.
        /// </summary>
        public string PreviousRelativeFilePath
        {
            get { return prevRelFilePath; }
        }

    }
}
