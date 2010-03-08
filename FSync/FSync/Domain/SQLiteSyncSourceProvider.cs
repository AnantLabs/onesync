using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;

namespace OneSync.Synchronization
{
    /// <summary>
    /// This class provide sync agent information about sync source objects
    /// </summary>
    public class SQLiteSyncSourceProvider:BaseSQLiteProvider, ISyncSourceProvider
    {
        private SqliteConnection con = null;
                       
        public SQLiteSyncSourceProvider(string baseFolder):base(baseFolder)
        {
            this.baseFolder = baseFolder;
        }

        /// <summary>
        /// This constructor is called when SQLiteSyncSourceProvider involved in 
        /// any operation that requires transaction features (for commit and rollback purpose)
        /// </summary>
        /// <param name="con"></param>
        public SQLiteSyncSourceProvider(SqliteConnection con)
        {
            this.con = con;
        }

        #region ISyncSourceProvider Members
        /// <summary>
        /// Insert sync source object
        /// </summary>
        /// <param name="source"></param>
        public void Insert(SyncSource source)
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
            IList<SyncSource> replicas = new List<SyncSource>();
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
                            replicas.Add(new SyncSource((string)reader[SyncSource.SOURCE_ID], (string)reader[SyncSource.SOURCE_ABSOLUTE_PATH]));
                        }
                    }
                }
            }
            return replicas;
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
        /// Delete sync source object
        /// </summary>
        /// <param name="source"></param>
        public void Delete(SyncSource source)
        {
            //Drop table

        }

        /// <summary>
        /// To find that list of sync sources that have specific absolute paths
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IList<SyncSource> FindByPath( string path)
        {
            IList<SyncSource> sources = Load();
            return ( from source in sources
                     where source.Path.Equals(path)
                     select source).ToList();            
        }

        /// <summary>
        /// To find sync source that has specific and unique id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SyncSource FindById(string id)
        {
            IList<SyncSource> sources = Load();
            IEnumerable<SyncSource> enumSources = from source in sources
                                                  where source.ID.Equals(id)
                                                  select source;
            //sync source id is a unique number
            foreach (SyncSource source in enumSources)
            {
                return source;
            }
            return null;
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
    }
}
