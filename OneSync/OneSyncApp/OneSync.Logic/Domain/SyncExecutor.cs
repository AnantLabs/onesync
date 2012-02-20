//Coded by Nguyen van Thuat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OneSync.Synchronization;
using System.IO;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;
using OneSync.Files;
using System.Resources;


namespace OneSync.Synchronization
{
    public class SyncExecutor
    {
        public static ResourceManager m_ResourceManager = new ResourceManager(Properties.Settings.Default.LanguageResx,
                                    System.Reflection.Assembly.GetExecutingAssembly());

        public static void CopyToDirtyFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            if (!Directory.Exists(job.SyncSource.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.SyncSource.Path));

            if ( !Directory.Exists(job.IntermediaryStorage.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.IntermediaryStorage.Path));

            if (!Directory.Exists(job.IntermediaryStorage.DirtyFolderPath))
                Directory.CreateDirectory(job.IntermediaryStorage.DirtyFolderPath);

            // TODO: make it atomic??
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;
            string absolutePathInImediateStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;

            var db = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME),true);
            var con = db.NewSQLiteConnection();

            if (con == null)
                throw new DatabaseException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME)));

            var transaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                var actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Add(action, con);

                if (!FileUtils.Copy(absolutePathInSyncSource, absolutePathInImediateStorage, true))
                    throw new Exception(String.Format(m_ResourceManager.GetString("err_cannotCopyFile"), absolutePathInSyncSource));
                
                transaction.Commit();
            }
            catch (OutOfDiskSpaceException)
            {
                transaction.Rollback();
                throw; 
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }

        public static void UpdateTableAction(SyncAction action, SyncJob job)
        {
            var actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
            actProvider.Add(action);
        }


        /// <summary>
        /// Copy a file from sync folder and update action table
        /// </summary>
        /// <param name="action"></param>
        /// <param name="profile"></param>
        public static void CopyToSyncFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            if (!Directory.Exists(job.SyncSource.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.SyncSource.Path));

            if (!Directory.Exists(job.IntermediaryStorage.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.IntermediaryStorage.Path));
            
            // TODO: atomic....
            string absolutePathInIntermediateStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess dbAccess = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME),true);
            SqliteConnection con = dbAccess.NewSQLiteConnection();

            if (con == null)
                throw new DatabaseException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME)));

            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action, con);
                if (!Files.FileUtils.Copy(absolutePathInIntermediateStorage, absolutePathInSyncSource, true))
                    throw new Exception(String.Format(m_ResourceManager.GetString("err_cannotCopyFile"), absolutePathInIntermediateStorage));

                trasaction.Commit();
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.IntermediaryStorage.DirtyFolderPath, absolutePathInIntermediateStorage, true);
            }
            catch (OutOfDiskSpaceException)
            {
                trasaction.Rollback();
                throw;
            }
            catch (Exception)
            {
                trasaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }


        public static void DuplicateRenameInSyncFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            if (!Directory.Exists(job.SyncSource.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.SyncSource.Path));

            if (!Directory.Exists(job.IntermediaryStorage.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.IntermediaryStorage.Path));

            string absolutePathInIntermediateStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME),true);
            SqliteConnection con = access.NewSQLiteConnection();

            if (con == null)
                throw new DatabaseException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME)));

            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action, con);

                if (!Files.FileUtils.DuplicateRename(absolutePathInSyncSource, absolutePathInSyncSource)
                    || !Files.FileUtils.Copy(absolutePathInIntermediateStorage, absolutePathInSyncSource, true))
                    throw new Exception(String.Format(m_ResourceManager.GetString("err_cannotCopyFile"), absolutePathInIntermediateStorage));
                trasaction.Commit();
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.IntermediaryStorage.DirtyFolderPath, absolutePathInIntermediateStorage, true);
            }
            catch (OutOfDiskSpaceException)
            {
                trasaction.Rollback();
                throw;
            }
            catch (Exception)
            {
                trasaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }

        /// <summary>
        /// Renames conflicted file so that RenameAction can be executed
        /// </summary>
        /// <param name="renameAction">Rename action that cannot be executed due to conflict.</param>
        public static void ConflictRenameAndUpdateActionTable(RenameAction action, SyncJob job, bool keepConflictedFile)
        {
            if (!Directory.Exists(job.SyncSource.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.SyncSource.Path));

            if (!Directory.Exists(job.IntermediaryStorage.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.IntermediaryStorage.Path));

            string absPathInIStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absPathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;
            string absOldPathInSyncSource = job.SyncSource.Path + action.PreviousRelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME), true);
            SqliteConnection con = access.NewSQLiteConnection();

            if (con == null)
                throw new DatabaseException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME)));

            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action, con);

                if (!keepConflictedFile)
                    Files.FileUtils.Delete(absPathInSyncSource, true);
                else
                    Files.FileUtils.DuplicateRename(absPathInSyncSource, absPathInSyncSource);

                if (!Files.FileUtils.Move(absOldPathInSyncSource, absPathInSyncSource, true))
                    throw new Exception(String.Format(m_ResourceManager.GetString("err_cannotRenameFile"), absPathInIStorage));

                trasaction.Commit();
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.IntermediaryStorage.DirtyFolderPath, absPathInIStorage, true);
            }
            catch (Exception)
            {
                trasaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }

        /// <summary>
        /// Delete a file in sync source folder and update action table
        /// </summary>
        /// <param name="action"></param>
        /// <param name="profile"></param>
        public static void DeleteInSyncFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME),true);
            SqliteConnection con = access.NewSQLiteConnection();

            if (con == null)
                throw new DatabaseException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME)));

            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();

            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action);
                if (!Files.FileUtils.Delete(absolutePathInSyncSource, true))
                    throw new Exception(String.Format(m_ResourceManager.GetString("err_cannotDeleteFile"), absolutePathInSyncSource));
                transaction.Commit();
                /*
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.IntermediaryStorage.DirtyFolderPath, 
                    job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath
                    , true);
                 */
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }

        public static void DeleteFromActionTable(SyncAction action, SyncJob job)
        {
            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
            actProvider.Delete(action);
        }

        public static bool CreateFolder(string baseFolder, string relativePath)
        {
            string absolutePath = baseFolder + relativePath;
            DirectoryInfo dirInfo = new DirectoryInfo(absolutePath);
            if (!dirInfo.Exists)
            {
                try { dirInfo.Create(); }
                catch (Exception) { return false; }
            }
            return true;
        }

        public static bool DeleteFolder(string baseFolder, string relativePath, bool forceToDelete)
        {

            string absolutePath = baseFolder + relativePath;
            try
            {
                //Files.FileUtils.DeleteEmptyFolderRecursively(baseFolder, new DirectoryInfo(absolutePath), forceToDelete);
                Files.FileUtils.DeleteFolder(absolutePath, true);
                DirectoryInfo info = new DirectoryInfo(absolutePath);                
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <summary>
        /// Rename a file in sync source folder and update action table
        /// </summary>
        /// <param name="action"></param>
        /// <param name="profile"></param>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static void RenameInSyncFolderAndUpdateActionTable(RenameAction action, SyncJob job)
        {
            if (!Directory.Exists(job.SyncSource.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.SyncSource.Path));

            if (!Directory.Exists(job.IntermediaryStorage.Path))
                throw new SyncSourceException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), job.IntermediaryStorage.Path));
            
            string oldAbsolutePathInSyncSource = job.SyncSource.Path + action.PreviousRelativeFilePath;
            string newAbsolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(Path.Combine (job.IntermediaryStorage.Path, Configuration.DATABASE_NAME),true);
            SqliteConnection con = access.NewSQLiteConnection();

            if (con == null)
                throw new DatabaseException(String.Format(m_ResourceManager.GetString("err_somethingNotFound"), Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME)));

            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();

            try
            {
                SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action);

                if (File.Exists(oldAbsolutePathInSyncSource) &&
                    !Files.FileUtils.Move(oldAbsolutePathInSyncSource, newAbsolutePathInSyncSource, true))
                    throw new Exception(String.Format(m_ResourceManager.GetString("err_cannotRenameFile"), oldAbsolutePathInSyncSource));
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }

        

    }
}
