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
using System.Data;

namespace OneSync.Synchronization
{
    
    public class SQLiteSyncSourceProvider : SyncSourceProvider
    {
        public const string DATABASE_NAME = "data.md";
        public const string ACTION_TABLE = "ACTION_TABLE";
        public const string ACTION_ID = "ACTION_ID";
        public const string CHANGE_IN = "CHANGE_IN";
        public const string ACTION_TYPE = "ACTION_TYPE";
        public const string OLD_RELATIVE_PATH = "OLD_RELATIVE_PATH";
        public const string NEW_RELATIVE_PATH = "NEW_RELATIVE_PATH";
        public const string OLD_HASH = "OLD_HASH";
        public const string NEW_HASH = "NEW_HASH";


        // TODO: move constants declaration to centralized location?
        public const string SOURCE_ABSOLUTE_PATH = "SOURCE_ABSOLUTE_PATH";
        public const string SOURCE_ID = "SOURCE_ID";
        public const string DATASOURCE_INFO_TABLE = "DATASOURCE_INFO_TABLE";

        public SQLiteSyncSourceProvider(string storagePath)
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

        public override bool Add(SyncSource s)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "INSERT INTO " + DATASOURCE_INFO_TABLE +
                                 "(" + SOURCE_ID + "," + SOURCE_ABSOLUTE_PATH + ") VALUES (@id, @path)";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", DbType.String) { Value = s.ID });
                paramList.Add(new SqliteParameter("@path", DbType.String) { Value = s.Path });

                db.ExecuteNonQuery(cmdText, paramList, false);

            }
            return true;
        }

        public override IList<SyncSource> LoadAll()
        {
            IList<SyncSource> syncSources = new List<SyncSource>();

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "SELECT * FROM  " + DATASOURCE_INFO_TABLE;

                db.ExecuteReader(cmdText, reader =>
                    {
                        syncSources.Add(new SyncSource((string)reader[SOURCE_ID], (string)reader[SOURCE_ABSOLUTE_PATH]));
                    }
                );
            }

            return syncSources;
        }

        public override bool Update(SyncSource source)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "UPDATE " + DATASOURCE_INFO_TABLE +
                        " SET " + SOURCE_ABSOLUTE_PATH + " = @path WHERE " + SOURCE_ID + " = @id";

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
                using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
                {
                    string cmdText = "SELECT COUNT (DISTINCT " + SOURCE_ID + ") AS num" +
                            " FROM " + DATASOURCE_INFO_TABLE;


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
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + DATASOURCE_INFO_TABLE +
                                 " ( " + SOURCE_ABSOLUTE_PATH + " TEXT, " +
                                 SOURCE_ID + " TEXT PRIMARY KEY)";

                db.ExecuteNonQuery(cmdText, false);
            }
        }
    }
}
