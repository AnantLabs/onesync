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
        public SQLiteActionProvider(Profile profile):base(profile.IntermediaryStorage.Path)
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
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + SyncAction.ACTION_TABLE +
                                                    " ( " + SyncAction.ACTION_ID + " INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                    SyncAction.CHANGE_IN + " TEXT, " +
                                                    SyncAction.ACTION_TYPE + " INT, " +
                                                    SyncAction.OLD_RELATIVE_PATH + " TEXT, " +
                                                    SyncAction.NEW_RELATIVE_PATH + " TEXT, " +
                                                    SyncAction.NEW_HASH + " TEXT, " +
                                                    SyncAction.OLD_HASH + " TEXT)"
                                                    ;
                        cmd.ExecuteNonQuery();          
                }       
        }

        #region IActionsProvider Members

        public IList<SyncAction> Load(SyncSource source, SourceOption option)
        {
            string opt = (option == SourceOption.EQUAL_SOURCE_ID) ? " = " : " <> ";
            IList<SyncAction> actions = new List<SyncAction>();
            using (SqliteConnection con = new SqliteConnection(ConnectionString))
            {                
                con.Open();
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM " + SyncAction.ACTION_TABLE + " WHERE " + SyncAction.CHANGE_IN + opt  + " @sourceId";
                    SqliteParameter param1 = new SqliteParameter("@sourceId",  System.Data.DbType.String);
                    param1.Value = source.ID;
                    cmd.Parameters.Add(param1);
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int actionType = (int) reader[SyncAction.ACTION_TYPE];
                            switch (actionType)
                            {
                                case (int)ChangeType.DELETED:
                                    DeleteAction delAction = new DeleteAction(
                                        (int)reader[SyncAction.ACTION_ID],                                        
                                        (string)reader[SyncAction.CHANGE_IN],
                                        (string)reader[SyncAction.OLD_RELATIVE_PATH], (string)reader[SyncAction.OLD_HASH]);
                                    actions.Add(delAction);
                                    break;
                                case (int)ChangeType.NEWLY_CREATED:
                                    CreateAction createAction = new CreateAction(
                                        (int)reader[SyncAction.ACTION_ID],                                     
                                        (string)reader[SyncAction.CHANGE_IN],
                                        (string)reader[SyncAction.NEW_RELATIVE_PATH], (string)reader[SyncAction.NEW_HASH]);
                                    actions.Add(createAction);
                                    break;
                                case (int)ChangeType.RENAMED:
                                    RenameAction renameAction = new RenameAction(
                                        (int)reader[SyncAction.ACTION_ID],
                                        (string)reader [SyncAction.CHANGE_IN],
                                        (string)reader[SyncAction.NEW_RELATIVE_PATH],
                                        (string)reader[SyncAction.OLD_RELATIVE_PATH],
                                        (string)reader[SyncAction.OLD_HASH]
                                       );
                                    actions.Add(renameAction);
                                    break;
                            }                           
                        }
                    }
                }
            }
            return actions ;
        }


        public void Insert(IList<SyncAction> actions, SqliteConnection con)
        {
           foreach (SyncAction action in actions)
           {
                switch (action.ChangeType)
                {

                    case ChangeType.DELETED:
                        InsertDeleteAction((DeleteAction)action, con);
                    break;
                    case ChangeType.NEWLY_CREATED:
                        InsertCreateAction((CreateAction) action,con);
                    break;
                    case ChangeType.RENAMED:
                        
                    break;                   

                }
                    
            }           
        }

        public void Insert(SyncAction action, SqliteConnection con)
        {
          
           switch (action.ChangeType)
           {
                 case ChangeType.DELETED:
                    InsertDeleteAction((DeleteAction)action, con);
                 break;
                 case ChangeType.NEWLY_CREATED:
                    InsertCreateAction((CreateAction)action, con);
                    break;
                case ChangeType.RENAMED:
                    InsertRenameAction((RenameAction)action, con);
                    break;
           }                    
                
                
       }

        public void Delete(IList<SyncAction> actions, SqliteConnection con)
        {
            foreach (SyncAction action in actions)
            {
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM " + SyncAction.ACTION_TABLE + " WHERE " + SyncAction.ACTION_ID + " = @id";
                    cmd.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.Int32) { Value = action.ActionId});
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteBySourceId (SyncSource source, SqliteConnection con, SourceOption option)
        {
            string opt = "";
            if (option == SourceOption.EQUAL_SOURCE_ID) opt = " = ";
            else opt = " <> ";

            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM " + SyncAction.ACTION_TABLE + " WHERE " + SyncAction.CHANGE_IN + " " +opt + " @id";
                cmd.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.Int32) { Value = source.ID });
                cmd.ExecuteNonQuery();                
            }
        }

        public void Delete(SyncAction action, SqliteConnection con )
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM " + SyncAction.ACTION_TABLE + 
                            " WHERE " + SyncAction.ACTION_ID + " = @id";
                cmd.Parameters.Add(new SqliteParameter("@id",  System.Data.DbType.Int32) {Value = action.ActionId });
                cmd.ExecuteNonQuery();
             }             
        }

        #endregion


        private void InsertCreateAction(CreateAction createAction, SqliteConnection con)
        {           
                using (SqliteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO " + SyncAction.ACTION_TABLE +
                                                " ( " + SyncAction.CHANGE_IN + "," +
                                                SyncAction.ACTION_TYPE  + "," +                                               
                                                SyncAction.NEW_RELATIVE_PATH + "," +
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
                cmd.CommandText = "INSERT INTO " + SyncAction.ACTION_TABLE +
                                            " ( " + SyncAction.CHANGE_IN + "," +
                                            SyncAction.ACTION_TYPE + "," +
                                            SyncAction.OLD_RELATIVE_PATH + "," +
                                            SyncAction.OLD_HASH + ") VALUES (@changeIn, @action, @oldPath, @oldHash)"
                                            ;
                cmd.Parameters.Add(new SqliteParameter("@changeIn", System.Data.DbType.String) { Value = deleteAction.SourceID });
                cmd.Parameters.Add(new SqliteParameter("@action", System.Data.DbType.Int32) { Value = (int)deleteAction.ChangeType});
                cmd.Parameters.Add(new SqliteParameter("@oldPath", System.Data.DbType.String) { Value = deleteAction.RelativeFilePath });
                cmd.Parameters.Add(new SqliteParameter("@oldHash", System.Data.DbType.String) { Value = deleteAction.FileHash });
                cmd.ExecuteNonQuery();
            }

        }

        private void InsertRenameAction(RenameAction renameAction, SqliteConnection con)
        {
            using (SqliteCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO " + SyncAction.ACTION_TABLE +
                                            " ( " + SyncAction.CHANGE_IN + "," +
                                            SyncAction.ACTION_TYPE + "," +
                                            SyncAction.OLD_RELATIVE_PATH + "," +
                                            SyncAction.NEW_RELATIVE_PATH + "," +
                                            SyncAction.OLD_HASH + ") VALUES (@changeIn, @action, @oldPath, @newPath, @oldHash)"
                                            ;
                cmd.Parameters.Add(new SqliteParameter("@changeIn", System.Data.DbType.String) { Value = renameAction.SourceID});
                cmd.Parameters.Add(new SqliteParameter("@action", System.Data.DbType.Int32) { Value = renameAction.ChangeType});
                cmd.Parameters.Add(new SqliteParameter("@oldPath", System.Data.DbType.String) { Value = renameAction.PreviousRelativeFilePath});
                cmd.Parameters.Add(new SqliteParameter("@newPath", System.Data.DbType.String) { Value = renameAction.RelativeFilePath});
                cmd.Parameters.Add(new SqliteParameter("@oldHash", System.Data.DbType.String) { Value = renameAction.FileHash});               

                cmd.ExecuteNonQuery();
            }

        }
        
    }
}
