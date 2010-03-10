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
    

    public class SQLiteMetaDataProvider : BaseSQLiteProvider, IMetaDataProvider 
    {       
        private Profile profile = null;

        /// <summary>
        /// Constructor take in Profile as parameter
        /// Profile used to get the base path of the sync source
        /// </summary>
        /// <param name="p"></param>
        public SQLiteMetaDataProvider(Profile p):base(p.IntermediaryStorage.Path)            
        {
            this.profile = p;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SQLiteMetaDataProvider() : base() { }

        /// <summary>
        /// Constructor takes in path which is path to metadata storage
        /// Used to create new table when profile is unknown
        /// </summary>
        /// <param name="path"></param>
        public SQLiteMetaDataProvider(string path)
            : base(path)
        {            
        }

        # region Create Data Storage
        /// <summary>
        /// Create metadata storage
        /// </summary>
       

        /// <summary>
        /// Create tables to store meta data 
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="dbName"></param>
        public void CreateSchema (SqliteConnection con)
        {       
            using (SqliteCommand cmd = con.CreateCommand())
            {                  
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + FileMetaData.METADATA_TABLE +
                                                " ( " + FileMetaData.SOURCE_ID + " TEXT, " +
                                                FileMetaData.RELATIVE_PATH + " TEXT, " +
                                                FileMetaData.HASH_CODE + " TEXT, " +
                                                FileMetaData.LAST_MODIFIED_TIME + " DATETIME, " +
                                                FileMetaData.NTFS_ID1 + " INT, " +
                                                FileMetaData.NTFS_ID2 + " INT," +
                                                "FOREIGN KEY (" + FileMetaData.SOURCE_ID + ") REFERENCES " + SyncSource.DATASOURCE_INFO_TABLE + "(" + SyncSource.SOURCE_ID +")" +
                                                "PRIMARY KEY (" + FileMetaData.SOURCE_ID + "," + FileMetaData.RELATIVE_PATH + ")"+
                                                ")";
                cmd.ExecuteNonQuery();                                    
            }
        }
       
      
        /// <summary>
        /// Insert metadata into metadata folder 
        /// </summary>
        /// <param name="mData"></param>
        /// <param name="con"></param>
        public void Insert(FileMetaData mData, SqliteConnection con)
        {
            FileMetaData fileMetaData = (FileMetaData)mData;              
            
            foreach (FileMetaDataItem item in mData.MetaDataItems)
            {
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO " + FileMetaData.METADATA_TABLE +
                        "( " + FileMetaData.SOURCE_ID + "," + FileMetaData.RELATIVE_PATH + "," +
                        FileMetaData.HASH_CODE + "," +
                        FileMetaData.LAST_MODIFIED_TIME + "," +
                        FileMetaData.NTFS_ID1 + "," +
                        FileMetaData.NTFS_ID2 + ")" +
                        "VALUES (@source_id , @relative_path, @hash_code, @last_modified_time, @ntfs_id1, @ntfs_id2) ";

                    cmd.Parameters.Add(new SqliteParameter("@source_id", DbType.String) { Value= item.SourceId});
                    cmd.Parameters.Add(new SqliteParameter("@relative_path", DbType.String) { Value = item.RelativePath });
                    cmd.Parameters.Add(new SqliteParameter("@hash_code", DbType.String) { Value = item.HashCode});
                    cmd.Parameters.Add(new SqliteParameter("@last_modified_time", DbType.DateTime) { Value = item.LastModifiedTime });
                    cmd.Parameters.Add(new SqliteParameter("@ntfs_id1", DbType.Int32) { Value = item.NTFS_ID1 });
                    cmd.Parameters.Add(new SqliteParameter("@ntfs_id2", DbType.Int32) { Value = item.NTFS_ID2});                 
                            
                    cmd.ExecuteNonQuery();                        
                }                        
            }        
        }     
       
        /// <summary>
        /// Delete items with specific source id and relative paths
        /// </summary>
        /// <param name="items"></param>
        /// <param name="con"></param>
        public void Delete (IList<FileMetaDataItem> items, SqliteConnection con)
        {
            foreach (FileMetaDataItem item in items)
            {
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM " + FileMetaData.METADATA_TABLE +
                        " WHERE " + FileMetaData.SOURCE_ID + " = @sourceId AND " +
                        FileMetaData.RELATIVE_PATH + " = @path";
                    cmd.Parameters.Add(new SqliteParameter("@sourceId", DbType.String) { Value = item.SourceId });
                    cmd.Parameters.Add(new SqliteParameter("@path", DbType.String) { Value = item.RelativePath });
                    cmd.ExecuteNonQuery();                   
                }
            }
        }


        /// <summary>
        /// Create metadata table and insert file metadata into the table
        /// </summary>
        /// <param name="mData"></param>
        public  void Insert(IList<FileMetaDataItem> mData, SqliteConnection con)
        {            
            foreach (FileMetaDataItem item in mData)
            {
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO " + FileMetaData.METADATA_TABLE +
                                              "( " + FileMetaData.SOURCE_ID + "," + FileMetaData.RELATIVE_PATH + "," +
                                              FileMetaData.HASH_CODE + "," +
                                              FileMetaData.LAST_MODIFIED_TIME + "," +
                                              FileMetaData.NTFS_ID1 + "," +
                                              FileMetaData.NTFS_ID2 + ")" +
                                              "VALUES (@source_id , @relative_path, @hash_code, @last_modified_time, @ntfs_id1, @ntfs_id2) ";
                    cmd.Parameters.Add(new SqliteParameter("@source_id", DbType.String) { Value = item.SourceId});
                    cmd.Parameters.Add(new SqliteParameter("@relative_path", DbType.String) { Value = item.RelativePath});
                    cmd.Parameters.Add(new SqliteParameter("@hash_code", DbType.String) { Value =  item.HashCode });
                    cmd.Parameters.Add(new SqliteParameter("@last_modified_time", DbType.DateTime) { Value = item.LastModifiedTime });

                    //NOTE: not yet implement the NTFS_ID
                    //temporarily use fake id
                    cmd.Parameters.Add(new SqliteParameter("@ntfs_id1", DbType.Int32) { Value = item.NTFS_ID1 + 1});
                    cmd.Parameters.Add(new SqliteParameter("@ntfs_id2", DbType.Int32) { Value = item.NTFS_ID2 + 2});
                    cmd.ExecuteNonQuery();
                }
             }
                
           Console.WriteLine("Insert done");            
        }      
        # endregion Insert Data To Database                 
        

        /// <summary>
        /// Update a list of metadata items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="con"></param>
        public void Update(IList<FileMetaDataItem> items, SqliteConnection con)
        {
            foreach (FileMetaDataItem item in items)
            {
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE " + FileMetaData.METADATA_TABLE +
                        " SET " + FileMetaData.HASH_CODE + " = @hash, " +
                        FileMetaData.LAST_MODIFIED_TIME + " = @lmf" +
                        " WHERE " + FileMetaData.RELATIVE_PATH + " = @rel AND " +
                        FileMetaData.SOURCE_ID + " = @sourceId";
                    cmd.Parameters.Add(new SqliteParameter("@hash", DbType.String) { Value = item.HashCode });
                    cmd.Parameters.Add(new SqliteParameter("@lmf", DbType.DateTime) { Value = item.LastModifiedTime });
                    cmd.Parameters.Add(new SqliteParameter("@rel", DbType.String) { Value = item.RelativePath });
                    cmd.Parameters.Add(new SqliteParameter("@sourceId", DbType.String) { Value = item.SourceId });
                    cmd.ExecuteNonQuery();
                }
            }
        }


        
        /// <summary>
        /// Generate a metadata from local folder
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public FileMetaData FromPath(SyncSource source)
        {
            FileMetaData metaData = new FileMetaData(source.ID, source.Path);
            string[] files = Directory.GetFiles(source.Path, "*.*", SearchOption.AllDirectories);

            for (int x = 0; x < files.Length; x++)
            {
                FileInfo fileInfo = new FileInfo(files[x]);
                metaData.MetaDataItems.Add(new FileMetaDataItem(source.ID, fileInfo.FullName,
                    OneSync.Files.FileUtils.GetRelativePath(source.Path, fileInfo.FullName), Files.FileUtils.GetFileHash(fileInfo.FullName),
                    fileInfo.LastWriteTime, (uint)x, (uint)x));
            }

            return metaData;
        }

        /// <summary>
        /// Load metadata given a source id        
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>

        public FileMetaData Load(SyncSource source, SourceOption option)

        {
            string opt = "";
            if (option == SourceOption.EQUAL_SOURCE_ID) { opt = " = "; }
            else { opt = " <> "; }

            FileMetaData mData = new FileMetaData(source.ID, source.Path);            
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {                
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM " + FileMetaData.METADATA_TABLE + " WHERE " + FileMetaData.SOURCE_ID + opt + " @sourceId";
                    cmd.Parameters.Add(new SqliteParameter("@sourceId", DbType.String) { Value = source.ID });                    
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {                        
                                mData.MetaDataItems.Add (new FileMetaDataItem(
                                (string)reader[FileMetaData.SOURCE_ID],
                                profile.SyncSource.Path + (string)reader[FileMetaData.RELATIVE_PATH], (string)reader[FileMetaData.RELATIVE_PATH],
                                (string)reader[FileMetaData.HASH_CODE], (DateTime) reader[FileMetaData.LAST_MODIFIED_TIME],
                                Convert.ToUInt32(reader[FileMetaData.NTFS_ID1]), Convert.ToUInt32(reader[FileMetaData.NTFS_ID2])));                                
                        }
                    }
                }
            }
            return  mData;
        }

        public void Update(Profile profile, FileMetaData oldMetadata, FileMetaData newMetada)
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
                                                         where ((FileMetaDataItem)_new).RelativePath.Equals(((FileMetaDataItem)old).RelativePath)
                                                         && !((FileMetaDataItem)_new).HashCode.Equals(((FileMetaDataItem)old).HashCode)
                                                         select _new;

            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    SQLiteMetaDataProvider provider = new SQLiteMetaDataProvider();
                    provider.Insert(newOnly.ToList(), con);
                    provider.Delete(oldOnly.ToList(), con);
                    provider.Update(bothModified.ToList(), con);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }
    }
}
