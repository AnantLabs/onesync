using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;

namespace OneSync.Synchronization
{
    public class SQLiteActionProvider:BaseSQLiteProvider, IActionsProvider
    {
        private string metadataStore = "";
        private Profile profile = null;
        /// <summary>
        /// Constructor takes a profile object as parameter
        /// Often used when load the actions
        /// </summary>
        /// <param name="profile"></param>
        public SQLiteActionProvider(Profile profile):base(profile.MetaDataSource.Path)
        {
            this.profile = profile;
        }

        /// <summary>
        /// Take in a string as parameter
        /// Often used when create actions/schema
        /// </summary>
        /// <param name="metaDataStore"></param>
        public SQLiteActionProvider(string metaDataStore):base(metaDataStore)
        {
            this.metadataStore = metaDataStore;            
        }

        public string MetaDataStore
        {            
            get
            {
                return this.metadataStore;
            }
        }

        public void CreateSchema(SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {
                        cmd.CommandText = "DROP TABLE IF EXISTS " + SyncAction.TABLE_NAME;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + SyncAction.TABLE_NAME +
                                                    " ( " + SyncAction.CHANGE_IN + " TEXT, " +
                                                    SyncAction.ACTION + " INT, " +
                                                    SyncAction.OLD + " TEXT, " +
                                                    SyncAction.NEW + " TEXT, " +
                                                    SyncAction.NEW_HASH + " TEXT, " +
                                                    SyncAction.OLD_HASH + " TEXT)"
                                                    ;
                        cmd.ExecuteNonQuery();          
                }       
        }

        #region IActionsProvider Members

        public IList<SyncAction> Load(SyncSource source)
        {
            IList<SyncAction> actions = new List<SyncAction>();
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {                
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM " + SyncAction.TABLE_NAME + " WHERE " + SyncAction.CHANGE_IN + " <> @sourceId";
                    SqliteParameter param1 = new SqliteParameter("@sourceId",  System.Data.DbType.String);
                    param1.Value = source.ID;
                    cmd.Parameters.Add(param1);
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int actionType = (int) reader[SyncAction.ACTION];
                            switch (actionType)
                            {
                                case (int)ChangeType.DELETED:
                                    DeleteAction delAction = new DeleteAction(
                                        source.Path,
                                        (string)reader[SyncAction.CHANGE_IN],
                                        (string)reader[SyncAction.OLD], (string)reader[SyncAction.OLD_HASH]);
                                    actions.Add(delAction);
                                    break;
                                case (int)ChangeType.MODIFIED:
                                    ModifyAction modAction = new ModifyAction(
                                        source.Path,
                                        (string)reader[SyncAction.CHANGE_IN], (ChangeType)((int)reader[SyncAction.ACTION]),
                                        (string)reader[SyncAction.OLD], (string)reader[SyncAction.OLD_HASH], (string)reader[SyncAction.NEW_HASH]);
                                    actions.Add(modAction);
                                    actions.Add(modAction);
                                    break;
                                case (int)ChangeType.NEWLY_CREATED:
                                    CreateAction createAction = new CreateAction(
                                        source.Path,
                                        (string)reader[SyncAction.CHANGE_IN],
                                        (string)reader[SyncAction.NEW], (string)reader[SyncAction.NEW_HASH]);
                                    actions.Add(createAction);
                                    break;
                                case (int)ChangeType.RENAMED:
                                    RenameAction renameAction = new RenameAction(
                                        source.Path,
                                        (string)reader[SyncAction.CHANGE_IN],
                                        (string)reader[SyncAction.OLD], (string)reader[SyncAction.NEW], (string)reader[SyncAction.OLD_HASH]);
                                    actions.Add(renameAction);
                                    break;
                            }                           
                        }
                    }
                }
            }
            return actions ;
        }


        public void Insert(IList<SyncAction> actions)
        {
            using (SqliteConnection con = new SqliteConnection (ConnectionString))
            {
                con.Open();
                SqliteTransaction transaction = (SqliteTransaction)con.BeginTransaction();
                try
                {
                    foreach (SyncAction action in actions)
                    {
                        switch (action.ChangeType)
                        {
                            case ChangeType.DELETED:
                                break;
                            case ChangeType.MODIFIED:
                                InsertModifyAction((ModifyAction)action,con);
                                break;
                            case ChangeType.NEWLY_CREATED:
                                InsertCreateAction((CreateAction) action,con);
                                break;
                            case ChangeType.RENAMED:
                                break;
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new DatabaseException("");
                }                
            }
        }

        public void Insert(SyncAction action)
        {
            using (SqliteConnection con = new SqliteConnection (ConnectionString))
            {
                con.Open();

            }
        }

        public void Update(SyncAction action)
        {
            
        }

        public void Delete(SyncAction action)
        {

        }

        #endregion


        private void InsertCreateAction(CreateAction createAction, SqliteConnection con)
        {           
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO " + SyncAction.TABLE_NAME +
                                                " ( " + SyncAction.CHANGE_IN + "," +
                                                SyncAction.ACTION  + "," +                                               
                                                SyncAction.NEW + "," +
                                                SyncAction.NEW_HASH + ") VALUES (@changeIn, @action, @newPath, @newHash)"                                            
                                                ;
                    SqliteParameter param1 = new SqliteParameter("@changeIn", System.Data.DbType.String);
                    param1.Value = createAction.SourceID;
                    cmd.Parameters.Add(param1);

                    SqliteParameter param2 = new SqliteParameter("@action", System.Data.DbType.Int32);
                    param2.Value =(int) createAction.ChangeType;
                    cmd.Parameters.Add(param2);

                    SqliteParameter param3 = new SqliteParameter("@newPath", System.Data.DbType.String);
                    param3.Value = createAction.RelativeFilePath;
                    cmd.Parameters.Add(param3);

                    SqliteParameter param4 = new SqliteParameter("@newHash", System.Data.DbType.String);
                    param4.Value = createAction.FileHash;
                    cmd.Parameters.Add(param4);                   

                    cmd.ExecuteNonQuery();
                }
            
        }

        private void InsertDeleteAction(DeleteAction deleteAction, SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {                
                cmd.CommandText = "INSERT INTO " + SyncAction.TABLE_NAME +
                                            " ( " + SyncAction.CHANGE_IN + "," +
                                            SyncAction.ACTION + "," +
                                            SyncAction.OLD + "," +
                                            SyncAction.OLD_HASH + ") VALUES (@changeIn, @action, @oldPath, @oldHash)"
                                            ;
                SqliteParameter param1 = new SqliteParameter("@changeIn", System.Data.DbType.String);
                param1.Value = deleteAction.SourceID;
                cmd.Parameters.Add(param1);

                SqliteParameter param2 = new SqliteParameter("@action", System.Data.DbType.Int32);
                param2.Value = (int)deleteAction.ChangeType;
                cmd.Parameters.Add(param2);

                SqliteParameter param3 = new SqliteParameter("@oldPath", System.Data.DbType.String);
                param3.Value = deleteAction.FileHash;
                cmd.Parameters.Add(param3);

                SqliteParameter param4 = new SqliteParameter("@oldHash", System.Data.DbType.String);
                param4.Value = deleteAction.FileHash;
                cmd.Parameters.Add(param4);

                cmd.ExecuteNonQuery();
            }

        }

        /*
        private void InsertModifyAction(ModifyAction modifyAction, SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {                
                cmd.CommandText = "INSERT INTO " + SyncAction.TABLE_NAME +
                                            " ( " + SyncAction.CHANGE_IN + "," +
                                            SyncAction.ACTION + "," +
                                            SyncAction.OLD + "," +
                                            SyncAction.OLD_HASH + "," +
                                            SyncAction.NEW_HASH + ") VALUES (@changeIn, @action, @old, @old_hash, @new_hash)"
                                            ;
                SqliteParameter param1 = new SqliteParameter("@changeIn", System.Data.DbType.String);
                param1.Value = modifyAction.SourceID;
                cmd.Parameters.Add(param1);

                SqliteParameter param2 = new SqliteParameter("@action", System.Data.DbType.Int32);
                param2.Value = (int)modifyAction.ChangeType;
                cmd.Parameters.Add(param2);

                SqliteParameter param3 = new SqliteParameter("@old", System.Data.DbType.String);
                param3.Value = modifyAction.RelativeFilePath;
                cmd.Parameters.Add(param3);

                SqliteParameter param4 = new SqliteParameter("@old_hash", System.Data.DbType.String);
                param4.Value = modifyAction.OldItemHash;
                cmd.Parameters.Add(param4);

                SqliteParameter param5 = new SqliteParameter("@new_hash", System.Data.DbType.String);
                param4.Value = modifyAction.NewItemHash;
                cmd.Parameters.Add(param5);

                cmd.ExecuteNonQuery();
            }

        }*/

    }
}
