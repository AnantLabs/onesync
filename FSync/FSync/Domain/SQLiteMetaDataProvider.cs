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
    public enum SourceOption
    {
        NOT_EQUAL_SOURCE_ID,
        EQUAL_SOURCE_ID
    }

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
        /// Create metadata table and insert file metadata into the table
        /// </summary>
        /// <param name="mData"></param>

        public void Insert(FileMetaData mData)
        {
            FileMetaData fileMetaData = (FileMetaData)mData;              
            
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {                
                con.Open();
                SqliteTransaction transaction =  (SqliteTransaction)con.BeginTransaction();
                try
                {
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
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("Database error");
                }
             }
            
                
           Console.WriteLine("Insert done");          
        }

        /// <summary>
        /// Create metadata table and insert file metadata into the table
        /// </summary>
        /// <param name="mData"></param>
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
                    SqliteParameter param0 = new SqliteParameter("@source_id", DbType.String);
                    param0.Value = item.SourceId;
                    cmd.Parameters.Add(param0);

                    SqliteParameter param1 = new SqliteParameter("@relative_path", DbType.String);
                    param1.Value = item.RelativePath;
                    cmd.Parameters.Add(param1);

                    SqliteParameter param2 = new SqliteParameter("@hash_code", DbType.String);
                    param2.Value = item.HashCode;
                    cmd.Parameters.Add(param2);

                    SqliteParameter param3 = new SqliteParameter("@last_modified_time", DbType.DateTime);
                    param3.Value = item.LastModifiedTime;
                    cmd.Parameters.Add(param3);

                    SqliteParameter param4 = new SqliteParameter("@ntfs_id1", DbType.Int32);
                    param4.Value = (item.NTFS_ID1 + 1);
                    cmd.Parameters.Add(param4);

                    SqliteParameter param5 = new SqliteParameter("@ntfs_id2", DbType.Int32);
                    param5.Value = (item.NTFS_ID2 + 2);
                    cmd.Parameters.Add(param5);

                    cmd.ExecuteNonQuery();
                }
             }
                
           Console.WriteLine("Insert done");            
        }      
        # endregion Insert Data To Database                 
        

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


        public void Update(FileMetaData mdData)
        {
 	        

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
                    SqliteParameter param1 = new SqliteParameter("@sourceId", DbType.String);
                    param1.Value = source.ID ;
                    cmd.Parameters.Add(param1);
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

        public int GetNumberOfSyncSources()
        {
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT (DISTINCT " + SyncSource.SOURCE_ID+ ") AS num"+
                        " FROM " + SyncSource.DATASOURCE_INFO_TABLE;
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return  Convert.ToInt32( (Int64) reader[0]);
                        }
                    }
                }
            }
            return -1;
        }

    }
}
