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
        private SqliteConnection conn;

        // Track whether Dispose has been called.
        private bool disposed = false;

        /// <summary>
        /// Creates a SQLiteAccess object of specified database file.
        /// Throws an exception if database connection cannot be opened.
        /// </summary>
        /// <param name="dbFilePath">File path of SQLite database file.</param>
        /// <exception cref="ArgumentNullException">dbFilePath is null.</exception>
        public SQLiteAccess(string dbFilePath)
        {
            // Note: dbFilePath need not exists.

            // Create directory for database file if it does not exist
            FileInfo fi = new FileInfo(dbFilePath);
            if (!fi.Directory.Exists) fi.Directory.Create();


            string connStr = connStr = String.Format(CONN_STRING_FORMAT, dbFilePath);

            // TEST: What if no access to file, security permissions, invalid path format, path too long, db cannot be created?

            if (dbFilePath == null)
                throw new ArgumentNullException("dbFilePath");

            // Creates a connection to Sqlite Database
            conn = new SqliteConnection(connStr);

            // Exception thrown if connection cannot be opened.
            TestConnection();
        }

        /// <summary>
        /// Executes specified SQL query and return query results in a DataTable.
        /// Throws an exception if query cannot be executed.
        /// </summary>
        /// <param name="cmdText">SQL select query string</param>
        /// <returns></returns>
        public DataTable ExecuteSelectCmd(string cmdText)
        {

            SqliteCommand cmd = GetCommand(cmdText, null);
            DataTable resultsTable = null;

            try
            {
                cmd.Connection.Open();
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    resultsTable = new DataTable();
                    resultsTable.Load(reader);
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
                cmd.Dispose();
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
            return ExecuteNonQuery(cmdText, null, atomic);
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
        public int ExecuteNonQuery(string cmdText, SqliteParameterCollection paramsList, bool atomic)
        {
            int affectedRows = 0;
            SqliteTransaction transaction = null;
            
            SqliteCommand cmd = GetCommand(cmdText, paramsList);

            try
            {
                cmd.Connection.Open();

                if (atomic)
                {
                    transaction = (SqliteTransaction)cmd.Connection.BeginTransaction();
                    cmd.Transaction = transaction;
                }

                affectedRows = cmd.ExecuteNonQuery(); // TODO: guarateed atomic if commandText has more than 1 statement??
                
                if (transaction != null) transaction.Commit();
                 
            }
            catch (Exception)
            {
                if (transaction != null) transaction.Rollback();
                throw;
            }
            finally
            {
                cmd.Connection.Close();
                cmd.Dispose();
            }

            return affectedRows;
        }


        /// <summary>
        /// Executes data reader of command object with specified SQL query
        /// </summary>
        /// <param name="cmdText">SQL query text.</param>
        /// <param name="callback">Callback method while moving through the records.</param>
        /// <returns></returns>
        public void ExecuteReader(string cmdText, DataReaderCallbackDelegate callback)
        {
            ExecuteReader(cmdText, null, callback);
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
            SqliteCommand cmd = GetCommand(cmdText, paramsList);

            try
            {
                cmd.Connection.Open();
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Need to check if callback is null?
                        callback(reader);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
                cmd.Dispose();
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
            SqliteCommand cmd = GetCommand(cmdText, paramsList);

            try
            {
                cmd.Connection.Open();
                retStr = cmd.ExecuteScalar().ToString();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
                cmd.Dispose();
            }

            return retStr;
        }

        /*
        /// <summary>
        /// Executes a transact-SQL statement without commit. Callee can commit or rollback by accessing
        /// Transaction property of returned command object.
        /// </summary>
        /// <param name="cmdText">SQL query text.</param>
        /// <param name="paramsList">Parameters list.</param>
        /// <returns></returns>
        public SqliteCommand ExecuteNonQueryWithoutCommit(string cmdText, SqliteParameterCollection paramsList)
        {
            SqliteCommand cmd = GetCommand(cmdText, paramsList);
            
            cmd.Connection.Open();

            SqliteTransaction transaction = (SqliteTransaction)cmd.Connection.BeginTransaction();
            cmd.Transaction = transaction;

            cmd.ExecuteNonQuery();

            return cmd;
        }
         */

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
        /// Test whether database connection is successful and throws an exception if
        /// it is not.
        /// </summary>
        private void TestConnection()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

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
                    if (conn.State != ConnectionState.Closed) conn.Close();
                    conn.Dispose();
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
