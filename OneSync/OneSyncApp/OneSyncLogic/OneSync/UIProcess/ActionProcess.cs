using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;

namespace OneSync.Synchronization
{
    public class ActionProcess
    {
        public static void DeleteBySourceId(Profile profile, SourceOption option)
        {
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).DeleteBySourceId(profile.SyncSource, con, option);
                    trasaction.Commit();
                }
                catch (Exception ex)
                {
                    trasaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }

        public static void InsertAction(SyncAction action, Profile profile)
        {
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Insert(action, con);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }

        /// <summary>
        /// Insert a list of actions into action table
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="profile"></param>
        public static void InsertActions(IList<SyncAction> actions, Profile profile)
        {
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection(conString1))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Insert(actions, con);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }


        public static void DeleteActions(IList<SyncAction> actions, Profile profile)
        {
            string conString1 = string.Format("Version=3,uri=file:{0}", profile.IntermediaryStorage.Path + Configuration.METADATA_RELATIVE_PATH);
            using (SqliteConnection con = new SqliteConnection())
            {
                con.Open();
                SqliteTransaction trasaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    new SQLiteActionProvider(profile).Delete(actions, con);
                    trasaction.Commit();
                }
                catch (Exception ex)
                {
                    trasaction.Rollback();
                    throw new DatabaseException("Database error");
                }
            }
        }

    }
}
