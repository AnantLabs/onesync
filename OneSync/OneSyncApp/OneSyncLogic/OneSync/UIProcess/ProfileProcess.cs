using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;
using System.IO;

namespace OneSync.Synchronization
{
    public class ProfileProcess
    {      
        public void CreateProfile(string absolutePathToProfileFolder, string profileName, string absoluteSyncPath, string absoluteIntermediatePath )
        {
            SyncSource syncSource = new SyncSource(System.Guid.NewGuid().ToString(), absoluteSyncPath);
            IntermediaryStorage iSource = new IntermediaryStorage(absoluteIntermediatePath);

            Profile profile = new Profile(System.Guid.NewGuid().ToString(), profileName, syncSource, iSource);
            //create schemas if not exists
            CreateDataStore(absolutePathToProfileFolder, syncSource, iSource);

            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            if (profileManager.IsAlreadyCreated(profileName))
            {
                throw new ProfileNameExistException("Profile " + profileName + " is already created");
            }

            SQLiteSyncSourceProvider sourceProvider = new SQLiteSyncSourceProvider(profile.IntermediaryStorage.Path);
            if (sourceProvider.GetNumberOfSyncSources() >= 2)
            {
                throw new SyncSourcesNumberExceededException("Number of sync sources can't exceed 2");
            }
            string conString1 = string.Format("Version=3,uri=file:{0}",
                absolutePathToProfileFolder + Configuration.METADATA_RELATIVE_PATH);
            string conString2 = string.Format("Version=3,uri=file:{0}",
                profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);

            SqliteConnection con1 = null;
            SqliteConnection con2 = null;

            SqliteTransaction transaction1 = null;
            SqliteTransaction transaction2 = null;

            try
            {
                (con1 = new SqliteConnection(conString1)).Open();
                (con2 = new SqliteConnection(conString2)).Open();
                transaction1 = (SqliteTransaction)con1.BeginTransaction();
                transaction2 = (SqliteTransaction)con2.BeginTransaction();

                new SQLiteSyncSourceProvider().Insert(profile.SyncSource,con1);
                new SQLiteProfileManager().Insert(profile, con1);

                new SQLiteSyncSourceProvider().Insert(profile.SyncSource, con2);
                FileMetaData md = (FileMetaData)new SQLiteMetaDataProvider().FromPath(profile.SyncSource);
                new SQLiteMetaDataProvider(profile).Insert(md.MetaDataItems, con2);
                transaction2.Commit();
                transaction1.Commit();
            }
            catch (Exception ex)
            {
                if (transaction2 != null) transaction2.Rollback();
                if (transaction1 != null) transaction1.Rollback();
                throw new DatabaseException("Database Exception");
            }
            finally
            {
                if (con1 != null) con1.Dispose();
            }
        }

        public void UpdateProfile(string absolutePathToProfileFolder, Profile profile)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            profileManager.Update(profile);
        }

        public void DeleteProfile(string absolutePathToProfileFolder, string profileId)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            Profile profile = profileManager.Load(profileId);
            if (profileId != null) profileManager.Delete(profile);
        }

        public IList<Profile> GetProfiles(string absolutePathToProfileFolder)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            return profileManager.Load();
        }

        public Profile GetProfileById(string absolutePathToProfileFolder, string id)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            return profileManager.Load(id);
        }


        /// <summary>
        /// Create data store 
        /// </summary>
        /// <param name="userDataPath"></param>
        /// <param name="source"></param>
        /// <param name="mSource"></param>
        private void CreateDataStore(string pathToProfileFolder, SyncSource syncSource, IntermediaryStorage metaDataSource)
        {
            if (!Directory.Exists(pathToProfileFolder)) Directory.CreateDirectory(pathToProfileFolder);
            if (!Directory.Exists(metaDataSource.Path)) Directory.CreateDirectory(metaDataSource.Path);

            string pathToMdSource = metaDataSource.Path + Configuration.METADATA_RELATIVE_PATH;
            string pathTouserSource = pathToProfileFolder + Configuration.METADATA_RELATIVE_PATH;

            string conString1 = string.Format("Version=3,uri=file:{0}", pathTouserSource);
            string conString2 = string.Format("Version=3,uri=file:{0}", pathToMdSource);

            SqliteConnection con1 = null;
            SqliteConnection con2 = null;
            SqliteTransaction transaction1 = null;
            SqliteTransaction transaction2 = null;
            try
            {
                (con1 = new SqliteConnection(conString1)).Open();
                (con2 = new SqliteConnection(conString2)).Open();

                transaction2 = (SqliteTransaction)con2.BeginTransaction();
                transaction1 = (SqliteTransaction)con1.BeginTransaction();

                new SQLiteMetaDataProvider(@pathToMdSource).CreateSchema(con2);
                new SQLiteActionProvider(@pathToMdSource).CreateSchema(con2);
                new SQLiteSyncSourceProvider(@pathToMdSource).CreateSchema(con2);

                new SQLiteSyncSourceProvider(@pathTouserSource).CreateSchema(con1);
                new SQLiteProfileManager(@pathTouserSource).CreateSchema(con1);

                transaction2.Commit();
                transaction1.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (transaction2 != null) transaction2.Rollback();
                if (transaction1 != null) transaction1.Rollback();
                throw new DatabaseException("Database exception");
            }
            finally
            {
                if (con1 != null) con1.Dispose();
                if (con2 != null) con2.Dispose();
            }
        }
    }
}
