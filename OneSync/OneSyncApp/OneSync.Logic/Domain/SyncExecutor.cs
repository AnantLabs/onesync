using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneSync.Synchronization;
using System.IO;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;


namespace OneSync.Synchronization
{
    public class SyncExecutor
    {
        #region Carryout actions
        public static void CopyToDirtyFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            // TODO: make it atomic??
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;
            string absolutePathInImediateStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;

            SQLiteAccess db = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = db.NewSQLiteConnection();
            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Add(action, con);
                if (!Files.FileUtils.Copy(absolutePathInSyncSource, absolutePathInImediateStorage, true)) throw new Exception("Can't copy file " + absolutePathInSyncSource);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (con != null) con.Dispose();
            }
        }

        /// <summary>
        /// Copy a file from sync folder and update action table
        /// </summary>
        /// <param name="action"></param>
        /// <param name="profile"></param>
        public static void CopyToSyncFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            // TODO: atomic....
            string absolutePathInIntermediateStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess dbAccess = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = dbAccess.NewSQLiteConnection();
            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action, con);
                if (!Files.FileUtils.Copy(absolutePathInIntermediateStorage, absolutePathInSyncSource, true)) throw new Exception("Can't copy file " + absolutePathInIntermediateStorage);
                trasaction.Commit();
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.IntermediaryStorage.DirtyFolderPath, absolutePathInIntermediateStorage, true);
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


        public static void DuplicateRenameToSyncFolderAndUpdateActionTable(SyncAction action, SyncJob job)
        {
            string absolutePathInIntermediateStorage = job.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = access.NewSQLiteConnection();
            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action, con);

                if (!Files.FileUtils.DuplicateRename(absolutePathInSyncSource, absolutePathInSyncSource)
                    || !Files.FileUtils.Copy(absolutePathInIntermediateStorage, absolutePathInSyncSource, true))
                    throw new Exception("Can't copy file " + absolutePathInIntermediateStorage);
                trasaction.Commit();
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.IntermediaryStorage.DirtyFolderPath, absolutePathInIntermediateStorage, true);
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

            SQLiteAccess access = new SQLiteAccess(Path.Combine(job.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = access.NewSQLiteConnection();
            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();

            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action);
                if (!Files.FileUtils.Delete(absolutePathInSyncSource, true))
                    throw new Exception("Can't delete file " + absolutePathInSyncSource);
                transaction.Commit();
                Files.FileUtils.DeleteFileAndFolderIfEmpty(job.SyncSource.Path, absolutePathInSyncSource, true);
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
                Files.FileUtils.DeleteEmptyFolderRecursively(baseFolder, new DirectoryInfo(absolutePath), forceToDelete);

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
            string oldAbsolutePathInSyncSource = job.SyncSource.Path + action.PreviousRelativeFilePath;
            string newAbsolutePathInSyncSource = job.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(job.IntermediaryStorage.Path);
            SqliteConnection con = access.NewSQLiteConnection();
            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();

            try
            {
                SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
                actProvider.Delete(action);

                if (!Files.FileUtils.Copy(oldAbsolutePathInSyncSource, newAbsolutePathInSyncSource, true))
                    throw new Exception("Can't copy file " + oldAbsolutePathInSyncSource);
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
        #endregion Carryout actions
    }
}
