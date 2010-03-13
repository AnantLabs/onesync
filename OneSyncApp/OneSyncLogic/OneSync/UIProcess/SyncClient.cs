/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;
using System.IO;

namespace OneSync.Synchronization
{
    /// <summary>
    /// TODO...
    /// </summary>
    public static class SyncClient
    {

        /// <summary>
        /// Gets a new ProfileManager instance.
        /// </summary>
        /// <param name="storagePath">Location where all profiles are saved to or loaded from.</param>
        /// <returns></returns>
        public static ProfileManager GetProfileManager(string storagePath)
        {
            // Returns any objects derived from ProfileManager abstract class.
            return new SQLiteProfileManager(storagePath);
        }


        /// <summary>
        /// Gets a new MetaDataProvider instance.
        /// </summary>
        /// <param name="storagePath">Location where all metadata are saved to or loaded from.</param>
        /// <param name="rootPath">To be combined with relative paths to give absolute paths of files.</param>
        /// <returns></returns>
        public static MetaDataProvider GetMetaDataProvider(string storagePath, string rootPath)
        {
            return new SQLiteMetaDataProvider(storagePath, rootPath);
        }


        /// <summary>
        /// Gets a new SyncSourceProvider instance.
        /// </summary>
        /// <param name="storagePath">Location where all SyncSource information are saved to or loaded from.</param>
        /// <returns></returns>
        public static SyncSourceProvider GetSyncSourceProvider(string storagePath)
        {
            return new SQLiteSyncSourceProvider(storagePath);
        }


        /// <summary>
        /// Gets a new SyncActionsProvider instance.
        /// </summary>
        /// <param name="storagePath">Location where all SyncActions are saved to or loaded from.</param>
        /// <returns></returns>
        public static SQLiteSyncActionsProvider GetSyncActionsProvider(string storagePath)
        {
            return new SQLiteSyncActionsProvider(storagePath);
        }


        public static void Initialize(string absolutePathToProfileFolder, string profileName, string absoluteSyncPath, string absoluteIntermediatePath)
        {
            // TODO: to make both atomic, can surround following stmts in try catch
            // if exception thrown, delete all created files.

            // Note: Order of actions matters, creating SyncSource will creates DataSourceInfo Table
            // which will be used to check if more than 2 syncsource exist. However, the table does
            // not exist, an exeption will be thrown and caught and it is assumed to be 'first-run'.

            // Creates and save SyncSource information in profile folder as well and on intermediary storage.
            // In this implementaion, an empty table will be created in both location during first-run.
            SyncSourceProvider syncSrc;
            syncSrc = GetSyncSourceProvider(absolutePathToProfileFolder);
            syncSrc = GetSyncSourceProvider(absoluteIntermediatePath);

            // Intermediary storage shouldn't be paired with more than 2 SyncSource.
            if (syncSrc.GetSyncSourceCount() >= 2)
            {
                throw new SyncSourcesNumberExceededException("Number of sync sources can't exceed 2");
            }

            // Creates and save profile
            ProfileManager pfManager = SyncClient.GetProfileManager(absolutePathToProfileFolder);
            Profile profile = new Profile(profileName, absoluteSyncPath, absoluteIntermediatePath);
            pfManager.Add(profile);

            // Creates and save metadata
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(absoluteIntermediatePath, absoluteSyncPath);
            FileMetaData md = MetaDataProvider.Generate(profile.SyncSource.Path, profile.SyncSource.ID);
            mdProvider.Add(md);

            // TODO: new SQLiteActionProvider(@pathToMdSource).CreateSchema(con2);



            /*
            //create schemas if not exists
            CreateDataStore(absolutePathToProfileFolder, profile.SyncSource, profile.IntermediaryStorage);


            
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

                new SQLiteSyncSourceProvider().Insert(profile.SyncSource, con1);
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
            }*/

        }

        /*
        /// <summary>
        /// Create data store 
        /// </summary>
        /// <param name="userDataPath"></param>
        /// <param name="source"></param>
        /// <param name="mSource"></param>
        private static void CreateDataStore(string pathToProfileFolder, SyncSource syncSource, IntermediaryStorage iStorage)
        {
            if (!Directory.Exists(pathToProfileFolder)) Directory.CreateDirectory(pathToProfileFolder);
            if (!Directory.Exists(iStorage.Path)) Directory.CreateDirectory(iStorage.Path);

            string pathToMdSource = iStorage.Path + Configuration.METADATA_RELATIVE_PATH;
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
         */
    }
}
