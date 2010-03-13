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
    // TODO: wrap all db stmts in try catch and return false if failed?

    public class SQLiteMetaDataProvider : MetaDataProvider
    {
        // TODO: store database constants elsewhere?

        // Database table and column names
        public const string DATABASE_NAME = "data.md";
        public const string DATASOURCE_INFO_TABLE = "DATASOURCE_INFO_TABLE";
        public const string METADATA_TABLE = "METADATA_TABLE";
        public const string COL_SOURCE_ID = "SOURCE_ID";
        public const string COL_FULL_NAME = "FULL_NAME"; // TODO: not used???
        public const string COL_RELATIVE_PATH = "RELATIVE_PATH";
        public const string COL_HASH_CODE = "HASH_CODE";
        public const string COL_LAST_MODIFIED_TIME = "LAST_MODIFIED_TIME";
        public const string COL_NTFS_ID1 = "NTFS_ID1";
        public const string COL_NTFS_ID2 = "NTFS_ID2";


        /// <summary>
        /// Creates an SQLiteMetaDataProvider that manages metadata stored in specified path.
        /// </summary>
        /// <param name="storagePath">Location where all metadata are saved to or loaded from.</param>
        /// <param name="rootPath">To be combined with relative paths to give absolute paths of files.</param>
        public SQLiteMetaDataProvider(string storagePath, string rootPath)
            : base(storagePath, rootPath)
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

        public override FileMetaData Load(string currId, bool loadOther)
        {
            string opt = (loadOther) ? " <> " : " = ";

            FileMetaData mData = new FileMetaData(currId, this.RootPath);

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "SELECT * FROM " + METADATA_TABLE + 
                                 " WHERE " + COL_SOURCE_ID + opt + " @sourceId";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@sourceId", DbType.String) { Value = currId });

                db.ExecuteReader(cmdText, paramList, reader =>
                    {
                        mData.MetaDataItems.Add(new FileMetaDataItem(
                            (string)reader[COL_SOURCE_ID],
                            this.RootPath + (string)reader[COL_RELATIVE_PATH], (string)reader[COL_RELATIVE_PATH],
                            (string)reader[COL_HASH_CODE], (DateTime)reader[COL_LAST_MODIFIED_TIME],
                            Convert.ToUInt32(reader[COL_NTFS_ID1]), Convert.ToUInt32(reader[COL_NTFS_ID2])));
                    }
                );
            }
            return mData;
        }


        public override bool Add(FileMetaData mData)
        {
            return this.Add(mData.MetaDataItems);
        }


        public override bool Add(IList<FileMetaDataItem> mData)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                SqliteCommand cmd = db.GetTransactCommand();

                try
                {
                    foreach (FileMetaDataItem item in mData)
                    {
                        cmd.CommandText = "INSERT INTO " + METADATA_TABLE +
                                     "( " + COL_SOURCE_ID + "," + COL_RELATIVE_PATH + "," +
                                     COL_HASH_CODE + "," +
                                     COL_LAST_MODIFIED_TIME + "," +
                                     COL_NTFS_ID1 + "," +
                                     COL_NTFS_ID2 + ")" +
                                     "VALUES (@source_id , @relative_path, @hash_code, @last_modified_time, @ntfs_id1, @ntfs_id2) ";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqliteParameter("@source_id", DbType.String) { Value = item.SourceId });
                        cmd.Parameters.Add(new SqliteParameter("@relative_path", DbType.String) { Value = item.RelativePath });
                        cmd.Parameters.Add(new SqliteParameter("@hash_code", DbType.String) { Value = item.HashCode });
                        cmd.Parameters.Add(new SqliteParameter("@last_modified_time", DbType.DateTime) { Value = item.LastModifiedTime });
                        cmd.Parameters.Add(new SqliteParameter("@ntfs_id1", DbType.Int32) { Value = item.NTFS_ID1 });
                        cmd.Parameters.Add(new SqliteParameter("@ntfs_id2", DbType.Int32) { Value = item.NTFS_ID2 });

                        cmd.ExecuteNonQuery();
                    }

                    cmd.Transaction.Commit();

                }
                catch (Exception)
                {
                    if (cmd.Transaction != null) cmd.Transaction.Rollback();
                    // Log error??

                    return false;
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                }

            }

            return true;
        } 


        public override bool Delete(IList<FileMetaDataItem> items)
        {
            // All deletions are atomic

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                SqliteCommand cmd = db.GetTransactCommand();

                try
                {
                    foreach (FileMetaDataItem item in items)
                    {
                        cmd.CommandText = "DELETE FROM " + METADATA_TABLE +
                                          " WHERE " + COL_SOURCE_ID + " = @sourceId AND " +
                                          COL_RELATIVE_PATH + " = @path";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqliteParameter("@sourceId", DbType.String) { Value = item.SourceId });
                        cmd.Parameters.Add(new SqliteParameter("@path", DbType.String) { Value = item.RelativePath });

                        cmd.ExecuteNonQuery();
                    }

                    cmd.Transaction.Commit();
                }
                catch (Exception)
                {
                    if (cmd.Transaction != null) cmd.Transaction.Rollback();
                    // Log error??

                    return false;
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                }

            }

            return true;
        }


        public override bool Update(IList<FileMetaDataItem> items)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                foreach (FileMetaDataItem item in items)
                {
                    string cmdText = "UPDATE " + METADATA_TABLE +
                                     " SET " + COL_HASH_CODE + " = @hash, " +
                                     COL_LAST_MODIFIED_TIME + " = @lmf" +
                                     " WHERE " + COL_RELATIVE_PATH + " = @rel AND " +
                                     COL_SOURCE_ID + " = @sourceId";

                    SqliteParameterCollection paramList = new SqliteParameterCollection();
                    paramList.Add(new SqliteParameter("@hash", DbType.String) { Value = item.HashCode });
                    paramList.Add(new SqliteParameter("@lmf", DbType.DateTime) { Value = item.LastModifiedTime });
                    paramList.Add(new SqliteParameter("@rel", DbType.String) { Value = item.RelativePath });
                    paramList.Add(new SqliteParameter("@sourceId", DbType.String) { Value = item.SourceId });

                    db.ExecuteNonQuery(cmdText, false);
                }
            }

            return true;
        }


        public override bool Update(FileMetaData oldMetadata, FileMetaData newMetada)
        {
            //Get newly created items by comparing relative paths
            //newOnly is metadata item in current metadata but not in old one
            IEnumerable<FileMetaDataItem> newOnly = from _new in newMetada.MetaDataItems
                                                    where !oldMetadata.MetaDataItems.Contains(_new, new FileMetaDataItemComparer())
                                                    select _new;

            //Get deleted items           
            IEnumerable<FileMetaDataItem> oldOnly = from old in oldMetadata.MetaDataItems
                                                    where !newMetada.MetaDataItems.Contains(old, new FileMetaDataItemComparer())
                                                    select old;

            //get the items from 2 metadata with same relative paths but different hashes.
            IEnumerable<FileMetaDataItem> bothModified = from _new in newMetada.MetaDataItems
                                                         from old in oldMetadata.MetaDataItems
                                                         where _new.RelativePath.Equals((old).RelativePath)
                                                         && !_new.HashCode.Equals(old.HashCode)
                                                         select _new;

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                // TODO: make 3 actions atomic
                this.Add(newOnly.ToList());
                this.Delete(oldOnly.ToList());
                this.Update(bothModified.ToList());
            }

            return true;
        }


        private void CreateSchema()
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, DATABASE_NAME)))
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + METADATA_TABLE +
                                                " ( " + COL_SOURCE_ID + " TEXT, " +
                                                COL_RELATIVE_PATH + " TEXT, " +
                                                COL_HASH_CODE + " TEXT, " +
                                                COL_LAST_MODIFIED_TIME + " DATETIME, " +
                                                COL_NTFS_ID1 + " INT, " +
                                                COL_NTFS_ID2 + " INT," +
                                                "FOREIGN KEY (" + COL_SOURCE_ID + ") REFERENCES " + DATASOURCE_INFO_TABLE + "(" + COL_SOURCE_ID + ")" +
                                                "PRIMARY KEY (" + COL_SOURCE_ID + "," + COL_RELATIVE_PATH + ")" +
                                                ")";

                db.ExecuteNonQuery(cmdText, false);
            }
        }

    }
}
