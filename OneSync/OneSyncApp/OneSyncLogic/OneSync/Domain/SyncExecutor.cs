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
        public static void CopyToDirtyFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            // TODO: make it atomic??
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;
            string absolutePathInImediateStorage = profile.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;

            SQLiteAccess db = new SQLiteAccess(Path.Combine(profile.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = db.NewSQLiteConnection();
            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
                actProvider.Add(action, con);
                Files.FileUtils.Copy(absolutePathInSyncSource, absolutePathInImediateStorage);
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

        public static void CopyToSyncFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            // TODO: atomic....
            string absolutePathInIntermediateStorage = profile.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess dbAccess = new SQLiteAccess(Path.Combine(profile.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = dbAccess.NewSQLiteConnection();
            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
                actProvider.Delete(action, con);
                Files.FileUtils.Copy(absolutePathInIntermediateStorage, absolutePathInSyncSource);
                Files.FileUtils.DeleteFileAndFolderIfEmpty(profile.IntermediaryStorage.DirtyFolderPath, absolutePathInIntermediateStorage);
                trasaction.Commit();
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
               

        public static void DuplicateRenameToSyncFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            string absolutePathInIntermediateStorage = profile.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(profile.IntermediaryStorage.Path);
            SqliteConnection con = access.NewSQLiteConnection();
            SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
                actProvider.Delete(action, con);

                Files.FileUtils.DuplicateRename(absolutePathInIntermediateStorage, absolutePathInSyncSource);
                Files.FileUtils.DeleteFileAndFolderIfEmpty(profile.IntermediaryStorage.DirtyFolderPath, absolutePathInIntermediateStorage);

                trasaction.Commit();
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

        public static void DeleteInSyncFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(Path.Combine(profile.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con = access.NewSQLiteConnection();
            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();

            try
            {
                SQLiteSyncActionsProvider actProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
                actProvider.Delete(action);
                Files.FileUtils.DeleteFileAndFolderIfEmpty(profile.SyncSource.Path, absolutePathInSyncSource);
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

        public static void RenameInSyncFolderAndUpdateActionTable(RenameAction action, Profile profile)
        {
            string oldAbsolutePathInSyncSource = profile.SyncSource.Path + action.PreviousRelativeFilePath;
            string newAbsolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;

            SQLiteAccess access = new SQLiteAccess(profile.IntermediaryStorage.Path);
            SqliteConnection con = access.NewSQLiteConnection();
            SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();

            try
            {
                SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
                actProvider.Delete(action);

                Files.FileUtils.Copy(oldAbsolutePathInSyncSource, newAbsolutePathInSyncSource);
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
