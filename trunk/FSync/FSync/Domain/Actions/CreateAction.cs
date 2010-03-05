using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Synchronization
{
    public class CreateAction:SyncAction
    {
        private string dirtyFilesAbsDir = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetAbsRootDir">Target root directory of where this action is should be applied.</param>
        /// <param name="sourceID">Source ID of where this action is generated.</param>
        /// <param name="relFilePath">Absolute root directory of where this action is created.</param>
        /// <param name="relFilePath">Relative file path where file should be created.</param>
        /// <param name="fileHash">File hash of the file to be created.</param>
        /// <param name="dirtyFilesDir">Absolute root directory of where dirty files are stored.</param>
        public CreateAction(string targetAbsRootDir, string sourceID, string relFilePath, string fileHash,
                            string dirtyFilesDir)
            : base(targetAbsRootDir, sourceID, ChangeType.NEWLY_CREATED, relFilePath, fileHash)
        {
        }

        

        /// <summary>
        /// Destination file shouldn't exist
        /// </summary>
        public override void Execute()
        {
            string srcPath = Path.Combine(dirtyFilesAbsDir, relFilePath);
            string destPath = Path.Combine(targetAbsRootDir, relFilePath);

            if (!File.Exists(srcPath))
                throw new FileNotFoundException(srcPath + " not found.");

            File.Copy(srcPath, destPath);
        }
    }
}
