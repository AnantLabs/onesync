/*
 $Id: DeleteAction.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class DeleteAction : SyncAction
    {

        /// <summary>
        /// Creates a Delete Action.
        /// </summary>
        /// <param name="targetAbsRootDir">Source root directory of where this action is created.</param>
        /// <param name="sourceID">Source ID of where this action is generated.</param>
        /// <param name="relFilePath">Relative file path of file to be deleted.</param>
        /// <param name="fileHash">File hash of file to be deleted.</param>
        public DeleteAction(int actionId, string sourceID, string relFilePath, string fileHash)
            : base(actionId, sourceID, ChangeType.DELETED, relFilePath, fileHash)
        {
        }


    }
}
