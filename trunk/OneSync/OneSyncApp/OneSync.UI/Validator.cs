//Coded by Goh Chun Lin
using System;
using System.IO;
using System.Security;
using System.Threading;
using OneSync.Synchronization;
using System.Collections.Generic;
using System.Resources;

namespace OneSync.Synchronization
{
    static class Validator
    {
        public static ResourceManager m_ResourceManager = new ResourceManager(Properties.Settings.Default.LanguageResx,
                                    System.Reflection.Assembly.GetExecutingAssembly());

        public static bool SyncJobParamsValidated(string name, string sourceAbsolute, string intermediate, IList<SyncJob> allEntries)
        {
            if (name == null || name.Equals(String.Empty))
                throw new SyncJobException(m_ResourceManager.GetString("err_jobNameCannotEmpty"));

            if (name.Length > 50)
                throw new SyncJobException(m_ResourceManager.GetString("err_jobNameTooLong"));

            return IsDirectoryValidated(intermediate)
                   && IsDirectoryValidated(sourceAbsolute)
                   && IsRecursive(sourceAbsolute, intermediate)
                   && IsDualRole(sourceAbsolute, intermediate, allEntries);
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
                throw new SyncJobException(m_ResourceManager.GetString("err_sourceSameAsIntermediate"));

            if (intermediate.LastIndexOf(@"\") > 0 && source.LastIndexOf(@"\") > 0)
            {
                if ((source.StartsWith(intermediate) || intermediate.StartsWith(source))
                    && System.IO.Path.GetDirectoryName(source) != System.IO.Path.GetDirectoryName(intermediate))
                {
                    throw new SyncJobException(m_ResourceManager.GetString("err_recursive"));
                }   
            }
            return true;
        }

        private static bool IsDualRole(string source, string intermediate, IList<SyncJob> allEntries)
        {
            if (allEntries != null)
            {
                foreach (SyncJob entry in allEntries)
                {
                    if (entry.IntermediaryStorage.Path.Equals(source) || entry.SyncSource.Path.Equals(intermediate))
                        return false;
                }
            }
            return true;
        }

        private static bool IsDirectoryValidated (string absolute)
        {
            if (absolute == null || absolute.Equals(String.Empty))
                throw new SyncJobException(m_ResourceManager.GetString("err_sourceIntermediateEmpty"));

            if (!Directory.Exists(absolute))
                throw new SyncJobException(String.Format(m_ResourceManager.GetString("err_directoryNotExist"), absolute));

            if (absolute.Length > 260)
                throw new SyncJobException(m_ResourceManager.GetString("err_directoryTooLong"));
            return true;
        }

        public static string validateSyncJobName(string newName)
        {
            string errorMsg = null;

            if (isNullOrEmpty(newName))
                errorMsg = m_ResourceManager.GetString("err_jobNameEmpty");
            else if (newName.Length > 50)
                errorMsg = m_ResourceManager.GetString("err_jobNameTooLong");

            return errorMsg;
        }

        // Does not check whether directory exist or not.
        public static string validateDirPath(string path)
        {

            if (isNullOrEmpty(path))
                return m_ResourceManager.GetString("err_directoryInvalid");

            DirectoryInfo di = null;

            try
            {
                di = new DirectoryInfo(path);
            }
            catch (SecurityException)
            {
                return m_ResourceManager.GetString("err_directoryNoPermission");
            }
            catch (PathTooLongException)
            {
                return m_ResourceManager.GetString("err_pathTooLong");
            }
            catch (ArgumentException)
            {
                return m_ResourceManager.GetString("err_pathIllegal");
            }

            // No errors
            return null;
        }
        

        public static string validateSyncDirs(string sync_source_dir, string intermediate_storage_dir)
        {
            if (validateDirPath(sync_source_dir) != null)
                return m_ResourceManager.GetString("err_syncSourceInvalid");

            if (validateDirPath(intermediate_storage_dir) != null)
                return m_ResourceManager.GetString("err_syncIntermediateInvalid");

            // Remove trailing backslash if any
            sync_source_dir = sync_source_dir.Trim(new char[] {'\\'});
            intermediate_storage_dir = intermediate_storage_dir.Trim(new char[] {'\\'});

            //Check #1: Check whether the Sync Source Folder and the Intermediate Storage Folder having the same directory.
            if (sync_source_dir.Equals(intermediate_storage_dir))
                return m_ResourceManager.GetString("err_syncFolderSameAsIntermediate");

            //Check #2a: Check whether the Sync Source Folder exists or not.
            if (!Directory.Exists(sync_source_dir))
                return m_ResourceManager.GetString("err_syncFolderNotFound");


            //Check #2b: Check whether the Intermediate Storage Folder exists or not.
            if (!Directory.Exists(intermediate_storage_dir))
                return m_ResourceManager.GetString("err_intermediateStorageNotFound");
            
            //Check #3: Check whether the Sync Source Folder and the Intermediate Storage Folder are the subdirectory of each other.
            if (intermediate_storage_dir.LastIndexOf(@"\") > 0 && sync_source_dir.LastIndexOf(@"\") > 0)
            {
                if (sync_source_dir.Equals(intermediate_storage_dir.Substring(0, intermediate_storage_dir.LastIndexOf(@"\")))
                || intermediate_storage_dir.Equals(sync_source_dir.Substring(0, sync_source_dir.LastIndexOf(@"\"))))
                    return m_ResourceManager.GetString("err_recursive2");
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
