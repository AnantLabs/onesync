/*
 $Id$
 */
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
        /// <summary>
        /// absolute path (exclude the file name) to metadata file        
        /// </summary>        
        private string metadataStoreFolder = "";

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
            this.metadataStoreFolder = metaDataStore;            
        }

        public string AbsoluteMetaDataStoreFolder
        {            
            get
            {
                return this.metadataStoreFolder;
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
                    cmd.Parameters.Add(new SqliteParameter("@sourceId", System.Data.DbType.String) { Value = source.ID });                 
                    
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

        /// <summary>
        /// Insert a list of actions into metadata table
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="con"></param>
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
                    InsertRenameAction((RenameAction)action, con);
                    break;                   
                }
                    
            }           
        }

        /// <summary>
        /// Insert a single action into action table
        /// </summary>
        /// <param name="action"></param>
        /// <param name="con"></param>
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

        /// <summary>
        /// Delete a list of actions based on action id 
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="con"></param>
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

        /// <summary>
        /// Delete action based source ID
        /// </summary>
        /// <param name="source"></param>
        /// <param name="con"></param>
        /// <param name="option">option could be either EQUAL_SOURCE_ID or NOT_EQUAL_SOURCE_ID</param>
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

        /// <summary>
        /// Delete a single action from action table based on action id
        /// </summary>
        /// <param name="action"></param>
        /// <param name="con"></param>
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

        /// <summary>
        /// Insert a create action
        /// </summary>
        /// <param name="createAction"></param>
        /// <param name="con"></param>
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
                    cmd.Parameters.Add(new SqliteParameter("@changeIn", System.Data.DbType.String) { Value = createAction.SourceID });
                    cmd.Parameters.Add(new SqliteParameter("@action", System.Data.DbType.Int32) { Value = createAction.ChangeType });
                    cmd.Parameters.Add(new SqliteParameter("@newPath", System.Data.DbType.String) { Value = createAction.RelativeFilePath});
                    cmd.Parameters.Add(new SqliteParameter("@newHash", System.Data.DbType.String) { Value = createAction.FileHash });
                    cmd.ExecuteNonQuery();
                }            
        }

        /// <summary>
        /// Insert a delete action
        /// </summary>
        /// <param name="deleteAction"></param>
        /// <param name="con"></param>
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

        /// <summary>
        /// Insert rename action
        /// </summary>
        /// <param name="renameAction"></param>
        /// <param name="con"></param>
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
