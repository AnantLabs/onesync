/*
 $Id: SQLiteMetaDataProvider.cs 83 2010-03-14 15:14:27Z deskohet $
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

        /// <summary>
        /// Creates an SQLiteMetaDataProvider that manages metadata stored in specified path.
        /// </summary>
        /// <param name="storagePath">Location where all metadata are saved to or loaded from.</param>
        /// <param name="rootPath">To be combined with relative paths to give absolute paths of files.</param>
        public SQLiteMetaDataProvider(string storagePath, string rootPath)
            : base(storagePath, rootPath)
        {
            // Create database schema if necessary

            FileInfo fi = new FileInfo(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));

            if (!fi.Exists)
            {
                // If the parent directory already exists, Create() does nothing.
                fi.Directory.Create();
            }
        }

        public override FileMetaData Load(string currId, SourceOption option)
        {
            string opt = (option == SourceOption.SOURCE_ID_NOT_EQUALS ) ? " <> " : " = ";

            FileMetaData mData = new FileMetaData(currId, this.RootPath);

            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                string cmdText = "SELECT * FROM " + Configuration.TBL_METADATA + 
                                 " WHERE " + Configuration.COL_SOURCE_ID + opt + " @sourceId";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@sourceId", DbType.String) { Value = currId });

                db.ExecuteReader(cmdText, paramList, reader =>
                    {
                        mData.MetaDataItems.Add(new FileMetaDataItem(
                            (string)reader[Configuration.COL_SOURCE_ID],
                            this.RootPath + (string)reader[Configuration.COL_RELATIVE_PATH], (string)reader[Configuration.COL_RELATIVE_PATH],
                            (string)reader[Configuration.COL_HASH_CODE], (DateTime)reader[Configuration.COL_LAST_MODIFIED_TIME],
                            Convert.ToUInt32(reader[Configuration.COL_NTFS_ID1]), Convert.ToUInt32(reader[Configuration.COL_NTFS_ID2])));
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
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    foreach (FileMetaDataItem item in mData)
                    {
                        string cmdText = "INSERT INTO " + Configuration.TBL_METADATA +
                                     "( " + Configuration.COL_SOURCE_ID + "," + Configuration.COL_RELATIVE_PATH + "," +
                                     Configuration.COL_HASH_CODE + "," +
                                     Configuration.COL_LAST_MODIFIED_TIME + "," +
                                     Configuration.COL_NTFS_ID1 + "," +
                                     Configuration.COL_NTFS_ID2 + ")" +
                                     "VALUES (@source_id , @relative_path, @hash_code, @last_modified_time, @ntfs_id1, @ntfs_id2) ";

                        SqliteParameterCollection paramList = new SqliteParameterCollection();
                        paramList.Add(new SqliteParameter("@source_id", DbType.String) { Value = item.SourceId });
                        paramList.Add(new SqliteParameter("@relative_path", DbType.String) { Value = item.RelativePath });
                        paramList.Add(new SqliteParameter("@hash_code", DbType.String) { Value = item.HashCode });
                        paramList.Add(new SqliteParameter("@last_modified_time", DbType.DateTime) { Value = item.LastModifiedTime });
                        paramList.Add(new SqliteParameter("@ntfs_id1", DbType.Int32) { Value = item.NTFS_ID1 });
                        paramList.Add(new SqliteParameter("@ntfs_id2", DbType.Int32) { Value = item.NTFS_ID2 });

                        db.ExecuteNonQuery(cmdText, paramList);
                    }
                    trasaction.Commit();
                }
                catch (Exception)
                {
                    trasaction.Rollback();
                    // Log error??
                    return false;
                }               
            }

            return true;
        } 


        public override bool Delete(IList<FileMetaDataItem> items)
        {
            // All deletions are atomic

            SQLiteAccess dbAccess = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));

            using (SqliteConnection con =  dbAccess.NewSQLiteConnection ())
            {
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    foreach (FileMetaDataItem item in items)
                    {
                        string cmdText = "DELETE FROM " + Configuration.TBL_METADATA +
                                          " WHERE " + Configuration.COL_SOURCE_ID + " = @sourceId AND " +
                                          Configuration.COL_RELATIVE_PATH + " = @path";

                        SqliteParameterCollection paramList = new SqliteParameterCollection();
                        paramList.Add(new SqliteParameter("@sourceId", DbType.String) { Value = item.SourceId });
                        paramList.Add(new SqliteParameter("@path", DbType.String) { Value = item.RelativePath });
                        dbAccess.ExecuteNonQuery(cmdText, paramList);
                    }
                    transaction.Commit();                    
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    // Log error??
                    return false;
                }              
            }

            return true;
        }


        public override bool Update(IList<FileMetaDataItem> items)
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    foreach (FileMetaDataItem item in items)
                    {
                        string cmdText = "UPDATE " + Configuration.TBL_METADATA +
                                         " SET " + Configuration.COL_HASH_CODE + " = @hash, " +
                                         Configuration.COL_LAST_MODIFIED_TIME + " = @lmf" +
                                         " WHERE " + Configuration.COL_RELATIVE_PATH + " = @rel AND " +
                                         Configuration.COL_SOURCE_ID + " = @sourceId";

                        SqliteParameterCollection paramList = new SqliteParameterCollection();
                        paramList.Add(new SqliteParameter("@hash", DbType.String) { Value = item.HashCode });
                        paramList.Add(new SqliteParameter("@lmf", DbType.DateTime) { Value = item.LastModifiedTime });
                        paramList.Add(new SqliteParameter("@rel", DbType.String) { Value = item.RelativePath });
                        paramList.Add(new SqliteParameter("@sourceId", DbType.String) { Value = item.SourceId });

                        db.ExecuteNonQuery(cmdText, false);
                    }
                    trasaction.Commit();
                }
                catch (Exception)
                {
                    trasaction.Rollback();
                    return false;
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

            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                // TODO: make 3 actions atomic
                try
                {
                    this.Add(newOnly.ToList());
                    this.Delete(oldOnly.ToList());
                    this.Update(bothModified.ToList());
                    trasaction.Commit();
                }
                catch (Exception)
                {
                    trasaction.Rollback();
                    throw;
                }               
            }

            return true;
        }


        public override void CreateSchema()
        {
            SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME));
            using (SqliteConnection con = db.NewSQLiteConnection ())
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + Configuration.TBL_METADATA +
                                                " ( " + Configuration.COL_SOURCE_ID + " TEXT, " +
                                                Configuration.COL_RELATIVE_PATH + " TEXT, " +
                                                Configuration.COL_HASH_CODE + " TEXT, " +
                                                Configuration.COL_LAST_MODIFIED_TIME + " DATETIME, " +
                                                Configuration.COL_NTFS_ID1 + " INT, " +
                                                Configuration.COL_NTFS_ID2 + " INT," +
                                                "FOREIGN KEY (" + Configuration.COL_SOURCE_ID + ") REFERENCES " + Configuration.TBL_DATASOURCE_INFO + "(" + Configuration.COL_SOURCE_ID + ")" +
                                                "PRIMARY KEY (" + Configuration.COL_SOURCE_ID + "," + Configuration.COL_RELATIVE_PATH + ")" +
                                                ")";

                db.ExecuteNonQuery(cmdText, false);
            }
        }

        public void CreateSchema(SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + Configuration.TBL_METADATA +
                                                " ( " + Configuration.COL_SOURCE_ID + " TEXT, " +
                                                Configuration.COL_RELATIVE_PATH + " TEXT, " +
                                                Configuration.COL_HASH_CODE + " TEXT, " +
                                                Configuration.COL_LAST_MODIFIED_TIME + " DATETIME, " +
                                                Configuration.COL_NTFS_ID1 + " INT, " +
                                                Configuration.COL_NTFS_ID2 + " INT," +
                                                "FOREIGN KEY (" + Configuration.COL_SOURCE_ID + ") REFERENCES " + Configuration.TBL_DATASOURCE_INFO + "(" + Configuration.COL_SOURCE_ID + ")" +
                                                "PRIMARY KEY (" + Configuration.COL_SOURCE_ID + "," + Configuration.COL_RELATIVE_PATH + ")" +
                                                ")";
                cmd.ExecuteNonQuery();
            }
        }

    }
}
