﻿/*
 $Id: SQLiteActionProvider.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System.Collections.Generic;
using System.Data;
using System.IO;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    public class SQLiteSyncActionsProvider : SyncActionsProvider
    {

        public SQLiteSyncActionsProvider(string storagePath)
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

        public override IList<SyncAction> Load(string sourceID, bool loadOther)
        {
            string opt = (loadOther) ? " <> " : " = ";
            IList<SyncAction> actions = new List<SyncAction>();

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "SELECT * FROM " + Configuration.TBL_ACTION + 
                                 " WHERE " + Configuration.COL_CHANGE_IN + opt + " @sourceId";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@sourceId", System.Data.DbType.String) { Value = sourceID });

                db.ExecuteReader(cmdText, paramList, reader =>
                    {
                        ChangeType actionType = (ChangeType)reader[Configuration.COL_ACTION_TYPE];

                        if (actionType == ChangeType.DELETED)
                        {
                            DeleteAction delAction = new DeleteAction(
                                    (int)reader[Configuration.COL_ACTION_ID],
                                    (string)reader[Configuration.COL_CHANGE_IN],
                                    (string)reader[Configuration.COL_OLD_RELATIVE_PATH], (string)reader[Configuration.COL_OLD_HASH]);
                            actions.Add(delAction);
                        }
                        else if (actionType == ChangeType.NEWLY_CREATED)
                        {
                            CreateAction createAction = new CreateAction(
                                (int)reader[Configuration.COL_ACTION_ID],
                                (string)reader[Configuration.COL_CHANGE_IN],
                                (string)reader[Configuration.COL_NEW_RELATIVE_PATH], (string)reader[Configuration.COL_NEW_HASH]);
                            actions.Add(createAction);
                        }
                        else if (actionType == ChangeType.RENAMED)
                        {
                            RenameAction renameAction = new RenameAction(
                                (int)reader[Configuration.COL_ACTION_ID],
                                (string)reader[Configuration.COL_CHANGE_IN],
                                (string)reader[Configuration.COL_NEW_RELATIVE_PATH],
                                (string)reader[Configuration.COL_OLD_RELATIVE_PATH],
                                (string)reader[Configuration.COL_OLD_HASH]);
                            actions.Add(renameAction);
                        }
                    }
                );

            }
            return actions;
        }

        public override bool Add(SyncAction action)
        {
            switch (action.ChangeType)
            {
                case ChangeType.DELETED:
                    return InsertDeleteAction((DeleteAction)action);
                case ChangeType.NEWLY_CREATED:
                    return InsertCreateAction((CreateAction)action);
                case ChangeType.RENAMED:
                    return InsertRenameAction((RenameAction)action);
                default:
                    // Log error? Throw ex?
                    return false;
            }
        }

        public override bool Add(IList<SyncAction> actions)
        {
            foreach (SyncAction action in actions)
            {
                switch (action.ChangeType)
                {
                    case ChangeType.DELETED:
                        InsertDeleteAction((DeleteAction)action);
                        break;
                    case ChangeType.NEWLY_CREATED:
                        InsertCreateAction((CreateAction)action);
                        break;
                    case ChangeType.RENAMED:
                        InsertRenameAction((RenameAction)action);
                        break;
                }
            }

            return true;
        }

        public override bool Delete(SyncAction action)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "DELETE FROM " + Configuration.TBL_ACTION + 
                                 " WHERE " + Configuration.COL_ACTION_ID + " = @id";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", DbType.Int32) { Value = action.ActionId });

                db.ExecuteNonQuery(cmdText, paramList, false);
            }

            return true;
        }

        public override bool Delete(IList<SyncAction> actions)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "DELETE FROM " + Configuration.TBL_ACTION +
                                 " WHERE " + Configuration.COL_ACTION_ID + " = @id";

                SqliteParameterCollection paramList = new SqliteParameterCollection();

                foreach (SyncAction action in actions)
                {
                    paramList.Clear();
                    paramList.Add(new SqliteParameter("@id", DbType.Int32) { Value = action.ActionId });
                    db.ExecuteNonQuery(cmdText, paramList, false);
                }
            }
            return true;
        }

        public override bool Delete(string sourceID, bool deleteOther)
        {
            string opt = (deleteOther) ? " <> " : " = ";

            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "DELETE FROM " + Configuration.TBL_ACTION + 
                                 " WHERE " + Configuration.COL_CHANGE_IN + " " + opt + " @id";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@id", System.Data.DbType.Int32) { Value = sourceID });

                db.ExecuteNonQuery(cmdText, paramList, false);
            }

            return true;
        }

        private void CreateSchema()
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "CREATE TABLE IF NOT EXISTS " + Configuration.TBL_ACTION +
                                 " ( " + Configuration.COL_ACTION_ID + " INTEGER PRIMARY KEY AUTOINCREMENT," +
                                 Configuration.COL_CHANGE_IN + " TEXT, " +
                                 Configuration.COL_ACTION_TYPE + " INT, " +
                                 Configuration.COL_OLD_RELATIVE_PATH + " TEXT, " +
                                 Configuration.COL_NEW_RELATIVE_PATH + " TEXT, " +
                                 Configuration.COL_NEW_HASH + " TEXT, " +
                                 Configuration.COL_OLD_HASH + " TEXT)";

                db.ExecuteNonQuery(cmdText, null, false);
            }

        }

        private bool InsertRenameAction(RenameAction renameAction)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "INSERT INTO " + Configuration.TBL_ACTION +
                                " ( " + Configuration.COL_CHANGE_IN + "," +
                                Configuration.COL_ACTION_TYPE + "," +
                                Configuration.COL_OLD_RELATIVE_PATH + "," +
                                Configuration.COL_NEW_RELATIVE_PATH + "," +
                                Configuration.COL_OLD_HASH + ") VALUES (@changeIn, @action, @oldPath, @newPath, @oldHash)";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@changeIn", DbType.String) { Value = renameAction.SourceID });
                paramList.Add(new SqliteParameter("@action", DbType.Int32) { Value = renameAction.ChangeType });
                paramList.Add(new SqliteParameter("@oldPath", DbType.String) { Value = renameAction.PreviousRelativeFilePath });
                paramList.Add(new SqliteParameter("@newPath", DbType.String) { Value = renameAction.RelativeFilePath });
                paramList.Add(new SqliteParameter("@oldHash", DbType.String) { Value = renameAction.FileHash });

                db.ExecuteNonQuery(cmdText, paramList, false);
            }

            return true;
        }

        private bool InsertCreateAction(CreateAction createAction)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "INSERT INTO " + Configuration.TBL_ACTION +
                                 " ( " + Configuration.COL_CHANGE_IN + "," +
                                 Configuration.COL_ACTION_TYPE + "," +
                                 Configuration.COL_NEW_RELATIVE_PATH + "," +
                                 Configuration.COL_NEW_HASH + ") VALUES (@changeIn, @action, @newPath, @newHash)";

                SqliteParameterCollection paramList = new SqliteParameterCollection();
                paramList.Add(new SqliteParameter("@changeIn", System.Data.DbType.String) { Value = createAction.SourceID });
                paramList.Add(new SqliteParameter("@action", System.Data.DbType.Int32) { Value = createAction.ChangeType });
                paramList.Add(new SqliteParameter("@newPath", System.Data.DbType.String) { Value = createAction.RelativeFilePath });
                paramList.Add(new SqliteParameter("@newHash", System.Data.DbType.String) { Value = createAction.FileHash });

                db.ExecuteNonQuery(cmdText, paramList, false);
            }

            return true;
        }

        private bool InsertDeleteAction(DeleteAction deleteAction)
        {
            using (SQLiteAccess db = new SQLiteAccess(Path.Combine(this.StoragePath, Configuration.DATABASE_NAME)))
            {
                string cmdText = "INSERT INTO " + Configuration.TBL_ACTION +
                                            " ( " + Configuration.COL_CHANGE_IN + "," +
                                            Configuration.COL_ACTION_TYPE + "," +
                                            Configuration.COL_OLD_RELATIVE_PATH + "," +
                                            Configuration.COL_OLD_HASH + ") VALUES (@changeIn, @action, @oldPath, @oldHash)";
                
                SqliteParameterCollection paramList = new SqliteParameterCollection();;
                paramList.Add(new SqliteParameter("@changeIn", DbType.String) { Value = deleteAction.SourceID });
                paramList.Add(new SqliteParameter("@action", DbType.Int32) { Value = (int)deleteAction.ChangeType });
                paramList.Add(new SqliteParameter("@oldPath", DbType.String) { Value = deleteAction.RelativeFilePath });
                paramList.Add(new SqliteParameter("@oldHash", DbType.String) { Value = deleteAction.FileHash });

                db.ExecuteNonQuery(cmdText, paramList, false);
            }

            return true;
        }
    }
}
