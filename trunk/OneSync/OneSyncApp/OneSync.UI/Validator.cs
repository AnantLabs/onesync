using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;

namespace OneSync.UI
{
    static class Validator
    {
        public static string validateSyncJobName(string newName)
        {
            string errorMsg = null;

            if (isNullOrEmpty(newName))
                errorMsg = "SyncJob name cannot be empty.";
            else if (newName.Length > 50)
                errorMsg = "SyncJob name is too long.";

            return errorMsg;
        }

        // Does not check whether directory exist or not.
        public static string validateDirPath(string path)
        {

            if (isNullOrEmpty(path))
                return "Invalid directory path.";

            DirectoryInfo di = null;

            try
            {
                di = new DirectoryInfo(path);
            }
            catch (SecurityException ex)
            {
                return "Does not have permission to access directory.";
            }
            catch (PathTooLongException ex)
            {
                return "Path is too long.";
            }
            catch (ArgumentException ex)
            {
                return "Path contains illegal characters.";
            }

            // No errors
            return null;
        }
        

        public static string validateSyncDirs(string sync_source_dir, string intermediate_storage_dir)
        {
            if (validateDirPath(sync_source_dir) != null)
                return "Sync source directory is invalid.";

            if (validateDirPath(intermediate_storage_dir) != null)
                return "Intermediary storage directory is invalid.";

            // Remove trailing backslash if any
            sync_source_dir = sync_source_dir.Trim(new char[] {'\\'});
            intermediate_storage_dir = intermediate_storage_dir.Trim(new char[] {'\\'});

            //Check #1: Check whether the Sync Source Folder and the Intermediate Storage Folder having the same directory.
            if (sync_source_dir.Equals(intermediate_storage_dir))
                return "Folder to be synchronized cannot be the same as intermediary storage folder.";

            //Check #2a: Check whether the Sync Source Folder exists or not.
            if (!Directory.Exists(sync_source_dir))
                return "Folder to be synchronized not found.";


            //Check #2b: Check whether the Intermediate Storage Folder exists or not.
            if (!Directory.Exists(intermediate_storage_dir))
                return "Intermediary storage folder not found.";
            
            //Check #3: Check whether the Sync Source Folder and the Intermediate Storage Folder are the subdirectory of each other.
            if (sync_source_dir.Equals(intermediate_storage_dir.Substring(0, intermediate_storage_dir.LastIndexOf(@"\")))
                || intermediate_storage_dir.Equals(sync_source_dir.Substring(0, sync_source_dir.LastIndexOf(@"\"))))
                return "Folder to be synchronized cannot be in the intermediary storage folder or vice-versa.";

            return null;
        }

        
        private static bool isNullOrEmpty(string str)
        {
            if (string.IsNullOrEmpty(str)) return true;
            if (str.Trim() == "") return true;
            return false;
        }

    }


}
