/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;
using System.IO;

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
                this.CreateSchema();
            }
        }

        /// <summary>
        /// Create schema for profile table
        /// </summary>
        public void CreateSchema()
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
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

        public override IList<Profile> LoadAllProfiles()
        {
            IList<Profile> profiles = new List<Profile>();

            // Note: The SQL depends on other tables as well which might not be created.
            // So empty profile list should be returned if there is an exception

            try
            {
                using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
                {
                    string cmdText = "SELECT * FROM " + PROFILE_TABLE +
                                     " p, " + DATASOURCE_INFO_TABLE +
                                     " d WHERE p" + "." + COL_SYNC_SOURCE_ID + " = d" + "." + COL_SOURCE_ID;

                    db.ExecuteReader(cmdText, reader =>
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
                    );
                }

            }
            catch (Exception)
            {
                // Log error?
            }
            
            return profiles;
        }

        public override Profile Load(string profileId)
        {
            Profile p = null;

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
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

        public override bool CreateProfile(string profileName, string absoluteSyncPath, string absoluteIntermediatePath)
        {
            throw new NotImplementedException();
        }

        public override bool Update(Profile profile)
        {
            // Update a profile requires update 2 tables at the same time, 
            // If one update on a table fails, the total update action must fail too.

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                SqliteParameterCollection paramList = new SqliteParameterCollection();

                string cmdText = "UPDATE " + PROFILE_TABLE +
                                 " SET " + COL_METADATA_SOURCE_LOCATION + " = @mdSource, " +
                                 COL_PROFILE_NAME + " = @name WHERE " + COL_PROFILE_ID + " = @id;";

                
                // Add parameters for 1st Update statement
                paramList.Add(new SqliteParameter("@mdSource", System.Data.DbType.String) { Value = profile.IntermediaryStorage.Path });
                paramList.Add(new SqliteParameter("@name", System.Data.DbType.String) { Value = profile.Name });
                paramList.Add(new SqliteParameter("@id", System.Data.DbType.String) { Value = profile.ID });

                // Concat 2nd Update statement
                cmdText += "UPDATE " + SyncSource.DATASOURCE_INFO_TABLE +
                           " SET " + SyncSource.SOURCE_ABSOLUTE_PATH + " = @path WHERE " + COL_SOURCE_ID + " = @gid";

                // Add parameters for 2nd Update statement
                paramList.Add(new SqliteParameter("@path", System.Data.DbType.String) { Value = profile.SyncSource.Path });
                paramList.Add(new SqliteParameter("@gid", System.Data.DbType.String) { Value = profile.SyncSource.ID });

                db.ExecuteNonQuery(cmdText, paramList, true);
            }

            return true;
        }

        public override bool Delete(Profile profile)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                SqliteParameterCollection paramList = new SqliteParameterCollection();

                string cmdText = "DELETE FROM " + SyncSource.DATASOURCE_INFO_TABLE +
                                 " WHERE " + SyncSource.SOURCE_ID + " = @sid;";
                
                paramList.Add(new SqliteParameter("@sid", System.Data.DbType.String) { Value = profile.SyncSource.ID });

                cmdText += "DELETE FROM " + PROFILE_TABLE +
                           " WHERE " + COL_PROFILE_ID + " = @pid";

                paramList.Add(new SqliteParameter("@pid", System.Data.DbType.String) { Value = profile.ID });

                db.ExecuteNonQuery(cmdText, paramList, true);
            }

            return true;
        }

        public override bool Add(Profile profile)
        {
            if (this.ProfileExists(profile.Name))
                throw new ProfileNameExistException("Profile " + profile.Name + " is already created");

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "INSERT INTO " + PROFILE_TABLE +
                                 " (" + COL_PROFILE_ID + ", " + COL_PROFILE_NAME +
                                 " ," + COL_METADATA_SOURCE_LOCATION + ", " + COL_SYNC_SOURCE_ID +
                                 ") VALUES (@id, @name, @meta, @source)";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", System.Data.DbType.String) { Value = profile.ID });
                paramList.Add(new SqliteParameter("@name", System.Data.DbType.String) { Value = profile.Name });
                paramList.Add(new SqliteParameter("@meta", System.Data.DbType.String) { Value = profile.IntermediaryStorage.Path });
                paramList.Add(new SqliteParameter("@source", System.Data.DbType.String) { Value = profile.SyncSource.ID });

                db.ExecuteNonQuery(cmdText, false);
            }

            return true;
        }

        public override bool ProfileExists(string profileName)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
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
