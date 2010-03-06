using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    /// <summary>
    /// This class manages list of profiles
    /// </summary>
    public class SQLiteProfileManager: BaseSQLiteProvider, IProfileManager
    {        
        IList<Profile> profiles = new List<Profile>();
        private SqliteConnection con = null;

        public SQLiteProfileManager(string baseFolder):base(baseFolder)
        {            
        }

        public SQLiteProfileManager(SqliteConnection con)
        {
            this.con = con;
        }

        #region IProfileManager Members
       
        public IList<Profile> FindByDataSource(string path)
        {
            IEnumerable<Profile> results = from profile in profiles
                                           where profile.SyncSource.Path.Equals(path)
                                           select profile;
            return results.ToList();
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
                        " d WHERE p" + "." + Profile.SYNC_SOURCE_ID + " = d" + "." + SyncSource.GID;
                       
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SyncSource source = new SyncSource((string)reader[Profile.SYNC_SOURCE_ID], (string)reader[SyncSource.PATH]);
                            IntermediaryStorage mdSource = new IntermediaryStorage((string)reader[Profile.METADATA_SOURCE_LOCATION]);
                            Profile p = new Profile((string)reader[Profile.PROFILE_ID],
                                (string)reader[Profile.NAME], source, mdSource);
                            profiles.Add(p);
                        }                        
                    }
                }
            }
            return profiles;
        }

        #endregion

        #region IProfileManager Members
                       

        public void Update(Profile profile)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IProfileManager Members

        /// <summary>
        /// Delete a profile
        /// </summary>
        /// <param name="profile"></param>
        public void Delete(Profile profile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create schema for profile table
        /// </summary>
        public void CreateSchema(SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {       
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + Profile.PROFILE_TABLE +
                            "(" + Profile.PROFILE_ID + " VARCHAR(50) PRIMARY KEY, "
                            + Profile.NAME + " VARCHAR(50) UNIQUE NOT NULL, "
                            + Profile.METADATA_SOURCE_LOCATION + " TEXT, "
                            + Profile.SYNC_SOURCE_ID + " VARCHAR (50), "
                            + "FOREIGN KEY (" + Profile.SYNC_SOURCE_ID + ") REFERENCES "
                            + SyncSource.DATASOURCE_INFO_TABLE + "(" + SyncSource.GID + "))"
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
        public void Insert(Profile profile)
        {
            
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO " + Profile.PROFILE_TABLE +
                        " (" + Profile.PROFILE_ID + ", " + Profile.NAME + 
                        " ," + Profile.METADATA_SOURCE_LOCATION + ", " +
                        Profile.SYNC_SOURCE_ID +") VALUES (@id, @name, @meta, @source)" ;
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
    }
}
