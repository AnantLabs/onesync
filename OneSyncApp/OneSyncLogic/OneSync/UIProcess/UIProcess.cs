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
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection())
            {
                con.Open();
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Delete(action, con);
                    trasaction.Commit();
                }
                catch (Exception ex)
                {
                    trasaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
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
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteMetaDataProvider(profile).Insert(md.MetaDataItems, con);
                    trasaction.Commit();
                }
                catch (Exception ex)
                {
                    trasaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }

           

        public static void InsertMetaData(IList<FileMetaDataItem> item, Profile profile)
        {
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteMetaDataProvider(profile).Insert(item, con);
                    trasaction.Commit();
                }
                catch (Exception ex)
                {
                    trasaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }

        

        #endregion Metadata

        
    }
}
