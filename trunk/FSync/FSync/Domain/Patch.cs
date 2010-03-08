using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;


namespace OneSync.Synchronization
{

    public class Patch
    {
       
        private IList<DirtyItem> dirtyItems = new List<DirtyItem>();
        private IList<SyncAction> actions = null;
        private Profile profile = null;

        /// <summary>
        /// Creates a new Patch.
        /// </summary>
        /// <param name="syncSource">SyncSource of other PC where patch is to be applied.</param>
        /// <param name="iStorage">Information about intermediary storage</param>
        /// <param name="actions">Actions to be contained in this patch.</param>
        public Patch(Profile profile)
        {
            this.profile = profile;
            actions = new SQLiteActionProvider(profile).Load(profile.SyncSource, SourceOption.NOT_EQUAL_SOURCE_ID);
        }

        /// <summary>
        /// Verify whether all the files contained in this patch are valid by
        /// checking whether it exists and optionally its file hash is the same.
        /// </summary>
        /// <param name="checkHash">Computes hash of files and verify it.</param>
        /// <returns>True if all the files are valid.</returns>
        public bool Verify(bool checkHash)
        {
            // Root directory where all dirty files are stored
            string rootDir = profile.IntermediaryStorage.DirtyFolderPath;


            foreach (SyncAction a in actions)
            {
                // Skip Delete Action as there is no need to check for
                // if file exists, neither is there a need to compute hash.
                if (a is DeleteAction) continue;

                string filePath = Path.Combine(rootDir, a.RelativeFilePath);

                bool result = File.Exists(filePath);

                // Check whether file hash tally
                if (checkHash)
                    result &= Files.FileUtils.GetFileHash(filePath).Equals(a.FileHash);

                if (result == false) return false;
            }
            return true;
        }

        public void Apply()
        {
            foreach (SyncAction action in actions)
            {

                if (action.ChangeType == ChangeType.NEWLY_CREATED)
                {
                    CopyToSyncFolderAndUpdateActionTable(action,profile);
                }
                else if (action.ChangeType == ChangeType.DELETED)
                {
                    DeleteInSourceFolderAndUpdateActionTable(action, profile);
                }
                else if (action.ChangeType == ChangeType.RENAMED)
                {
                    RenameInSourceFolderAndUpdateActionTable((RenameAction)action, profile);
                }
            }

            // TODO:
            // Update metadata after patch is applied
            // Delete all dirty files
        }

        public void Generate()
        {

            FileMetaData currentItems = (FileMetaData)new SQLiteMetaDataProvider(profile).FromPath(profile.SyncSource);
            //read metadata of the current folder stored in the database
            FileMetaData storedItems = new SQLiteMetaDataProvider(profile).Load(profile.SyncSource, SourceOption.EQUAL_SOURCE_ID);
            MetaDataProcess.UpdateMetadata (profile, storedItems, currentItems);


            storedItems = (FileMetaData)new SQLiteMetaDataProvider(profile).Load(profile.SyncSource, SourceOption.NOT_EQUAL_SOURCE_ID);
            FileSyncProvider syncProvider = new FileSyncProvider(currentItems, storedItems, profile.IntermediaryStorage);

            //generate list of sync actions by comparing 2 metadata

            IList<SyncAction> newActions = syncProvider.GenerateActions();

            //delete actions of previous sync
            ActionProcess.DeleteBySourceId(profile, SourceOption.EQUAL_SOURCE_ID);
            if (Directory.Exists(profile.IntermediaryStorage.DirtyFolderPath)) Directory.Delete(profile.IntermediaryStorage.DirtyFolderPath, true);
            foreach (SyncAction action in newActions)
            {
                if (action.ChangeType == ChangeType.NEWLY_CREATED)
                {
                    CopyToDirtyFolderAndUpdateActionTable(action, profile);
                }
                else if (action.ChangeType == ChangeType.DELETED || action.ChangeType == ChangeType.RENAMED)
                {
                    ActionProcess.InsertAction(action, profile);
                }
            }
            
        }

        private void Process(IList<SyncAction> actions)
        {
            DirtyItem dirtyItem = null;
            foreach (SyncAction action in actions)
            {
                switch (action.ChangeType)
                {
                    /*
                    case ChangeType.MODIFIED:
                        ModifyAction modifiedAction = (ModifyAction)action;
                        dirtyItem = new DirtyItem(syncSource + modifiedAction.RelativeFilePath,
                           metaDataSource.Path + modifiedAction.RelativeFilePath, modifiedAction.NewItemHash);
                        dirtyItems.Add(dirtyItem);
                        break;
                     */
                    case ChangeType.NEWLY_CREATED:
                        CreateAction createAction = (CreateAction)action;
                        dirtyItem = new DirtyItem(profile.SyncSource.Path + createAction.RelativeFilePath, profile.IntermediaryStorage.Path + createAction.RelativeFilePath, createAction.FileHash);
                        break;
                }
            }
        }
       

       
        #region Carryout actions 
        public void CopyToDirtyFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            string connectionString = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;
            string absolutePathInImediateStorage = profile.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            using (SqliteConnection con = new SqliteConnection(connectionString))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Insert(action, con);
                    Files.FileUtils.Copy(absolutePathInSyncSource, absolutePathInImediateStorage);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException();
                }
            }
        }

        public void CopyToSyncFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            string absolutePathInIntermediateStorage = profile.IntermediaryStorage.DirtyFolderPath + action.RelativeFilePath;
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Delete(action, con);
                    Files.FileUtils.Copy(absolutePathInIntermediateStorage, absolutePathInSyncSource);
                    File.Delete(absolutePathInIntermediateStorage);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException();
                }
            }
        }

        public void DeleteInSourceFolderAndUpdateActionTable(SyncAction action, Profile profile)
        {
            string absolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Delete(action, con);
                    File.Delete(absolutePathInSyncSource);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException();
                }
            }
        }

        public void RenameInSourceFolderAndUpdateActionTable(RenameAction action, Profile profile)
        {
            string oldAbsolutePathInSyncSource = profile.SyncSource.Path + action.PreviousRelativeFilePath;
            string newAbsolutePathInSyncSource = profile.SyncSource.Path + action.RelativeFilePath;
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Delete(action, con);
                    Files.FileUtils.Copy(oldAbsolutePathInSyncSource, newAbsolutePathInSyncSource);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException();
                }
            }
        }
        #endregion Carryout actions
    }    
}
