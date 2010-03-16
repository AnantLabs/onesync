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
            this.CreateSchema();
        }

        public override bool Add(SyncSource s)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "INSERT INTO " + Configuration.TBL_SYNCSOURCE_INFO +
                                 "(" + Configuration.COL_SOURCE_ID + "," + Configuration.COL_SOURCE_ABSOLUTE_PATH + 
                                 ") VALUES (@id, @path)";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", DbType.String) { Value = s.ID });
                paramList.Add(new SqliteParameter("@path", DbType.String) { Value = s.Path });

                db.ExecuteNonQuery(cmdText, paramList);

            }
            return true;
        }

        public override IList<SyncSource> LoadAll()
        {
            IList<SyncSource> syncSources = new List<SyncSource>();

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "SELECT * FROM  " + Configuration.TBL_SYNCSOURCE_INFO;

                db.ExecuteReader(cmdText, reader =>
                    {
                        syncSources.Add(new SyncSource((string)reader[Configuration.COL_SOURCE_ID],
                                                       (string)reader[Configuration.COL_SOURCE_ABSOLUTE_PATH]));
                    }
                );
            }

            return syncSources;
        }

        public override bool Update(SyncSource source)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "UPDATE " + Configuration.TBL_SYNCSOURCE_INFO +
                        " SET " + Configuration.COL_SOURCE_ABSOLUTE_PATH + " = @path WHERE "
                        + Configuration.COL_SOURCE_ID + " = @id";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", DbType.String) { Value = source.ID });
                paramList.Add(new SqliteParameter("@path", DbType.String) { Value = source.Path });

                db.ExecuteNonQuery(cmdText, false);
            }
            return true;
        }

        public override int GetSyncSourceCount()
        {
            try
            {
                using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
                {
                    string cmdText = "SELECT COUNT (DISTINCT " + Configuration.COL_SOURCE_ID + ") AS num" +
                            " FROM " + Configuration.TBL_SYNCSOURCE_INFO;


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

        private void CreateSchema()
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + Configuration.TBL_SYNCSOURCE_INFO +
                                 " ( " + Configuration.COL_SOURCE_ABSOLUTE_PATH + " TEXT, " +
                                 Configuration.COL_SOURCE_ID + " TEXT PRIMARY KEY)";

                db.ExecuteNonQuery(cmdText, false);
            }
        }
    }
}
