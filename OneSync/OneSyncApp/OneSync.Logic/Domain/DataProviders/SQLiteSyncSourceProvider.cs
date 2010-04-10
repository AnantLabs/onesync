using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    
    public class SQLiteSyncSourceProvider : SyncSourceProvider
    {       
        public SQLiteSyncSourceProvider(string storagePath)
            : base(storagePath)
        {
            // Create database schema if necessary

            FileInfo fi = new FileInfo(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));

            if (!fi.Exists)
            {
                // If the parent directory already exists, Create() does nothing.
                fi.Directory.Create();
            }

            // Create table if it does not exist
            
        }
        
        /// <summary>
        /// This method takes in SQLiteConnection object as a parameter
        /// </summary>
        /// <param name="s"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public  bool Add(SyncSource s, SqliteConnection con)
        {            
             using (SqliteCommand cmd = con.CreateCommand())
             {
                 cmd.CommandText = "INSERT INTO " + Configuration.TBL_DATASOURCE_INFO +
                                 "(" + Configuration.COL_SOURCE_ID + "," + Configuration.COL_SOURCE_ABSOLUTE_PATH +
                                 ") VALUES (@id, @path)";               
                 cmd.Parameters.Add(new SqliteParameter("@id", DbType.String) { Value = s.ID });
                 cmd.Parameters.Add(new SqliteParameter("@path", DbType.String) { Value = s.Path });
                 cmd.ExecuteNonQuery();                               
             }
             return true;
        }

        /// <summary>
        /// Load all the sync sources objects
        /// </summary>
        /// <returns></returns>
        public override IList<SyncSource> LoadAll()
        {
            IList<SyncSource> syncSources = new List<SyncSource>();

            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME),true);
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                using (SqliteCommand cmd = con.CreateCommand ())
                {
                    cmd.CommandText = "SELECT * FROM  " + Configuration.TBL_DATASOURCE_INFO;
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            syncSources.Add(new SyncSource((string)reader[Configuration.COL_SOURCE_ID],
                                                       (string)reader[Configuration.COL_SOURCE_ABSOLUTE_PATH]));
                        }
                    }
                }       
            }

            return syncSources;
        }

        /// <summary>
        /// Update details of a sync source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override bool Update(SyncSource source)
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME),true);
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                string cmdText = "UPDATE " + Configuration.TBL_DATASOURCE_INFO +
                        " SET " + Configuration.COL_SOURCE_ABSOLUTE_PATH + " = @path WHERE "
                        + Configuration.COL_SOURCE_ID + " = @id";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                paramList.Add(new SqliteParameter("@path", DbType.String) { Value = source.Path });

                db.ExecuteNonQuery(cmdText, false);
            }
            return true;
        }

        public override bool Delete(SyncSource source)
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME),true);
            using (SqliteConnection con = db.NewSQLiteConnection())
            {
                string cmdText = "DELETE FROM " + Configuration.TBL_DATASOURCE_INFO +
                        " WHERE " + Configuration.COL_SOURCE_ID + " = @id";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });

                db.ExecuteNonQuery(cmdText, paramList);
            }
            return true;
        }

        public override bool DeleteSyncSourceInIntermediateStorage(SyncSource source)
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME),false);
            SqliteTransaction transaction = null ;
            try
            {
                using (SqliteConnection con = db.NewSQLiteConnection())
                {
                    transaction = (SqliteTransaction)con.BeginTransaction();
                    using (SqliteCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM " + Configuration.TBL_METADATA +
                                            " WHERE " + Configuration.COL_SOURCE_ID + " = @id";
                        cmd.Parameters.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM " + Configuration.TLB_FOLDERMETADATA +
                                            " WHERE " + Configuration.COL_SOURCE_ID + " = @id"; 
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "DELETE FROM " + Configuration.TBL_DATASOURCE_INFO +
                                            " WHERE " + Configuration.COL_SOURCE_ID + " = @id";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "DELETE FROM " + Configuration.TBL_ACTION +
                                            " WHERE " + Configuration.COL_CHANGE_IN + " = @id";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                if ( transaction != null && transaction.Connection.State == ConnectionState.Open)  transaction.Rollback();
                return false;
            }            
            return true;
        }

        /// <summary>
        /// Update details of sync source
        /// Pass SQLiteConnection object to make atomic action
        /// </summary>
        /// <param name="source"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public bool Update(SyncSource source, SqliteConnection con )
        {          
            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "UPDATE " + Configuration.TBL_DATASOURCE_INFO +
                        " SET " + Configuration.COL_SOURCE_ABSOLUTE_PATH + " = @path WHERE "
                        + Configuration.COL_SOURCE_ID + " = @id";
                cmd.Parameters.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                cmd.Parameters.Add(new SqliteParameter("@path", DbType.String) { Value = source.Path });
                cmd.ExecuteNonQuery();
                return true;
            }
            
        }

        /// <summary>
        /// Get number of sync sources in intermediate storage
        /// Maximum numbers of sync sources can be added is 2
        /// </summary>
        /// <returns></returns>
        public override int GetSyncSourceCount()
        {
            try
            {
                SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME),true);
                using (SqliteConnection con = db.NewSQLiteConnection ())
                {
                    string cmdText = "SELECT COUNT (DISTINCT " + Configuration.COL_SOURCE_ID + ") AS num" +
                            " FROM " + Configuration.TBL_DATASOURCE_INFO;

                    return Convert.ToInt32(db.ExecuteScalar(cmdText, null));
                }
            }
            catch (Exception)
            {
                // Possible exception during first-run when DataSourceInfo Table not created yet.
                // Log error?
                return -1;
            }
        }

        /// <summary>
        /// Create schema of sync source table
        /// No transaction supported
        /// </summary>
        public override void CreateSchema()
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME),true);
            using (SqliteConnection con =  db.NewSQLiteConnection ())
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + Configuration.TBL_DATASOURCE_INFO +
                                 " ( " + Configuration.COL_SOURCE_ABSOLUTE_PATH + " TEXT, " +
                                 Configuration.COL_SOURCE_ID + " TEXT PRIMARY KEY)";

                db.ExecuteNonQuery(cmdText, false);
            }
        }

        /// <summary>
        /// Create schema of sync source table
        /// supports transaction
        /// </summary>
        /// <param name="con"></param>
        public void CreateSchema(SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + Configuration.TBL_DATASOURCE_INFO +
                                 " ( " + Configuration.COL_SOURCE_ABSOLUTE_PATH + " TEXT, " +
                                 Configuration.COL_SOURCE_ID + " TEXT PRIMARY KEY)";
                cmd.ExecuteNonQuery();
            }
        }
               
        /// <summary>
        /// Add sync source to database
        /// no transaction supports
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public override bool Add(SyncSource s)
        {
            if (GetSyncSourceCount() == 2) throw new SyncSourcesNumberExceededException("The intermediate storage can only be attached to one profile");

            string insertText =  "INSERT INTO " + Configuration.TBL_DATASOURCE_INFO +
                                 "(" + Configuration.COL_SOURCE_ID + "," + Configuration.COL_SOURCE_ABSOLUTE_PATH +
                                 ") VALUES (@id, @path)";
            
            SqliteParameterCollection paramList = new SqliteParameterCollection ();
            paramList.Add( new SqliteParameter("@id", DbType.String) { Value = s.ID } );
            paramList.Add( new SqliteParameter("@path", DbType.String) { Value = s.Path });

            SQLiteAccess dbAccess = new SQLiteAccess(Path.Combine (this.StoragePath, Configuration.DATABASE_NAME),true);
            using (SqliteConnection con = dbAccess.NewSQLiteConnection())
            {
                dbAccess.ExecuteNonQuery(insertText, paramList);
            }
            return true;
        }
    }
}
