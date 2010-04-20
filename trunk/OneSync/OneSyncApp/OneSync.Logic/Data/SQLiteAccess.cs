//Coded by Nguyen van Thuat
using System;
using System.Data;
using Community.CsharpSqlite.SQLiteClient;
using System.IO;

namespace OneSync.Synchronization
{
    public class SQLiteAccess: IDisposable
    {
        public delegate void DataReaderCallbackDelegate(SqliteDataReader reader);

        private const string CONN_STRING_FORMAT = "Version=3,uri=file:{0}";
        private SqliteConnection conn = null;

        // Track whether Dispose has been called.
        private bool disposed = false;
        private string dbFilePath = "";
        private bool createIfNotExist = true;

        private string connectionString = "";
        /// <summary>
        /// Creates a SQLiteAccess object of specified database file.
        /// Throws an exception if database connection cannot be opened.
        /// </summary>
        /// <param name="dbFilePath">File path of SQLite database file.</param>
        /// <exception cref="ArgumentNullException">dbFilePath is null.</exception>
        public SQLiteAccess(string dbFilePath, bool createIfNotExist)
        {
            // Note: dbFilePath need not exists.
            this.dbFilePath = dbFilePath;
            this.createIfNotExist = createIfNotExist;
            connectionString = String.Format(CONN_STRING_FORMAT, dbFilePath);
        }

        /// <summary>
        /// Create a new connection based on parameters
        /// </summary>
        /// <returns></returns>
        public SqliteConnection NewSQLiteConnection()
        {     
            conn = new SqliteConnection(connectionString);
            // Create directory for database file if it does not exist
            FileInfo fi = new FileInfo(dbFilePath);
            if ( fi.Exists )
            {
                conn.Open();
                return conn;
            }
            if ((!fi.Exists) && createIfNotExist)
            {
                fi.Directory.Create();
                conn.Open();
                return conn;
            }
            return null;           
        }
        /// <summary>
        /// Get the current connection
        /// </summary>
        /// <returns></returns>
        public SqliteConnection GetCurrentConnection()
        {
            return conn;
        }
        /// <summary>
        /// Executes specified SQL query and return query results in a DataTable.
        /// Throws an exception if query cannot be executed.
        /// </summary>
        /// <param name="cmdText">SQL select query string</param>
        /// <returns></returns>
        public DataTable ExecuteSelectCmd(string cmdText)
        {            
            DataTable resultsTable = null;
           
            using (SqliteCommand cmd = GetCommand(cmdText, null))
            {
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    resultsTable = new DataTable();
                    resultsTable.Load(reader);                   
                }
            }                     
            return resultsTable;
        }

        /// <summary>
        /// Executes a SQL statement against the SQLite connection
        /// and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">SQL query string.</param>
        /// <param name="paramsList">Parameters list.</param>
        /// <param name="atomic">Whether query is executed as a transaction.</param>
        /// <returns>
        /// For UPDATE, INSERT, and DELETE statements, the return value is the
        /// number of rows affected by the command.
        /// For all other types of statements, the return value is -1.
        /// </returns>
        public int ExecuteNonQuery(string cmdText, bool atomic)
        {
            return ExecuteNonQuery(cmdText, null);
        }

        /// <summary>
        /// Executes a SQL statement against the SQLite connection
        /// and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">SQL query string.</param>
        /// <param name="paramsList">Parameters list.</param>
        /// <param name="atomic">Whether query is executed as a transaction.</param>
        /// <returns>
        /// For UPDATE, INSERT, and DELETE statements, the return value is the
        /// number of rows affected by the command.
        /// For all other types of statements, the return value is -1.
        /// </returns>
        public int ExecuteNonQuery(string cmdText, SqliteParameterCollection paramsList)
        {
            int affectedRows = 0;
            
            using (SqliteCommand cmd = GetCommand(cmdText, paramsList))
            {
                affectedRows = cmd.ExecuteNonQuery();
            }
            return affectedRows;
        }

               
       
        /// <summary>
        /// Executes data reader of command object with specified SQL query
        /// </summary>
        /// <param name="cmdText">SQL query text.</param>
        /// <param name="paramsList">Parameters list.</param>
        /// <param name="callback">Callback method while moving through the records.</param>
        /// <returns></returns>
        public void ExecuteReader(string cmdText, SqliteParameterCollection paramsList, DataReaderCallbackDelegate callback)
        {
            using (SqliteCommand cmd = GetCommand(cmdText, paramsList))
            {
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {                        
                        // Need to check if callback is null?
                        if (callback != null) callback(reader);
                    }
                }
            }         
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// All other columns and rows are ignored.
        /// </summary>
        /// <param name="cmdText">SQL query string.</param>
        /// /// <param name="paramsList">Parameters list.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public string ExecuteScalar(string cmdText, SqliteParameterCollection paramsList)
        {
            string retStr = null;
            using (SqliteCommand cmd = GetCommand(cmdText, paramsList))
            {
                retStr = cmd.ExecuteScalar().ToString();
            }         
            return retStr;
        }
 
        /// <summary>
        /// Returns a command object associated with a transaction and opened connection.
        /// Associated transaction object can be accessed from transaction property
        /// of returned command object.
        /// </summary>
        /// <returns></returns>
        public SqliteCommand GetTransactCommand()
        {
            SqliteCommand cmd = GetCommand("", null);
            cmd.Connection.Open();
            SqliteTransaction transaction = (SqliteTransaction)cmd.Connection.BeginTransaction();

            cmd.Transaction = transaction;
            return cmd;
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// All other columns and rows are ignored.
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <returns>The first column of the first row in the result set.</returns>

        /// <summary>
        /// Gets Command object with specified command text of current Sqlite connection
        /// </summary>
        private SqliteCommand GetCommand(string cmdText, SqliteParameterCollection paramsList)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = cmdText;

            // Add parameters is necessary
            if (paramsList != null)
            {
                cmd.Parameters.Clear();
                foreach(SqliteParameter p in paramsList)
                {
                    cmd.Parameters.Add(p);
                }
            }

            return cmd;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    //if (conn.State != ConnectionState.Closed) conn.Close();
                    if (conn != null) conn.Dispose();
                }

                // Call the appropriate methods to clean up unmanaged resources here.
                // If disposing is false, only the following code is executed.

                // Note disposing has been done.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~SQLiteAccess()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}
