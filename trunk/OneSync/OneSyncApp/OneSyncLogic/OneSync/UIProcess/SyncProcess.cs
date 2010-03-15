/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using System.IO;

namespace OneSync.Synchronization
{
    public class SyncProcess
    {        
        public static void UpdateMetadata(Profile profile, FileMetaData oldMetadata, FileMetaData newMetada)
        {
            // todo: check 2nd argument is not important during update?
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(profile.IntermediaryStorage.Path, newMetada.RootDir);

            mdProvider.Update(oldMetadata, newMetada);

            /*
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
                    SQLiteMetaDataProvider provider = new SQLiteMetaDataProvider(profile);
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
            }*/
        }

    }
}
