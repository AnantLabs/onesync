/*
 $Id: SQLiteProfileManager.cs 255 2010-03-17 16:08:53Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.IO;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    /// <summary>
    /// This class manages jobs related to profiles
    /// </summary>
    public class SQLiteProfileManager: ProfileManager
    {
        // TODO: Wrap Update/Add etc with try catch and return false if exception thrown?

        // TODO: Find a better place to define these constant?
        // Database constants
        public const string DATABASE_NAME = "data.md";
        public const string PROFILE_TABLE = "PROFILE";
        public const string COL_PROFILE_ID = "PROFILE_ID";
        public const string COL_PROFILE_NAME = "PROFILE_NAME";
        public const string COL_METADATA_SOURCE_LOCATION = "METADATA_SOURCE_LOCATION";
        public const string COL_SYNC_SOURCE_ID = "SYNC_SOURCE_ID";
        public const string COL_SOURCE_ABS_PATH = "SOURCE_ABSOLUTE_PATH";
        public const string COL_SOURCE_ID = "SOURCE_ID";
        public const string DATASOURCE_INFO_TABLE = "DATASOURCE_INFO_TABLE";

        /// <summary>
        /// Creates an instance of SQLiteProfileManager. Database schema will be created if database file is newly created.
        /// </summary>
        /// <param name="storagePath">Location where all database of profiles are saved to or loaded from.</param>
        public SQLiteProfileManager(string storagePath)
            : base(storagePath)
        {
            // Create database schema if necessary

            FileInfo fi = new FileInfo(Path.Combine(this.StoragePath, DATABASE_NAME));

            if (!fi.Exists)
            {
                // If the parent directory already exists, Create() does nothing.
                fi.Directory.Create();
            }           
        }

        /// <summary>
        /// Create schema for profile table
        /// </summary>
        public override void CreateSchema()
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection())
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + PROFILE_TABLE +
                                "(" + COL_PROFILE_ID + " VARCHAR(50) PRIMARY KEY, "
                                + COL_PROFILE_NAME + " VARCHAR(50) UNIQUE NOT NULL, "
                                + COL_METADATA_SOURCE_LOCATION + " TEXT, "
                                + COL_SYNC_SOURCE_ID + " VARCHAR (50), "
                                + "FOREIGN KEY (" + COL_SYNC_SOURCE_ID + ") REFERENCES "
                                + DATASOURCE_INFO_TABLE + "(" + COL_SOURCE_ID + "))";

                db.ExecuteNonQuery(cmdText, false);
            }
        }

        public void CreateSchema(SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + PROFILE_TABLE +
                                "(" + COL_PROFILE_ID + " VARCHAR(50) PRIMARY KEY, "
                                + COL_PROFILE_NAME + " VARCHAR(50) UNIQUE NOT NULL, "
                                + COL_METADATA_SOURCE_LOCATION + " TEXT, "
                                + COL_SYNC_SOURCE_ID + " VARCHAR (50), "
                                + "FOREIGN KEY (" + COL_SYNC_SOURCE_ID + ") REFERENCES "
                                + DATASOURCE_INFO_TABLE + "(" + COL_SOURCE_ID + "))";
                cmd.ExecuteNonQuery();
            }
        }

        public override IList<Profile> LoadAllProfiles()
        {
            IList<Profile> profiles = new List<Profile>();

            // Note: The SQL depends on other tables as well which might not be created.
            // So empty profile list should be returned if there is an exception

            try
            {
                SQLiteAccess db = new SQLiteAccess(Path.Combine (this.StoragePath, Configuration.DATABASE_NAME ));

                using (SqliteConnection con =  db.NewSQLiteConnection ())
                {                                   
                    using (SqliteCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM " + PROFILE_TABLE +
                                     " p, " + DATASOURCE_INFO_TABLE +
                                     " d WHERE p" + "." + COL_SYNC_SOURCE_ID + " = d" + "." + COL_SOURCE_ID;
                        using (SqliteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // TODO: constructor of Profile takes in more arguments to remove dependency on IntermediaryStorage and SyncSource class.
                                SyncSource source = new SyncSource((string)reader[COL_SYNC_SOURCE_ID], (string)reader[COL_SOURCE_ABS_PATH]);
                                IntermediaryStorage mdSource = new IntermediaryStorage((string)reader[COL_METADATA_SOURCE_LOCATION]);
                                Profile p = new Profile((string)reader[COL_PROFILE_ID],
                                    (string)reader[COL_PROFILE_NAME], source, mdSource);
                                profiles.Add(p);
                            }
                        }
                    }                                       
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Log error?
            }
            
            return profiles;
        }

        public override Profile Load(string profileId)
        {
            Profile p = null;

            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                string cmdText = "SELECT * FROM " + PROFILE_TABLE +
                                 " p, " + DATASOURCE_INFO_TABLE +
                                 " d WHERE p" + "." + COL_SYNC_SOURCE_ID + " = d" + "." + COL_SOURCE_ID +
                                 " AND " + COL_PROFILE_ID + " = @pid";


                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@pid", System.Data.DbType.String) { Value = profileId });

                db.ExecuteReader(cmdText, paramList, reader => 
                    {
                        // TODO: constructor of Profile takes in more arguments to remove dependency on IntermediaryStorage and SyncSource class.
                        SyncSource source = new SyncSource((string)reader[COL_SYNC_SOURCE_ID], (string)reader[COL_SOURCE_ABS_PATH]);
                        IntermediaryStorage mdSource = new IntermediaryStorage((string)reader[COL_METADATA_SOURCE_LOCATION]);
                        
                        p = new Profile((string)reader[COL_PROFILE_ID], (string)reader[COL_PROFILE_NAME], source, mdSource);
                        
                        return;
                    }
                );

            }
            return p;
        }

        public Profile GetProfileByName (string name)
        {
            Profile p = null;

            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection())
            {
                string cmdText = "SELECT * FROM " + PROFILE_TABLE +
                                 " p, " + DATASOURCE_INFO_TABLE +
                                 " d WHERE p" + "." + COL_SYNC_SOURCE_ID + " = d" + "." + COL_SOURCE_ID +
                                 " AND " + COL_PROFILE_NAME + " = @pname";


                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@pname", System.Data.DbType.String) { Value = name });

                db.ExecuteReader(cmdText, paramList, reader =>
                {
                    // TODO: constructor of Profile takes in more arguments to remove dependency on IntermediaryStorage and SyncSource class.
                    SyncSource source = new SyncSource((string)reader[COL_SYNC_SOURCE_ID], (string)reader[COL_SOURCE_ABS_PATH]);
                    IntermediaryStorage mdSource = new IntermediaryStorage((string)reader[COL_METADATA_SOURCE_LOCATION]);

                    p = new Profile((string)reader[COL_PROFILE_ID], (string)reader[COL_PROFILE_NAME], source, mdSource);

                    return;
                }
                );

            }
            return p;
        }


        public override bool CreateProfile(string profileName, string absoluteSyncPath, string absoluteIntermediatePath)
        {            
            SyncSource syncSource = new SyncSource(System.Guid.NewGuid().ToString(), absoluteSyncPath);
            IntermediaryStorage iStorage = new IntermediaryStorage(absoluteIntermediatePath);
            Profile profile = new Profile(System.Guid.NewGuid().ToString(),
                profileName, syncSource, iStorage);
            CreateDataStore(this.StoragePath, syncSource, iStorage); 
            return Add(profile);
        }

        public static void CreateDataStore(string pathToProfileFolder, SyncSource syncSource, IntermediaryStorage metaDataSource)
        {
            if (!Directory.Exists(pathToProfileFolder)) Directory.CreateDirectory(pathToProfileFolder);
            if (!Directory.Exists(metaDataSource.Path)) Directory.CreateDirectory(metaDataSource.Path);

            SqliteConnection con1 = null;
            SqliteConnection con2 = null;
            SqliteTransaction transaction1 = null;
            SqliteTransaction transaction2 = null;
            try
            {
                SQLiteAccess dbAccess1 = new SQLiteAccess(Path.Combine(pathToProfileFolder, Configuration.DATABASE_NAME));
                SQLiteAccess dbAccess2 = new SQLiteAccess(Path.Combine (metaDataSource.Path, Configuration.DATABASE_NAME));
                con1 = dbAccess1.NewSQLiteConnection();
                con2 = dbAccess2.NewSQLiteConnection();

                transaction2 = (SqliteTransaction)con2.BeginTransaction();
                transaction1 = (SqliteTransaction)con1.BeginTransaction();

                //Create schema for source info table in profile folder
                SQLiteSyncSourceProvider sourceProvider = (SQLiteSyncSourceProvider)SyncClient.GetSyncSourceProvider(pathToProfileFolder);
                sourceProvider.CreateSchema(con1);

                //Create schema for profile table in profile folder
                SQLiteProfileManager pManager = (SQLiteProfileManager)SyncClient.GetProfileManager(pathToProfileFolder);
                pManager.CreateSchema(con1);

                //create schema for source info table in intermediate storage folder
                sourceProvider = (SQLiteSyncSourceProvider)SyncClient.GetSyncSourceProvider(metaDataSource.Path);
                sourceProvider.CreateSchema(con2);

                //create schema for metadata table in intermediate storage folder
                SQLiteMetaDataProvider mdProvider = (SQLiteMetaDataProvider)SyncClient.GetMetaDataProvider(metaDataSource.Path, Configuration.DATABASE_NAME);
                mdProvider.CreateSchema(con2);

                //create schema for action table in intermediate storage folder
                SQLiteSyncActionsProvider actionProvider = (SQLiteSyncActionsProvider)SyncClient.GetSyncActionsProvider(metaDataSource.Path);
                actionProvider.CreateSchema(con2);

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

        public override bool Update(Profile profile)
        {
            // Update a profile requires update 2 tables at the same time, 
            // If one update on a table fails, the total update action must fail too.
            string updateProfileText  = "UPDATE " + PROFILE_TABLE +
                                        " SET " + COL_METADATA_SOURCE_LOCATION + " = @mdSource, " +
                                        COL_PROFILE_NAME + " = @name WHERE " + COL_PROFILE_ID + " = @id;";

            SqliteParameterCollection paramList = new SqliteParameterCollection();                           
            // Add parameters for 1st Update statement
            paramList.Add(new SqliteParameter("@mdSource", System.Data.DbType.String) { Value = profile.IntermediaryStorage.Path });
            paramList.Add(new SqliteParameter("@name", System.Data.DbType.String) { Value = profile.Name });
            paramList.Add(new SqliteParameter("@id", System.Data.DbType.String) { Value = profile.ID });

            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteSyncSourceProvider(this.StoragePath).Update(profile.SyncSource,con );
                    db.ExecuteNonQuery(updateProfileText, paramList);
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }          
        }

        public override bool Delete(Profile profile)
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                SqliteParameterCollection paramList = new SqliteParameterCollection();

                string cmdText = "DELETE FROM " + SyncSource.DATASOURCE_INFO_TABLE +
                                 " WHERE " + SyncSource.SOURCE_ID + " = @sid;";
                
                paramList.Add(new SqliteParameter("@sid", System.Data.DbType.String) { Value = profile.SyncSource.ID });

                cmdText += "DELETE FROM " + PROFILE_TABLE +
                           " WHERE " + COL_PROFILE_ID + " = @pid";

                paramList.Add(new SqliteParameter("@pid", System.Data.DbType.String) { Value = profile.ID });

                db.ExecuteNonQuery(cmdText, paramList);
            }

            return true;
        }

        public override bool Add(Profile profile)
        {
            if (this.ProfileExists(profile.Name))
                throw new ProfileNameExistException("Profile " + profile.Name + " is already created");

            
            SQLiteAccess dbAccess1 = new SQLiteAccess(Path.Combine (this.StoragePath, Configuration.DATABASE_NAME));
            SQLiteAccess dbAccess2 = new SQLiteAccess(Path.Combine(profile.IntermediaryStorage.Path, Configuration.DATABASE_NAME));
            SqliteConnection con1 = dbAccess1.NewSQLiteConnection();
            SqliteConnection con2 = dbAccess2.NewSQLiteConnection();
            SqliteTransaction transaction1 = (SqliteTransaction)con1.BeginTransaction();
            SqliteTransaction transaction2 = (SqliteTransaction)con2.BeginTransaction();

            try
            {
                string insertProfileText = "INSERT INTO " + PROFILE_TABLE +
                                 " (" + COL_PROFILE_ID + ", " + COL_PROFILE_NAME +
                                 " ," + COL_METADATA_SOURCE_LOCATION + ", " + COL_SYNC_SOURCE_ID +
                                 ") VALUES (@id, @name, @meta, @source)";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", System.Data.DbType.String) { Value = profile.ID });
                paramList.Add(new SqliteParameter("@name", System.Data.DbType.String) { Value = profile.Name });
                paramList.Add(new SqliteParameter("@meta", System.Data.DbType.String) { Value = profile.IntermediaryStorage.Path });
                paramList.Add(new SqliteParameter("@source", System.Data.DbType.String) { Value = profile.SyncSource.ID });

                dbAccess1.ExecuteNonQuery(insertProfileText, paramList);

                SQLiteSyncSourceProvider provider = (SQLiteSyncSourceProvider)SyncClient.GetSyncSourceProvider(this.StoragePath);
                provider.Add(profile.SyncSource, con1);

                provider = (SQLiteSyncSourceProvider)SyncClient.GetSyncSourceProvider(profile.IntermediaryStorage.Path);
                provider.Add(profile.SyncSource, con2);
                
                transaction1.Commit();
                transaction2.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction1.Rollback();
                transaction2.Rollback();
                throw;
            }
            finally
            {
                if (con1 != null) con1.Dispose();
                if (con2 != null) con2.Dispose();
            }
        }           
        

        public bool Add(Profile profile, SqliteConnection con)
        {
            if (this.ProfileExists(profile.Name))
                throw new ProfileNameExistException("Profile " + profile.Name + " is already created");               

            using (SqliteCommand cmd = con.CreateCommand ())
            {
                cmd.CommandText = "INSERT INTO " + PROFILE_TABLE +
                                  " (" + COL_PROFILE_ID + ", " + COL_PROFILE_NAME +
                                  " ," + COL_METADATA_SOURCE_LOCATION + ", " + COL_SYNC_SOURCE_ID +
                                  ") VALUES (@id, @name, @meta, @source)";

                cmd.Parameters.Add ( new SqliteParameter("@id", System.Data.DbType.String) { Value = profile.ID });
                cmd.Parameters.Add(new SqliteParameter("@name", System.Data.DbType.String) { Value = profile.Name });
                cmd.Parameters.Add(new SqliteParameter("@meta", System.Data.DbType.String) { Value = profile.IntermediaryStorage.Path });
                cmd.Parameters.Add(new SqliteParameter("@source", System.Data.DbType.String) { Value = profile.SyncSource.ID });

                cmd.ExecuteNonQuery ();
            }                
            new SQLiteSyncSourceProvider(this.StoragePath).Add(profile.SyncSource, con);
            return true;
        }              
            
        

        public override bool ProfileExists(string profileName)
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine (this.StoragePath, Configuration.DATABASE_NAME ));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                // TODO: Change sql to SELECT COUNT(*)?
                string cmdText = "SELECT * FROM " + PROFILE_TABLE + " WHERE "
                                 + COL_PROFILE_NAME + " = @profileName";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@profileName", System.Data.DbType.String) { Value = profileName });

                bool found = false;
                db.ExecuteReader(cmdText, paramList, reader =>
                    { found = true; return; }
                );

                return found;
            }
        }
    }
}
