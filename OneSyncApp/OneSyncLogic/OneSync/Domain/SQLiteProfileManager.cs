/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    /// <summary>
    /// This class manages jobs related to profiles
    /// </summary>
    public class SQLiteProfileManager: BaseSQLiteProvider, IProfileManager
    {        
        public SQLiteProfileManager(string baseFolder):base(baseFolder)
        {            
        }
        //default constructor
        public SQLiteProfileManager() { }
        #region IProfileManager Members
       
        /// <summary>
        /// Given a sync source absolute path, return a list of profiles contain that sources
        /// </summary>
        /// <param name="path"></param>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public IList<Profile> FindByDataSource(string path, IList<Profile> profiles)
        {
            IEnumerable<Profile> results = from profile in profiles
                                           where profile.SyncSource.Path.Equals(path)
                                           select profile;
            return results.ToList();
        }

        /// <summary>
        /// Given a list of profiles loaded from database, return a profile with specific id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public Profile FindByProfileId(string id, IList<Profile> profiles)
        {
            IEnumerable<Profile> results = from profile in profiles
                                           where profile.ID.Equals(id)
                                           select profile;
            IList<Profile> pList = results.ToList();
            if (pList.Count == 1) return pList[0];
            return null;
        }        
        #endregion

        #region IProfileManager Members

        /// <summary>
        /// Load all profiles
        /// </summary>
        /// <returns></returns>
        public IList<Profile> Load()
        {
            IList<Profile> profiles = new List<Profile>();
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand ())
                {
                    cmd.CommandText = "SELECT * FROM " + Profile.PROFILE_TABLE +
                        " p, " + SyncSource.DATASOURCE_INFO_TABLE +
                        " d WHERE p" + "." + Profile.SYNC_SOURCE_ID + " = d" + "." + SyncSource.SOURCE_ID;
                       
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SyncSource source = new SyncSource((string)reader[Profile.SYNC_SOURCE_ID], (string)reader[SyncSource.SOURCE_ABSOLUTE_PATH]);
                            IntermediaryStorage mdSource = new IntermediaryStorage((string)reader[Profile.METADATA_SOURCE_LOCATION]);
                            Profile p = new Profile((string)reader[Profile.PROFILE_ID],
                                (string)reader[Profile.PROFILE_NAME], source, mdSource);
                            profiles.Add(p);
                        }                        
                    }
                }
            }
            return profiles;
        }

        /// <summary>
        /// Load profile with specific profile id 
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public Profile Load(string profileId)
        {            
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM " + Profile.PROFILE_TABLE +
                        " p, " + SyncSource.DATASOURCE_INFO_TABLE +
                        " d WHERE p" + "." + Profile.SYNC_SOURCE_ID + " = d" + "." + SyncSource.SOURCE_ID +
                        " AND " + Profile.PROFILE_ID + " = @pid";
                    cmd.Parameters.Add(new SqliteParameter("@pid", System.Data.DbType.String) { Value = profileId});

                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SyncSource source = new SyncSource((string)reader[Profile.SYNC_SOURCE_ID], (string)reader[SyncSource.SOURCE_ABSOLUTE_PATH]);
                            IntermediaryStorage mdSource = new IntermediaryStorage((string)reader[Profile.METADATA_SOURCE_LOCATION]);
                            Profile p = new Profile((string)reader[Profile.PROFILE_ID],
                                (string)reader[Profile.PROFILE_NAME], source, mdSource);
                            return p;
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        #region IProfileManager Members                      


        /// <summary>
        /// Update a profile requires update 2 tables at the same time, 
        /// If one update on a table fails, the total update action must fail too.
        /// </summary>
        /// <param name="profile"></param>
        public void Update(Profile profile)
        {
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    using (SqliteCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE " + Profile.PROFILE_TABLE + 
                            " SET " + Profile.METADATA_SOURCE_LOCATION + " = @mdSource, " +
                            Profile.PROFILE_NAME + " = @name WHERE " + Profile.PROFILE_ID + " = @id";
                        cmd.Parameters.Add(new SqliteParameter("@mdSource", System.Data.DbType.String) { Value = profile.IntermediaryStorage.Path});
                        cmd.Parameters.Add(new SqliteParameter("@name", System.Data.DbType.String) { Value = profile.Name});
                        cmd.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.String) { Value = profile.ID });                        
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "UPDATE " + SyncSource.DATASOURCE_INFO_TABLE +
                            " SET " + SyncSource.SOURCE_ABSOLUTE_PATH + " = @path WHERE " + SyncSource.SOURCE_ID + " = @gid" ;
                        cmd.Parameters.Add(new SqliteParameter("@path", System.Data.DbType.String) { Value = profile.SyncSource.Path });
                        cmd.Parameters.Add(new SqliteParameter("@gid",System.Data.DbType.String){Value = profile.SyncSource.ID} );
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }                    
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("Database Exception");
                }                
            }
        }


        /// <summary>
        /// Delete a profile requires delete data from 2 tables DATASOURCE_INFO and PROFILE
        /// If deletion action on one table fails, the total action must fail too.
        /// </summary>
        /// <param name="profile"></param>
        public void Delete(Profile profile)
        {
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    using (SqliteCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM " + SyncSource.DATASOURCE_INFO_TABLE +
                            " WHERE " + SyncSource.SOURCE_ID + " = @sid";
                        cmd.Parameters.Add(new SqliteParameter("@sid", System.Data.DbType.String) { Value = profile.SyncSource.ID });
                        cmd.ExecuteNonQuery();                        
                        
                        cmd.CommandText = "DELETE FROM " + Profile.PROFILE_TABLE +                        
                            " WHERE " + Profile.PROFILE_ID + " = @pid";
                        cmd.Parameters.Add(new SqliteParameter("@pid", System.Data.DbType.String) { Value = profile.ID});
                        cmd.ExecuteNonQuery();
                        
                        transaction.Commit();
                    }                    
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("Database Exception");
                }
            }
        }
      
        #endregion

        #region IProfileManager Members

       

        /// <summary>
        /// Create schema for profile table
        /// </summary>
        public void CreateSchema(SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {       
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + Profile.PROFILE_TABLE +
                            "(" + Profile.PROFILE_ID + " VARCHAR(50) PRIMARY KEY, "
                            + Profile.PROFILE_NAME + " VARCHAR(50) UNIQUE NOT NULL, "
                            + Profile.METADATA_SOURCE_LOCATION + " TEXT, "
                            + Profile.SYNC_SOURCE_ID + " VARCHAR (50), "
                            + "FOREIGN KEY (" + Profile.SYNC_SOURCE_ID + ") REFERENCES "
                            + SyncSource.DATASOURCE_INFO_TABLE + "(" + SyncSource.SOURCE_ID + "))"
                            ;
                    cmd.ExecuteNonQuery();                                           
            }
        }

        #endregion

        #region IProfileManager Members

        /// <summary>
        /// Insert new profile
        /// </summary>
        /// <param name="profile"></param>
        public void Insert(Profile profile, SqliteConnection con)
        {
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO " + Profile.PROFILE_TABLE +
                        " (" + Profile.PROFILE_ID + ", " + Profile.PROFILE_NAME +
                        " ," + Profile.METADATA_SOURCE_LOCATION + ", " +
                        Profile.SYNC_SOURCE_ID + ") VALUES (@id, @name, @meta, @source)";
                    SqliteParameter param1 = new SqliteParameter("@id", System.Data.DbType.String);
                    param1.Value = profile.ID;
                    cmd.Parameters.Add(param1);

                    SqliteParameter param2 = new SqliteParameter("@name", System.Data.DbType.String);
                    param2.Value = profile.Name;
                    cmd.Parameters.Add(param2);

                    SqliteParameter param3 = new SqliteParameter("@meta", System.Data.DbType.String);
                    param3.Value = profile.IntermediaryStorage.Path;
                    cmd.Parameters.Add(param3);

                    SqliteParameter param4 = new SqliteParameter("@source", System.Data.DbType.String);
                    param4.Value = profile.SyncSource.ID;
                    cmd.Parameters.Add(param4);

                    cmd.ExecuteNonQuery();
                }            
        }
        #endregion

        public bool IsAlreadyCreated (string profileName)
        {
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM " + Profile.PROFILE_TABLE + " WHERE "
                        + Profile.PROFILE_NAME + " = @profileName";
                    cmd.Parameters.Add (new SqliteParameter ("@profileName",  System.Data.DbType.String){Value = profileName});
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) { return true; }                       
                    }
                }                
            }
            return false;
        }
    }
}
