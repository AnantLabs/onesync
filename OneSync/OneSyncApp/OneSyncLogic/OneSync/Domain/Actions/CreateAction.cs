/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class CreateAction : SyncAction
    {
        /// <summary>
        /// To
        /// </summary>
        /// <param name="targetAbsRootDir">Target root directory of where this action is should be applied.</param>
        /// <param name="sourceID">Source ID of where this action is generated.</param>
        /// <param name="relFilePath">Absolute root directory of where this action is created.</param>
        /// <param name="relFilePath">Relative file path where file should be created.</param>
        /// <param name="fileHash">File hash of the file to be created.</param>
        /// <param name="dirtyFilesDir">Absolute root directory of where dirty files are stored.</param>
        public CreateAction(int actionId, string sourceID, string relFilePath, string fileHash)
            : base(actionId, sourceID, ChangeType.NEWLY_CREATED, relFilePath, fileHash)
        {
        }


    }
}
