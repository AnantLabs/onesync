using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;

namespace OneSync.Synchronization
{
    public enum SourceOption
    {
        NOT_EQUAL_SOURCE_ID,
        EQUAL_SOURCE_ID
    }

    /// <summary>
    /// This class provide sync agent information about sync source objects
    /// </summary>
    public class SQLiteSyncSourceProvider:BaseSQLiteProvider, ISyncSourceProvider
    {
        public SQLiteSyncSourceProvider(string baseFolder):base(baseFolder)
        {
            this.baseFolder = baseFolder;
        }

        //default constructor
        public SQLiteSyncSourceProvider() { }

        #region ISyncSourceProvider Members
        /// <summary>
        /// Insert sync source object
        /// </summary>
        /// <param name="source"></param>
        public void Insert(SyncSource source, SqliteConnection con )
        {                        
             using (SqliteCommand cmd = con.CreateCommand())
             {
                    cmd.CommandText = "INSERT INTO " + SyncSource.DATASOURCE_INFO_TABLE + 
                        "(" + SyncSource.SOURCE_ID + "," + SyncSource.SOURCE_ABSOLUTE_PATH + ") VALUES (@id, @path)" ;
                    SqliteParameter param1 = new SqliteParameter("@id", System.Data.DbType.String);
                    param1.Value = source.ID;
                    cmd.Parameters.Add(param1);

                    SqliteParameter param2 = new SqliteParameter("@path", System.Data.DbType.String);
                    param2.Value = source.Path;
                    cmd.Parameters.Add(param2);
                    cmd.ExecuteNonQuery();
            }
            
        }    


        /// <summary>
        /// Load information of replicas
        /// </summary>
        /// <returns></returns>
        public IList<SyncSource> Load()
        {
            IList<SyncSource> syncSources = new List<SyncSource>();
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {   
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM  " + SyncSource.DATASOURCE_INFO_TABLE;
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            syncSources.Add(new SyncSource((string)reader[SyncSource.SOURCE_ID], (string)reader[SyncSource.SOURCE_ABSOLUTE_PATH]));
                        }
                    }
                }
            }
            return syncSources;
        }
        /// <summary>
        /// Update sync source object
        /// </summary>
        /// <param name="source"></param>
        public void Update(SyncSource source)
        {
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {                
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE " + SyncSource.DATASOURCE_INFO_TABLE +
                        " SET " + SyncSource.SOURCE_ABSOLUTE_PATH + " = @path WHERE " + SyncSource.SOURCE_ID + " = @id" ;
                    SqliteParameter param1 = new SqliteParameter("@id", System.Data.DbType.String);
                    param1.Value = source.ID;
                    cmd.Parameters.Add(param1);

                    SqliteParameter param2 = new SqliteParameter("@path", System.Data.DbType.String);
                    param2.Value = source.Path;
                    cmd.Parameters.Add(param2);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Create new empty table
        /// </summary>
        public void CreateSchema(SqliteConnection con)
        {            
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + SyncSource.DATASOURCE_INFO_TABLE +
                                                " ( " + SyncSource.SOURCE_ABSOLUTE_PATH + " TEXT, " +
                                                SyncSource.SOURCE_ID + " TEXT PRIMARY KEY)";
                    cmd.ExecuteNonQuery();
                }            
        }

        #endregion

        /// <summary>
        /// Get the number of sync sources which already participate in sync job.
        /// Maximum 2 sync sources are allowed.
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfSyncSources()
        {
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT (DISTINCT " + SyncSource.SOURCE_ID + ") AS num" +
                        " FROM " + SyncSource.DATASOURCE_INFO_TABLE;
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return Convert.ToInt32((Int64)reader[0]);
                        }
                    }
                }
            }
            return -1;
        }
    }
}
