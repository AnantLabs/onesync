using System;
using System.IO;
using System.Security;
using System.Threading;
using OneSync.Synchronization;

namespace OneSync.Synchronization
{
    static class Validator
    {
        public static bool SyncJobParamsValidated (string name, string sourceAbsolute, string immediate)
        {
            if (name == null || name.Equals(String.Empty))
                throw new SyncJobException("Sync job name can't be empty");

            if (name.Length > 50)
                throw new SyncJobException("Sync job name can't exceed 50 characters");

            return IsDirectoryValidated(immediate)
                   && IsDirectoryValidated(sourceAbsolute)
                   && IsRecursive(sourceAbsolute, immediate);
        }
        
        public static bool IntermediaryMovable (string baseFolder, string sourceId)
        {
            string absolutePath = Path.Combine(baseFolder, Configuration.DATABASE_NAME);
            if (!File.Exists(absolutePath)) return true;

            SyncSourceProvider sourceProvider = SyncClient.GetSyncSourceProvider(baseFolder);
            return sourceProvider.SourceExist(sourceId);
        }

        private static bool IsRecursive (string source, string intermediate)
        {
            //Check #3: Check whether the Sync Source Folder and the Intermediate Storage Folder are the subdirectory of each other.
            if(source.Equals(intermediate))
                throw new SyncJobException("Source and intermediate path can't be the same");

            if (intermediate.LastIndexOf(@"\") > 0 && source.LastIndexOf(@"\") > 0)
            {
                if (source.Contains(intermediate.Substring(0, intermediate.LastIndexOf(@"\")))
                || intermediate.Contains(source.Substring(0, source.LastIndexOf(@"\"))))
                    throw new SyncJobException("Sync between folder and its sub-folder is not allowed");
            }
            return true;
        }

        private static bool IsDirectoryValidated (string absolute)
        {
            if (absolute == null || absolute.Equals(String.Empty))
                throw  new SyncJobException("Source or Intermediary storage is empty");

            if (!Directory.Exists(absolute))
                throw  new SyncJobException("Directory " + absolute + " doesn't exist");

            if (absolute.Length > 260)
                throw new SyncJobException("The directory length can't exceed " + 260 + " characters");
            return true;
        }

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
                return "Folder to be synchronized cannot be found.";


            //Check #2b: Check whether the Intermediate Storage Folder exists or not.
            if (!Directory.Exists(intermediate_storage_dir))
                return "Intermediary storage folder cannot be found.";
            
            //Check #3: Check whether the Sync Source Folder and the Intermediate Storage Folder are the subdirectory of each other.
            if (intermediate_storage_dir.LastIndexOf(@"\") > 0 && sync_source_dir.LastIndexOf(@"\") > 0)
            {
                if (sync_source_dir.Equals(intermediate_storage_dir.Substring(0, intermediate_storage_dir.LastIndexOf(@"\")))
                || intermediate_storage_dir.Equals(sync_source_dir.Substring(0, sync_source_dir.LastIndexOf(@"\"))))
                    return "Folder to be synchronized cannot be in the intermediary storage folder or vice-versa.";
            }

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
