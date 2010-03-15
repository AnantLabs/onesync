/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO ;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    public class UIProcess
    {
        #region Action

        public static void DeleteAction(SyncAction action, Profile profile)
        {
            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
            bool result = actProvider.Delete(action); // inform UI if result == false?

            /*
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection())
            {
                con.Open();
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteSyncActionsProvider(profile).Delete(action, con);
                    trasaction.Commit();
                }
                catch (Exception)
                {
                    trasaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
             */
        }

       
        #endregion region Action   
       
       

        #region Metadata
        /// <summary>
        /// Insert metadata into table
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="md"></param>
        public static void InsertMetaData(FileMetaData md, Profile profile)
        {
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(profile.IntermediaryStorage.Path, profile.SyncSource.Path);
            
            // TODO: if returned false, means insert failed... notify UI? or throw exception?
            mdProvider.Add(md.MetaDataItems);
        }

           

        public static void InsertMetaData(IList<FileMetaDataItem> items, Profile profile)
        {
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(profile.IntermediaryStorage.Path, profile.SyncSource.Path);

            // TODO: if returned false, means insert failed... notify UI? or throw exception?
            mdProvider.Add(items);
        }

        

        #endregion Metadata

        
    }
}
