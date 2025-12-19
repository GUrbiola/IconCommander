using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.SQLite;

namespace IconCommander.DataAccess
{
    /// <summary>
    /// Provides helper methods to execute SQLite commands using ADO.NET,
    /// offering sync and async execution, transaction support, and simple logging.
    /// </summary>
    public partial class SqliteConnector : IIconCommanderDb
    {
        #region Variables
        private DateTime _startTime, _endingTime;
        SQLiteCommand cmd = null;
        SQLiteDataAdapter da = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the number of rows affected by the last non-query command.
        /// </summary>
        public int RowsAffected { get; set; }

        /// <summary>
        /// Gets or sets the number of rows read by the last query execution.
        /// </summary>
        public int RowsRead { get; set; }

        /// <summary>
        /// Gets or sets the last operation message. "OK" indicates success; otherwise contains an error message.
        /// </summary>
        public string LastMessage { get; set; }

        /// <summary>
        /// Gets or sets the underlying SQLite connection used by commands.
        /// </summary>
        public SQLiteConnection Connection { get; set; }

        /// <summary>
        /// Gets a new SQLite connection instance using the current connection string.
        /// </summary>
        public SQLiteConnection NewConnection
        {
            get
            {
                return new SQLiteConnection(ConnectionString);
            }
        }

        /// <summary>
        /// Gets or sets the connection string used to establish SQLite connections.
        /// </summary>
        public string ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }

        /// <summary>
        /// Gets the time span representing the last execution duration.
        /// </summary>
        public TimeSpan ExecutionLapse
        {
            get
            {
                return _endingTime.Subtract(_startTime);
            }
        }

        /// <summary>
        /// Gets a value indicating whether any execution is currently in progress.
        /// </summary>
        public bool OnExecution { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating the execution state. Setting true starts timing; false stops timing.
        /// </summary>
        public bool Executing
        {
            get
            {
                return OnExecution;
            }
            set
            {
                if (value)
                {
                    _startTime = DateTime.Now;
                    OnExecution = true;
                }
                else
                {
                    _endingTime = DateTime.Now;
                    OnExecution = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the command timeout (in seconds). Zero means default.
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Gets a value indicating whether the last operation produced an error.
        /// </summary>
        public bool Error
        {
            get
            {
                return LastMessage != "OK";
            }
        }

        /// <summary>
        /// Gets or sets the last thrown general exception.
        /// </summary>
        public Exception LastException { get; set; }

        /// <summary>
        /// Gets or sets the last thrown SQLite exception.
        /// </summary>
        public SQLiteException LastSqliteException { get; set; }

        /// <summary>
        /// Gets the SQLite database file path from the current connection.
        /// </summary>
        public string Server
        {
            get
            {
                if (Connection != null)
                    return Connection.DataSource;
                return "";
            }
        }

        /// <summary>
        /// Gets the current database name from the connection (for SQLite, this is the file path).
        /// </summary>
        public string DataBase
        {
            get
            {
                if (Connection != null)
                {
                    SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(Connection.ConnectionString);
                    return builder.DataSource;
                }
                    
                return "";
            }
        }

        public int InsertedId { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SqliteConnector with an empty connection.
        /// </summary>
        public SqliteConnector()
        {
            TimeOut = 0;
            Connection = new SQLiteConnection();

            LastMessage = "OK";
            cmd = new SQLiteCommand();
            da = new SQLiteDataAdapter();
        }

        /// <summary>
        /// Initializes a new instance of the SqliteConnector with the provided connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        public SqliteConnector(string connectionString)
        {
            TimeOut = 0;
            Connection = new SQLiteConnection(connectionString);

            LastMessage = "OK";
            cmd = new SQLiteCommand();
            da = new SQLiteDataAdapter();
        }

        /// <summary>
        /// Initializes a new instance of the SqliteConnector with the provided SQLiteConnection.
        /// </summary>
        /// <param name="connection">The SQLite connection to use. If null, a new connection is created.</param>
        public SqliteConnector(SQLiteConnection connection)
        {
            TimeOut = 0;
            Connection = (connection == null ? new SQLiteConnection() : connection);

            LastMessage = "OK";
            cmd = new SQLiteCommand();
            da = new SQLiteDataAdapter();
        }
        #endregion

        /// <summary>
        /// Tests the ability to open and close the current connection.
        /// </summary>
        /// <returns>True if connection open/close succeeds; otherwise false.</returns>
        public bool TestConnection()
        {
            try
            {
                Connection.Open();
                LastMessage = "OK";
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                return false;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
            }
            LastMessage = "OK";
            return true;
        }

        /// <summary>
        /// Begins a transaction on the current connection and assigns it to the command.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when connection or command is invalid.</exception>
        private void BeginTransaction()
        {
            if (Connection != null && cmd != null)
            {
                cmd.Transaction = Connection.BeginTransaction();
            }
            else
            {
                throw new InvalidOperationException("There must be a valid connection before starting a transaction.");
            }
        }

        /// <summary>
        /// Commits the current transaction associated with the command.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no transaction is in progress.</exception>
        private void CommitTransaction()
        {
            if (cmd != null && cmd.Transaction != null)
            {
                try
                {
                    cmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new InvalidOperationException("A transaction must be in progress before commiting the changes.");
            }
        }

        /// <summary>
        /// Rolls back the current transaction associated with the command.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no transaction is in progress.</exception>
        private void RollbackTransaction()
        {

            if (cmd != null && cmd.Transaction != null)
            {
                try
                {
                    cmd.Transaction.Rollback();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new InvalidOperationException("A transaction must be in progress before rolling back the changes.");
            }
        }

        /// <summary>
        /// Begins a public transaction for bulk operations with PRAGMA optimizations.
        /// Opens the connection, applies PRAGMA settings, then starts transaction.
        /// </summary>
        /// <param name="applyOptimizations">If true, applies PRAGMA optimizations for maximum speed</param>
        /// <returns>The SQLiteTransaction object to be used with bulk operations.</returns>
        public IDbTransaction BeginBulkTransaction(bool applyOptimizations = false)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            // CRITICAL: Apply PRAGMA settings BEFORE starting transaction
            // PRAGMA settings are per-connection and must be set on the OPEN connection
            if (applyOptimizations)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(Connection))
                {
                    cmd.CommandText = "PRAGMA synchronous = OFF";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "PRAGMA temp_store = MEMORY";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "PRAGMA cache_size = -64000";
                    cmd.ExecuteNonQuery();

                    // NOTE: EXCLUSIVE locking can cause "database is closed" errors during checkpoints
                    // Use NORMAL locking for better stability with multiple transactions
                    cmd.CommandText = "PRAGMA locking_mode = NORMAL";
                    cmd.ExecuteNonQuery();
                }
            }

            return Connection.BeginTransaction();
        }

        /// <summary>
        /// Commits a public transaction.
        /// </summary>
        /// <param name="transaction">The transaction to commit.</param>
        public void CommitBulkTransaction(IDbTransaction trans)
        {
            SQLiteTransaction transaction = (SQLiteTransaction)trans;
            if (transaction != null)
                transaction.Commit();
        }

        /// <summary>
        /// Rolls back a public transaction.
        /// </summary>
        /// <param name="transaction">The transaction to roll back.</param>
        public void RollbackBulkTransaction(IDbTransaction trans)
        {
            SQLiteTransaction transaction = (SQLiteTransaction)trans;
            if (transaction != null)
                transaction.Rollback();
        }

        /// <summary>
        /// Closes the connection if it's open. Useful after bulk transactions.
        /// </summary>
        public void CloseBulkConnection()
        {
            if (Connection != null && Connection.State == ConnectionState.Open)
                Connection.Close();
        }

        /// <summary>
        /// Drops ONLY the search indexes on IconTags, keeps composite unique index.
        /// This allows fast inserts while preventing duplicates.
        /// </summary>
        public void DropIconTagsIndexes()
        {
            ExecuteNonQuery("DROP INDEX IF EXISTS idx_IconTags");   // Tag search index
            ExecuteNonQuery("DROP INDEX IF EXISTS idx_IconTags2");  // Icon search index
            // NOTE: idx_IconTags3 (Icon, Tag) is kept as UNIQUE to prevent duplicates during import
        }

        /// <summary>
        /// Recreates search indexes on IconTags table after bulk insert.
        /// Also ensures the composite UNIQUE index exists.
        /// </summary>
        public void RecreateIconTagsIndexes()
        {
            // Recreate search indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_IconTags ON IconTags(Tag)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_IconTags2 ON IconTags(Icon)");

            // Ensure composite UNIQUE index exists (may already exist from schema)
            ExecuteNonQuery("CREATE UNIQUE INDEX IF NOT EXISTS idx_IconTags3 ON IconTags(Icon, Tag)");
        }

        /// <summary>
        /// Drops indexes on IconFiles table for faster bulk insert.
        /// </summary>
        public void DropIconFilesIndexes()
        {
            ExecuteNonQuery("DROP INDEX IF EXISTS idx_IconFiles");
        }

        /// <summary>
        /// Recreates indexes on IconFiles table after bulk insert.
        /// </summary>
        public void RecreateIconFilesIndexes()
        {
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_IconFiles ON IconFiles(Icon)");
        }

        /// <summary>
        /// Configures SQLite for maximum bulk insert performance.
        /// CAUTION: Reduces durability - use only for bulk imports!
        /// Call DisableBulkInsertMode() after import to restore normal settings.
        /// </summary>
        public void EnableBulkInsertMode()
        {
            ExecuteNonQuery("PRAGMA synchronous = OFF");        // Skip fsync - much faster writes
            ExecuteNonQuery("PRAGMA journal_mode = MEMORY");    // Keep journal in RAM
            ExecuteNonQuery("PRAGMA temp_store = MEMORY");      // Keep temp tables in RAM
            ExecuteNonQuery("PRAGMA cache_size = -64000");      // 64MB cache (negative = KB)
            ExecuteNonQuery("PRAGMA locking_mode = EXCLUSIVE"); // Exclusive lock for performance
        }

        /// <summary>
        /// Restores normal SQLite settings after bulk insert.
        /// Works on the currently open connection.
        /// </summary>
        public void DisableBulkInsertMode()
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return; // Connection already closed, settings will reset automatically

            using (SQLiteCommand cmd = new SQLiteCommand(Connection))
            {
                cmd.CommandText = "PRAGMA synchronous = NORMAL";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA journal_mode = DELETE";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA temp_store = DEFAULT";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA locking_mode = NORMAL";
                cmd.ExecuteNonQuery();

                // Note: cache_size persists for the connection, which is fine
            }
        }

        /// <summary>
        /// Executes a non-query SQL command (INSERT/UPDATE/DELETE).
        /// </summary>
        /// <param name="sql">The SQL text to execute.</param>
        /// <param name="autoTransact">If true, wraps execution in a transaction which is committed on success or rolled back on failure.</param>
        /// <returns>0 on success; -1 on failure.</returns>
        public SqlResponse<int> ExecuteNonQuery(string sql, Dictionary<string, string> ps = null, bool autoTransact = false)
        {
            int result = 0;
            SqlResponse<int> back = null;
            try
            {
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                cmd.CommandTimeout = TimeOut;

                if (ps != null && ps.Count > 0)
                {
                    cmd.Parameters.Clear();
                    foreach (var p in ps)
                        cmd.Parameters.AddWithValue(p.Key, p.Value);
                }

                Executing = true;
                cmd.Connection.Open();
                if (autoTransact)
                    BeginTransaction();

                cmd.CommandType = CommandType.Text;

                RowsAffected = cmd.ExecuteNonQuery();
                if (autoTransact)
                    CommitTransaction();

                back = SqlResponse<int>.Successful(result);
                LastMessage = "OK";
            }
            catch (SQLiteException sqlex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = sqlex.Message;
                LastSqliteException = sqlex;
                result = -1;
                back = SqlResponse<int>.Failure(sqlex);
            }
            catch (Exception ex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = ex.Message;
                LastException = ex;
                result = -1;
                back = SqlResponse<int>.Failure(ex);
            }
            finally
            {
                if (cmd != null)
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
                Executing = false;
            }
            return back;
        }

        /// <summary>
        /// Executes a SQL command and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL text to execute.</param>
        /// <param name="autoTransact">If true, wraps execution in a transaction which is committed on success or rolled back on failure.</param>
        /// <returns>The scalar value returned by the query; null if failed.</returns>
        public SqlResponse<object> ExecuteScalar(string sql, Dictionary<string, string> ps = null, bool autoTransact = false)
        {
            object result = null;
            SqlResponse<object> back = null;
            try
            {
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                cmd.CommandTimeout = TimeOut;

                if (ps != null && ps.Count > 0)
                {
                    cmd.Parameters.Clear();
                    foreach (var p in ps)
                        cmd.Parameters.AddWithValue(p.Key, p.Value);
                }

                Executing = true;
                cmd.Connection.Open();
                if (autoTransact)
                    BeginTransaction();

                cmd.CommandType = CommandType.Text;
                RowsRead = 0;
                result = cmd.ExecuteScalar();
                RowsRead++;
                if (autoTransact)
                    CommitTransaction();

                back = SqlResponse<object>.Successful(result);

                LastMessage = "OK";
            }
            catch (SQLiteException sqlex)
            {
                if (autoTransact)
                    RollbackTransaction();

                LastMessage = sqlex.Message;
                LastSqliteException = sqlex;
                result = null;

                back = SqlResponse<object>.Failure(sqlex);
            }
            catch (Exception ex)
            {
                if (autoTransact)
                    RollbackTransaction();


                LastMessage = ex.Message;
                LastException = ex;
                result = null;

                back = SqlResponse<object>.Failure(ex);
            }
            finally
            {
                if (cmd != null)
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
                Executing = false;
            }
            return back;
        }

        /// <summary>
        /// Executes a SQL query and returns a DataSet containing one or more result tables.
        /// </summary>
        /// <param name="sql">The SQL text to execute.</param>
        /// <param name="autoTransact">If true, wraps execution in a transaction which is committed on success or rolled back on failure.</param>
        /// <returns>A DataSet with the results; null if failed.</returns>
        public SqlResponse<DataSet> ExecuteDataSet(string sql, Dictionary<string, string> ps = null, bool autoTransact = false)
        {
            DataSet result = new DataSet();
            SqlResponse<DataSet> back = null;
            try
            {
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                cmd.CommandTimeout = TimeOut;

                if (ps != null && ps.Count > 0)
                {
                    cmd.Parameters.Clear();
                    foreach (var p in ps)
                        cmd.Parameters.AddWithValue(p.Key, p.Value);
                }

                da = new SQLiteDataAdapter(cmd);
                Executing = true;

                cmd.CommandType = CommandType.Text;

                da.SelectCommand.Connection.Open();
                if (autoTransact)
                    BeginTransaction();

                RowsRead = 0;
                da.Fill(result);
                foreach (DataTable tab in result.Tables)
                    if (tab != null && tab.Rows != null)
                        RowsRead = tab.Rows.Count;

                if (autoTransact)
                    CommitTransaction();

                back = SqlResponse<DataSet>.Successful(result);

                LastMessage = "OK";
            }
            catch (SQLiteException sqlex)
            {
                if (autoTransact)
                    RollbackTransaction();

                LastMessage = sqlex.Message;
                LastSqliteException = sqlex;
                result = null;
                back = SqlResponse<DataSet>.Failure(sqlex);
            }
            catch (Exception ex)
            {
                if (autoTransact)
                    RollbackTransaction();

                LastMessage = ex.Message;
                LastException = ex;
                result = null;
                back = SqlResponse<DataSet>.Failure(ex);
            }
            finally
            {
                if (da.SelectCommand != null)
                {
                    if (da.SelectCommand.Connection.State == ConnectionState.Open)
                        da.SelectCommand.Connection.Close();
                }
                Executing = false;
            }
            return back;
        }

        /// <summary>
        /// Executes a SQL query and returns the first table in the result.
        /// </summary>
        /// <param name="sql">The SQL text to execute.</param>
        /// <param name="autoTransact">If true, wraps execution in a transaction which is committed on success or rolled back on failure.</param>
        /// <param name="tableName">Optional name to assign to the returned DataTable.</param>
        /// <returns>The first DataTable from the result; null if failed or no tables.</returns>
        public SqlResponse<DataTable> ExecuteTable(string sql, Dictionary<string, string> ps = null, bool autoTransact = false, string tableName = "")
        {
            SqlResponse<DataSet> tmp = null;
            SqlResponse<DataTable> back = null;

            tmp = ExecuteDataSet(sql, ps, autoTransact);
            if (tmp.IsOK)
            {
                if (tmp?.Result != null && tmp.Result.Tables.Count > 0)
                {
                    DataTable table = tmp.Result.Tables[0];
                    if (!String.IsNullOrEmpty(tableName))
                        table.TableName = tableName; // Use 'table' instead of 'tmp.Result.Tables[0]'

                    back = SqlResponse<DataTable>.Successful(table);
                }
                else
                {
                    back = SqlResponse<DataTable>.Successful(new DataTable());
                }
                LastMessage = "OK";
            }
            else
            {
                back = new SqlResponse<DataTable>();
                back.Errors.AddRange(tmp.Errors);
                LastMessage = "Errors occurred during execution.";
            }

            return back;
        }

        /// <summary>
        /// Executes a SQL query expected to return a single column and maps it to a list of strings.
        /// </summary>
        /// <param name="sql">The SQL text to execute.</param>
        /// <param name="autoTransact">If true, wraps execution in a transaction which is committed on success or rolled back on failure.</param>
        /// <returns>A list of string values from the first column; empty list on failure or no rows.</returns>
        public SqlResponse<List<string>> ExecuteColumn(string sql, Dictionary<string, string> ps = null, bool autoTransact = false)
        {
            SqlResponse<List<string>> back = new SqlResponse<List<string>>();
            SqlResponse<DataTable> aux = ExecuteTable(sql, ps, autoTransact);

            if (aux.IsOK)
            {
                if (aux?.Result != null)
                {
                    DataTable table = (DataTable)aux.Result;
                    List<string> list = new List<string>();
                    int rc = table.Rows.Count;

                    for (int i = 0; i < rc; i++)
                        list.Add(table.Rows[i][0].ToString());

                    back = SqlResponse<List<string>>.Successful(list);
                }
                else
                {
                    back = SqlResponse<List<string>>.Successful(new List<string>());
                }
                LastMessage = "OK";
            }
            else
            {
                back = new SqlResponse<List<string>>();
                back.Errors.AddRange(aux.Errors);
                LastMessage = "Errors occurred during execution.";
            }

            return back;
        }

        /// <summary>
        /// Creates a table in SQLite based on the provided DataTable schema.
        /// </summary>
        /// <param name="table">The DataTable whose schema is used to generate a CREATE TABLE statement.</param>
        public void CreateTableInSQL(DataTable table)
        {
            string sql;
            sql = GetTableScript(table);
            ExecuteScalar(sql);
        }

        /// <summary>
        /// Builds a SQLite CREATE TABLE script from a DataTable schema,
        /// including default values and primary key constraints.
        /// </summary>
        /// <param name="table">The DataTable to inspect.</param>
        /// <returns>A string containing the SQLite CREATE TABLE statement.</returns>
        public static string GetTableScript(DataTable table)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE IF NOT EXISTS [{0}] (", table.TableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                bool isNumeric = false;
                bool isPrimaryKey = table.PrimaryKey.Length > 0 && table.PrimaryKey.Contains(table.Columns[i]);

                sql.AppendFormat("\n\t[{0}]", table.Columns[i].ColumnName);

                // SQLite type mapping
                switch (table.Columns[i].DataType.ToString())
                {
                    case "System.Boolean":
                        sql.AppendFormat(" INTEGER"); // SQLite uses INTEGER for boolean (0/1)
                        break;
                    case "System.Byte":
                        sql.AppendFormat(" INTEGER");
                        isNumeric = true;
                        break;
                    case "System.Char":
                        sql.AppendFormat(" TEXT");
                        break;
                    case "System.Int16":
                        sql.Append(" INTEGER");
                        isNumeric = true;
                        break;
                    case "System.Int32":
                        sql.Append(" INTEGER");
                        isNumeric = true;
                        break;
                    case "System.Int64":
                        sql.Append(" INTEGER");
                        isNumeric = true;
                        break;
                    case "System.DateTime":
                        sql.Append(" TEXT"); // SQLite stores datetime as TEXT in ISO8601 format
                        break;
                    case "System.Single":
                        sql.Append(" REAL");
                        isNumeric = true;
                        break;
                    case "System.Double":
                        sql.Append(" REAL");
                        isNumeric = true;
                        break;
                    case "System.Decimal":
                        sql.AppendFormat(" NUMERIC"); // SQLite NUMERIC for decimal values
                        isNumeric = true;
                        break;
                    default:
                    case "System.String":
                        sql.AppendFormat(" TEXT");
                        break;
                }

                // Handle AutoIncrement - SQLite uses INTEGER PRIMARY KEY AUTOINCREMENT
                if (table.Columns[i].AutoIncrement)
                {
                    sql.Append(" PRIMARY KEY AUTOINCREMENT");
                }
                else
                {
                    // Handle default values - in SQLite, defaults must be in the CREATE TABLE statement
                    if (table.Columns[i].DefaultValue != null && !String.IsNullOrEmpty(table.Columns[i].DefaultValue.ToString()))
                    {
                        if (isNumeric)
                        {
                            sql.AppendFormat(" DEFAULT {0}", table.Columns[i].DefaultValue);
                        }
                        else if (table.Columns[i].DataType == typeof(DateTime))
                        {
                            // For DateTime, try to get SQL-compliant default from caption
                            try
                            {
                                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                                xml.LoadXml(table.Columns[i].Caption);
                                sql.AppendFormat(" DEFAULT {0}", xml.GetElementsByTagName("defaultValue")[0].InnerText);
                            }
                            catch
                            {
                                // If no XML caption, use CURRENT_TIMESTAMP for datetime
                                sql.Append(" DEFAULT CURRENT_TIMESTAMP");
                            }
                        }
                        else
                        {
                            sql.AppendFormat(" DEFAULT '{0}'", table.Columns[i].DefaultValue.ToString().Replace("'", "''"));
                        }
                    }
                }

                // Handle NOT NULL constraint
                if (!table.Columns[i].AllowDBNull)
                {
                    sql.Append(" NOT NULL");
                }

                sql.Append(",");
            }

            // Add PRIMARY KEY constraint for composite keys or non-autoincrement single keys
            if (table.PrimaryKey.Length > 0)
            {
                // Check if this is not an autoincrement column (already handled above)
                bool hasAutoIncrement = false;
                foreach (var pkCol in table.PrimaryKey)
                {
                    if (pkCol.AutoIncrement)
                    {
                        hasAutoIncrement = true;
                        break;
                    }
                }

                // Only add PRIMARY KEY constraint if not autoincrement or if composite key
                if (!hasAutoIncrement || table.PrimaryKey.Length > 1)
                {
                    StringBuilder primaryKeySql = new StringBuilder();
                    primaryKeySql.Append("\n\tPRIMARY KEY (");

                    for (int i = 0; i < table.PrimaryKey.Length; i++)
                    {
                        primaryKeySql.AppendFormat("[{0}]", table.PrimaryKey[i].ColumnName);
                        if (i < table.PrimaryKey.Length - 1)
                            primaryKeySql.Append(",");
                    }

                    primaryKeySql.Append(")");
                    sql.Append(primaryKeySql);
                }
                else
                {
                    // Remove the trailing comma
                    sql.Remove(sql.Length - 1, 1);
                }
            }
            else
            {
                // Remove the trailing comma
                sql.Remove(sql.Length - 1, 1);
            }

            sql.Append("\n);");

            return sql.ToString();
        }

        #region Async execution properties and methods
        private Thread Executor;
        private string curquery;
        public int AsyncResult;
        private bool _CancelExecution;

        /// <summary>
        /// Occurs when an async query execution starts.
        /// </summary>
        public event ProcessingQuery StartExecution;

        /// <summary>
        /// Occurs when an async query execution finishes.
        /// </summary>
        public event ProcessingQuery FinishExecution;

        /// <summary>
        /// Gets or sets the command used during async operations.
        /// </summary>
        public SQLiteCommand AsyncCmd = null;

        private DataSet _Results;

        /// <summary>
        /// Gets the results DataSet from the last async execution.
        /// </summary>
        public DataSet Results { get { return _Results; } }

        /// <summary>
        /// Starts asynchronous execution of the provided SQL query, building a DataSet incrementally.
        /// </summary>
        /// <param name="Query">The SQL query text to execute asynchronously.</param>
        public void AsyncExecuteDataSet(string Query)
        {
            if (!OnExecution)
            {
                Executor = new Thread(AsyncExecQuery);
                curquery = Query;
                LastException = null;
                LastSqliteException = null;
                Executing = true;
                _CancelExecution = false;
                if (StartExecution != null)
                    StartExecution(Query, DateTime.Now);
                Executor.Start();
            }
            else
            {
                LastMessage = "There is already another async query on execution, must wait until its completion to execute a new one.";
            }
        }

        /// <summary>
        /// Requests cancellation of the current async execution loop.
        /// </summary>
        public void CancelExecute()
        {
            _CancelExecution = true;
        }

        /// <summary>
        /// Attempts to stop the async execution immediately and clean up resources.
        /// </summary>
        public void ExtremeStop()
        {
            if (Executor.IsAlive)
            {
                if (AsyncCmd != null)
                {
                    try
                    {
                        AsyncCmd.Cancel();
                        AsyncCmd.Dispose();
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
                AsyncResult = 0;
                Executor.Join();
                Executing = false;
                if (FinishExecution != null)
                    FinishExecution("", DateTime.Now);
            }
        }

        /// <summary>
        /// Internal thread method that executes the current query asynchronously and populates the Results DataSet.
        /// </summary>
        private void AsyncExecQuery()
        {
            DateTime LastCheck;
            _Results = new DataSet();
            int indexxx;
            try
            {

                AsyncCmd = new SQLiteCommand(curquery, Connection);
                AsyncCmd.Connection.Open();
                using (SQLiteDataReader AsyncReader = AsyncCmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    indexxx = 0;
                    LastCheck = DateTime.Now;
                    do
                    {
                        indexxx++;
                        // Create new data table
                        DataTable schemaTable = AsyncReader.GetSchemaTable();
                        DataTable dataTable = new DataTable();
                        if (schemaTable != null)
                        {// A query returning records was executed
                            for (int i = 0; i < schemaTable.Rows.Count; i++)
                            {
                                DataRow dataRow = schemaTable.Rows[i];
                                // Create a column name that is unique in the data table
                                string columnName = (string)dataRow["ColumnName"]; //+ "<C" + i + "/>";
                                if (dataTable.Columns.Contains(columnName))
                                {
                                    int index = 1;
                                    foreach (DataColumn Col in dataTable.Columns)
                                        if (Col.ColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                                            index++;
                                    columnName += index.ToString();
                                }
                                // Add the column definition to the data table
                                DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                                dataTable.Columns.Add(column);
                            }
                            _Results.Tables.Add(dataTable);
                            // Fill the data table we just created
                            while (AsyncReader.Read())
                            {
                                DataRow dataRow = dataTable.NewRow();
                                for (int i = 0; i < AsyncReader.FieldCount; i++)
                                    dataRow[i] = AsyncReader.GetValue(i);
                                dataTable.Rows.Add(dataRow);
                                if (DateTime.Now.Subtract(LastCheck) > new TimeSpan(0, 0, 1))
                                {
                                    if (_CancelExecution)
                                    {
                                        AsyncReader.Close();
                                        LastMessage = "OK";
                                        AsyncResult = 0;
                                        if (FinishExecution != null)
                                            FinishExecution(curquery, DateTime.Now);
                                        break;
                                    }
                                    else
                                    {
                                        LastCheck = DateTime.Now;
                                    }
                                }
                            }

                            DataTable NonQ1 = new DataTable("NonQuery" + indexxx.ToString());
                            NonQ1.Columns.Add(new DataColumn("RowsAffected"));
                            DataRow DRx1 = NonQ1.NewRow();
                            DRx1[0] = Math.Max(AsyncReader.RecordsAffected, 0);
                            NonQ1.Rows.Add(DRx1);
                            Results.Tables.Add(NonQ1);
                        }
                        else
                        {
                            // No records were returned
                            DataTable NonQ2 = new DataTable("NonQuery" + indexxx.ToString());
                            NonQ2.Columns.Add(new DataColumn("RowsAffected"));
                            DataRow DRx2 = NonQ2.NewRow();
                            DRx2[0] = Math.Max(AsyncReader.RecordsAffected, 0);
                            NonQ2.Rows.Add(DRx2);
                            Results.Tables.Add(NonQ2);
                        }
                    } while (AsyncReader.NextResult());
                    AsyncReader.Close();
                    LastMessage = "OK";
                }
            }
            catch (SQLiteException sqlex)
            {
                AsyncResult = -1;
                LastMessage = sqlex.Message;
                LastSqliteException = sqlex;
            }
            catch (Exception ex)
            {
                AsyncResult = -1;
                LastMessage = ex.Message;
                LastException = ex;
            }
            finally
            {
                Executing = false;
                if (AsyncCmd != null)
                    AsyncCmd.Dispose();
            }

            Executing = false;
            if (FinishExecution != null)
                FinishExecution(curquery, DateTime.Now);

            AsyncResult = 1;
        }
        #endregion

        /// <summary>
        /// Creates database tables required for logging (SystemLog and SystemExceptions) if they do not exist.
        /// </summary>
        public void CreateLogTables()
        {
            string sql = @"
CREATE TABLE IF NOT EXISTS SystemLog
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Comment TEXT NOT NULL,
    ClassName TEXT NULL,
    MethodName TEXT NULL,
    Executor TEXT NULL,
    ExecutionTime TEXT NOT NULL,
    LogLevel TEXT NOT NULL,
    ProcessType TEXT NULL,
    Exception INTEGER NULL
);

CREATE TABLE IF NOT EXISTS SystemExceptions
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Message TEXT NULL,
    StackTrace TEXT NOT NULL,
    Source TEXT NULL,
    ExecutionTime TEXT NOT NULL
);
";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// Registers an exception in the SystemExceptions table.
        /// </summary>
        /// <param name="Ex">The exception to register.</param>
        /// <returns>A response containing the inserted exception ID or failure details.</returns>
        private SqlResponse<int> RegisterException(Exception Ex)
        {
            return RegisterException(Ex.Message, Ex.StackTrace, Ex.Source);
        }

        /// <summary>
        /// Registers an exception in the SystemExceptions table using explicit fields.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        /// <param name="stackTrace">Exception stack trace.</param>
        /// <param name="source">Exception source.</param>
        /// <returns>A response containing the inserted exception ID or failure details.</returns>
        private SqlResponse<int> RegisterException(string msg, string stackTrace, string source)
        {
            string sql = @"
INSERT INTO SystemExceptions (Message, StackTrace, Source, ExecutionTime)
VALUES (@Message, @StackTrace, @Source, datetime('now'));

SELECT last_insert_rowid() AS Result;
";

            sql = sql.Replace("@Message", "'" + (msg ?? "").Replace("'", "''") + "'")
                     .Replace("@StackTrace", "'" + (stackTrace ?? "").Replace("'", "''") + "'")
                     .Replace("@Source", "'" + (source ?? "").Replace("'", "''") + "'");
            int back = -1;

            var buff = ExecuteScalar(sql);
            back = buff == null ? -1 : Convert.ToInt32(buff);
            if (back <= 0)
            {
                return SqlResponse<int>.Failure("Error while registering exception on DB.", LastException ?? (Exception)LastSqliteException);
            }
            return SqlResponse<int>.Successful(back);
        }

        /// <summary>
        /// Registers a DEBUG level log into SystemLog.
        /// </summary>
        /// <param name="comment">Log comment.</param>
        /// <param name="className">Origin class name.</param>
        /// <param name="methodName">Origin method name.</param>
        /// <param name="executor">Executor identity.</param>
        /// <param name="processType">Process type descriptor.</param>
        /// <returns>Response with inserted log ID or failure.</returns>
        public SqlResponse<int> Debug(string comment, string className, string methodName, string executor, string processType)
        {
            return RegisterLog(comment, className, methodName, executor, "DEBUG", processType);
        }

        /// <summary>
        /// Registers an INFO level log into SystemLog.
        /// </summary>
        /// <param name="comment">Log comment.</param>
        /// <param name="className">Origin class name.</param>
        /// <param name="methodName">Origin method name.</param>
        /// <param name="executor">Executor identity.</param>
        /// <param name="processType">Process type descriptor.</param>
        /// <returns>Response with inserted log ID or failure.</returns>
        public SqlResponse<int> Info(string comment, string className, string methodName, string executor, string processType)
        {
            return RegisterLog(comment, className, methodName, executor, "INFO", processType);
        }

        /// <summary>
        /// Registers a WARN level log into SystemLog, optionally linking an exception.
        /// </summary>
        /// <param name="comment">Log comment.</param>
        /// <param name="className">Origin class name.</param>
        /// <param name="methodName">Origin method name.</param>
        /// <param name="executor">Executor identity.</param>
        /// <param name="processType">Process type descriptor.</param>
        /// <param name="Ex">Optional exception to register.</param>
        /// <returns>Response with inserted log ID or failure.</returns>
        public SqlResponse<int> Warning(string comment, string className, string methodName, string executor, string processType, Exception Ex = null)
        {
            return RegisterLog(comment, className, methodName, executor, "WARN", processType, Ex);
        }

        /// <summary>
        /// Registers an ERROR level log into SystemLog, optionally linking an exception.
        /// </summary>
        /// <param name="comment">Log comment.</param>
        /// <param name="className">Origin class name.</param>
        /// <param name="methodName">Origin method name.</param>
        /// <param name="executor">Executor identity.</param>
        /// <param name="processType">Process type descriptor.</param>
        /// <param name="Ex">Optional exception to register.</param>
        /// <returns>Response with inserted log ID or failure.</returns>
        public SqlResponse<int> Exception(string comment, string className, string methodName, string executor, string processType, Exception Ex = null)
        {
            return RegisterLog(comment, className, methodName, executor, "ERROR", processType, Ex);
        }

        /// <summary>
        /// Registers a log record into SystemLog with the specified level, optionally linking an exception.
        /// </summary>
        /// <param name="comment">Log comment.</param>
        /// <param name="className">Origin class name.</param>
        /// <param name="methodName">Origin method name.</param>
        /// <param name="executor">Executor identity.</param>
        /// <param name="logLevel">Log level text (e.g., DEBUG, INFO, WARN, ERROR).</param>
        /// <param name="processType">Process type descriptor.</param>
        /// <param name="Ex">Optional exception to register.</param>
        /// <returns>Response with inserted log ID or failure.</returns>
        public SqlResponse<int> RegisterLog(string comment, string className, string methodName, string executor, string logLevel, string processType, Exception Ex = null)
        {
            string sql = @"
INSERT INTO SystemLog (Comment, ClassName, MethodName, Executor, ExecutionTime, LogLevel, ProcessType, Exception)
VALUES (@Comment, @ClassName, @MethodName, @Executor, datetime('now'), @LogLevel, @ProcessType, @Exception);

SELECT last_insert_rowid() AS Result;
";
            int exceptionId = -1, back;

            if (Ex != null)
            {
                var response = RegisterException(Ex);
                if (response.IsFailure)
                {
                    return SqlResponse<int>.Failure("Error while registering exception on DB.", response.Errors.FirstOrDefault()?.Exception);
                }
                exceptionId = response.Result;
            }

            sql = sql.Replace("@Comment", "'" + (comment ?? "").Replace("'", "''") + "'")
                     .Replace("@ClassName", "'" + (className ?? "").Replace("'", "''") + "'")
                     .Replace("@MethodName", "'" + (methodName ?? "").Replace("'", "''") + "'")
                     .Replace("@Executor", "'" + (executor ?? "").Replace("'", "''") + "'")
                     .Replace("@LogLevel", "'" + (logLevel ?? "").Replace("'", "''") + "'")
                     .Replace("@ProcessType", "'" + (processType ?? "").Replace("'", "''") + "'")
                     .Replace("@Exception", Ex == null ? "NULL" : exceptionId.ToString());

            var buff = ExecuteScalar(sql);
            back = buff == null ? -1 : Convert.ToInt32(buff);
            if (back <= 0)
            {
                return SqlResponse<int>.Failure("Error while registering log on DB.", LastException ?? (Exception)LastSqliteException);
            }
            return SqlResponse<int>.Successful(back);
        }

        public bool ValidateSchema()
        {

            return true;
        }

        public bool Keywords_Insert(int id, string word)
        {
            string sql = @"
INSERT INTO KeywordBuffer (Id, Keyword)
SELECT @Id@, '@Word@'
WHERE NOT EXISTS ( SELECT 1 FROM KeywordBuffer WHERE Id = @Id@ AND Keyword = '@Word@' )
";
            sql = sql.Replace("@Word@", word).Replace("@Id@", id.ToString());
            var result = ExecuteNonQuery(sql);

            return result.IsOK;
        }

        public bool Icons_Insert(int id, string fileName)
        {
            string sql = @"
INSERT INTO IconBuffer (Id, IconFile)
SELECT @Id@, '@IconFile@'
WHERE NOT EXISTS ( SELECT 1 FROM IconBuffer WHERE Id = @Id@ AND IconFile = '@IconFile@' )
";
            sql = sql.Replace("@IconFile@", fileName).Replace("@Id@", id.ToString());
            var result = ExecuteNonQuery(sql);

            return result.IsOK;
        }

        public SqlResponse<int> GetIconId(int Vein, string iconFile)
        {
            // CRITICAL: Use separate connection to avoid closing transaction's connection
            string sql = @"SELECT Id FROM Icons WHERE Name = @Name AND Vein = @Vein";

            using (SQLiteConnection conn = new SQLiteConnection(this.ConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", iconFile);
                    cmd.Parameters.AddWithValue("@Vein", Vein);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            int iconId = Convert.ToInt32(result);
                            return SqlResponse<int>.Successful(iconId);
                        }

                        return SqlResponse<int>.Successful(int.MinValue);
                    }
                    catch (Exception ex)
                    {
                        LastMessage = ex.Message;
                        LastException = ex;
                        return SqlResponse<int>.Failure(ex);
                    }
                }
            }
        }

        public SqlResponse<int> GetIconBufferId(string iconFile)
        {
            string sql = @"SELECT Id FROM IconBuffer WHERE IconFile = @IconFile";
            sql = sql.Replace("@IconFile@", iconFile);
            var result = ExecuteScalar(sql);

            if (result.IsOK)
            {
                int iconBufferId = Convert.ToInt32(result.Result);

                if (result.Results.Count > 0)
                    return SqlResponse<int>.Successful(iconBufferId);
                return SqlResponse<int>.Successful(int.MinValue);
            }

            return SqlResponse<int>.Failure(result.Message, result.Errors[0].Exception);
        }

        public bool RegisterIconRelationShip(int iconId, int keywordId)
        {
            string sql = @"
INSERT INTO IconKeywordBuffer (IconBuffer, KeywordBuffer)
SELECT @IconBuffer@, @KeywordBuffer@
WHERE NOT EXISTS ( SELECT 1 FROM IconKeywordBuffer WHERE IconBuffer = @IconBuffer@ AND KeywordBuffer = @KeywordBuffer@ )
";
            sql = sql.Replace("@IconBuffer@", iconId.ToString()).Replace("@KeywordBuffer@", keywordId.ToString());
            var result = ExecuteNonQuery(sql);

            return result.IsOK;
        }

        public bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg)
        {
            string sql = @"
INSERT INTO Icons(Name, IsImage, IsIco, IsSvg, Vein)
SELECT '@Name@', @IsImage@, @IsIco@, @IsSvg@, @Vein@
WHERE NOT EXISTS ( SELECT 1 FROM Icons WHERE Name = '@Name@' AND Vein = @Vein@ )";

            sql = sql.Replace("@Name@", crudeFileName)
                .Replace("@Vein@", vein.ToString())
                .Replace("@IsImage@", isImage.ToString())
                .Replace("@IsIco@", isIcon.ToString())
                .Replace("@IsSvg@", isSvg.ToString());

            var result = ExecuteNonQuery(sql);

            return result.IsOK;

        }

        /// <summary>
        /// Optimized RegisterIcon that accepts external transaction for bulk operations.
        /// </summary>
        public bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg, IDbTransaction trans)
        {
            SQLiteTransaction transaction = (SQLiteTransaction) trans;
            string sql = @"
INSERT INTO Icons(Name, IsImage, IsIco, IsSvg, Vein)
SELECT @Name, @IsImage, @IsIco, @IsSvg, @Vein
WHERE NOT EXISTS ( SELECT 1 FROM Icons WHERE Name = @Name AND Vein = @Vein )";

            SQLiteConnection conn = transaction?.Connection;
            bool shouldCloseConnection = false;

            if (conn == null)
            {
                conn = new SQLiteConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Name", crudeFileName);
                    cmd.Parameters.AddWithValue("@Vein", vein);
                    cmd.Parameters.AddWithValue("@IsImage", isImage);
                    cmd.Parameters.AddWithValue("@IsIco", isIcon);
                    cmd.Parameters.AddWithValue("@IsSvg", isSvg);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    // CRITICAL FIX: Capture the inserted ID within the same transaction
                    // This ensures VeinImport can use it without a separate query
                    if (rowsAffected > 0)
                    {
                        // Icon was inserted, get its ID
                        cmd.CommandText = "SELECT last_insert_rowid()";
                        cmd.Parameters.Clear();
                        object idResult = cmd.ExecuteScalar();
                        InsertedId = Convert.ToInt32(idResult);
                    }
                    else
                    {
                        // Icon already exists, query for its ID within transaction
                        cmd.CommandText = "SELECT Id FROM Icons WHERE Name = @Name AND Vein = @Vein";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Name", crudeFileName);
                        cmd.Parameters.AddWithValue("@Vein", vein);
                        object idResult = cmd.ExecuteScalar();
                        InsertedId = idResult != null ? Convert.ToInt32(idResult) : 0;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                return false;
            }
            finally
            {
                if (shouldCloseConnection && conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash)
        {
            string sql = @"
INSERT INTO IconFiles(Icon, FileName, Extension, Type, Size, OriginalPath, BinData, IsMerged, Hash)
SELECT @Icon, @FileName, @Extension, @Type, @Size, @OriginalPath, @BinData, @IsMerged, @Hash
WHERE NOT EXISTS ( SELECT 1 FROM IconFiles WHERE Hash = @Hash )	";

            using (SQLiteConnection cc = new SQLiteConnection(this.ConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, cc))
                {
                    cmd.Parameters.Add("@Icon", System.Data.DbType.Int32).Value = iconId;
                    cmd.Parameters.Add("@FileName", System.Data.DbType.String).Value = crudeFileName;
                    cmd.Parameters.Add("@Extension", System.Data.DbType.String).Value = extension;
                    cmd.Parameters.Add("@Type", System.Data.DbType.String).Value = type;
                    cmd.Parameters.Add("@Size", System.Data.DbType.Int32).Value = width;
                    cmd.Parameters.Add("@OriginalPath", System.Data.DbType.String).Value = originalPath;
                    cmd.Parameters.Add("@BinData", System.Data.DbType.Binary).Value = binData;
                    cmd.Parameters.Add("@IsMerged", System.Data.DbType.Int32).Value = isMerged;
                    cmd.Parameters.Add("@Hash", System.Data.DbType.String).Value = fileHash;

                    try
                    {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (cmd != null)
                        {
                            if (cmd.Connection.State == ConnectionState.Open)
                                cmd.Connection.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Optimized SaveIconFile that accepts external transaction for bulk operations.
        /// </summary>
        public void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash, IDbTransaction trans)
        {
            SQLiteTransaction transaction = (SQLiteTransaction)trans;
            string sql = @"
INSERT INTO IconFiles(Icon, FileName, Extension, Type, Size, OriginalPath, BinData, IsMerged, Hash)
SELECT @Icon, @FileName, @Extension, @Type, @Size, @OriginalPath, @BinData, @IsMerged, @Hash
WHERE NOT EXISTS ( SELECT 1 FROM IconFiles WHERE Hash = @Hash )";

            SQLiteConnection conn = transaction?.Connection;
            bool shouldCloseConnection = false;

            if (conn == null)
            {
                conn = new SQLiteConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }

            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn, transaction))
            {
                cmd.Parameters.Add("@Icon", DbType.Int32).Value = iconId;
                cmd.Parameters.Add("@FileName", DbType.String).Value = crudeFileName;
                cmd.Parameters.Add("@Extension", DbType.String).Value = extension;
                cmd.Parameters.Add("@Type", DbType.String).Value = type;
                cmd.Parameters.Add("@Size", DbType.Int32).Value = width;
                cmd.Parameters.Add("@OriginalPath", DbType.String).Value = originalPath;
                cmd.Parameters.Add("@BinData", DbType.Binary).Value = binData;
                cmd.Parameters.Add("@IsMerged", DbType.Int32).Value = isMerged;
                cmd.Parameters.Add("@Hash", DbType.String).Value = fileHash;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    LastMessage = ex.Message;
                    LastException = ex;
                    throw ex;
                }
                finally
                {
                    if (shouldCloseConnection && conn != null && conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }
        }

        public int GetIconFileId(string fileHash)
        {
            string sql = @"SELECT Id FROM IconFiles WHERE Hash = @Hash";
            int back = -1;
            using (SQLiteConnection cc = new SQLiteConnection(this.ConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, cc))
                {
                    cmd.Parameters.Add("@Hash", System.Data.DbType.String).Value = fileHash;

                    try
                    {
                        cmd.Connection.Open();
                        back = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (cmd != null)
                        {
                            if (cmd.Connection.State == ConnectionState.Open)
                                cmd.Connection.Close();
                        }
                    }
                }
            }
            return back;
        }

        public bool RegisterIconTag(int iconId, string tag)
        {
            string sql = @"
INSERT INTO IconTags(Icon, Tag)
SELECT @Icon@, '@Tag@'
WHERE NOT EXISTS ( SELECT 1 FROM IconTags WHERE Icon = @Icon@ AND Tag = '@Tag@' )
";
            sql = sql.Replace("@Icon@", iconId.ToString())
                     .Replace("@Tag@", tag);

            var result = ExecuteNonQuery(sql);

            return result.IsOK;

        }

        /// <summary>
        /// ULTRA-FAST BULK INSERT - Inserts thousands/millions of tags in chunks.
        /// This is THE KEY to massive performance improvement.
        /// </summary>
        /// <param name="allTags">List of (iconId, tag) tuples to insert</param>
        /// <param name="transaction">Transaction to use</param>
        /// <param name="worker">Background worker for progress reporting</param>
        public void BulkInsertAllTags(List<(int iconId, string tag)> allTags, IDbTransaction trans, System.ComponentModel.BackgroundWorker worker)
        {
            SQLiteTransaction transaction = (SQLiteTransaction)trans;

            if (allTags == null || allTags.Count == 0)
                return;

            SQLiteConnection conn = transaction?.Connection ?? Connection;

            // SQLite parameter limit is ~999, with 2 params per row (iconId, tag)
            // we can safely do ~450 rows per INSERT. Use 400 to be safe.
            int chunkSize = 400;
            int totalChunks = (int)Math.Ceiling((double)allTags.Count / chunkSize);

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                List<(int iconId, string tag)> chunk = allTags
                    .Skip(chunkIndex * chunkSize)
                    .Take(chunkSize)
                    .ToList();

                // Build massive multi-row INSERT
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine("INSERT OR IGNORE INTO IconTags(Icon, Tag) VALUES");

                List<string> valuesClauses = new List<string>();
                for (int i = 0; i < chunk.Count; i++)
                {
                    valuesClauses.Add($"(@Icon{i}, @Tag{i})");
                }
                sqlBuilder.Append(string.Join(", ", valuesClauses));

                using (SQLiteCommand cmd = new SQLiteCommand(sqlBuilder.ToString(), conn, transaction))
                {
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@Icon{i}", chunk[i].iconId);
                        cmd.Parameters.AddWithValue($"@Tag{i}", chunk[i].tag);
                    }

                    cmd.ExecuteNonQuery();
                }

                // Report progress
                int tagsProcessed = (chunkIndex + 1) * chunkSize;
                if (tagsProcessed > allTags.Count) tagsProcessed = allTags.Count;
                int percent = (tagsProcessed * 100) / allTags.Count;
                worker?.ReportProgress(0, $"  Inserted {tagsProcessed} / {allTags.Count} tags ({percent}%)");
            }
        }

        /// <summary>
        /// TRUE BATCH insert - inserts ALL tags in a SINGLE SQL statement.
        /// MASSIVE performance improvement over individual inserts.
        /// NOTE: This is now DEPRECATED - use BulkInsertAllTags instead!
        /// </summary>
        /// <param name="iconId">The icon ID to associate tags with.</param>
        /// <param name="tags">List of tag strings to insert.</param>
        /// <param name="transaction">Optional transaction to use. If null, creates own connection.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool RegisterIconTagsBatch(int iconId, List<string> tags, IDbTransaction trans = null)
        {
            SQLiteTransaction transaction = trans != null ? (SQLiteTransaction)trans : null;

            if (tags == null || tags.Count == 0)
                return true;

            // Filter out invalid tags
            List<string> validTags = new List<string>();
            foreach (string tag in tags)
            {
                if (!string.IsNullOrWhiteSpace(tag) && !tag.Equals("-") && tag.Length >= 2)
                    validTags.Add(tag);
            }

            if (validTags.Count == 0)
                return true;

            SQLiteConnection conn = transaction?.Connection;
            bool shouldCloseConnection = false;

            if (conn == null)
            {
                conn = new SQLiteConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }

            try
            {
                // Build multi-row INSERT statement with VALUES
                // INSERT OR IGNORE is MUCH faster than WHERE NOT EXISTS
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine("INSERT OR IGNORE INTO IconTags(Icon, Tag) VALUES");

                List<string> valuesClauses = new List<string>();
                for (int i = 0; i < validTags.Count; i++)
                {
                    valuesClauses.Add($"(@Icon, @Tag{i})");
                }
                sqlBuilder.Append(string.Join(", ", valuesClauses));

                using (SQLiteCommand cmd = new SQLiteCommand(sqlBuilder.ToString(), conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Icon", iconId);

                    for (int i = 0; i < validTags.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@Tag{i}", validTags[i]);
                    }

                    // SINGLE ExecuteNonQuery for ALL tags!
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                return false;
            }
            finally
            {
                if (shouldCloseConnection && conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public List<string> GetTagsFromTempTablesFor(string IconFile)
        {
            // CRITICAL: Use separate connection to avoid closing transaction's connection
            string sql = @"
SELECT
	K.Keyword
FROM
	IconKeywordBuffer Ik INNER JOIN IconBuffer I on Ik.IconBuffer = I.Id
	INNER JOIN KeywordBuffer K on Ik.KeywordBuffer = K.Id
WHERE
	I.IconFile = @IconFile
";

            using (SQLiteConnection conn = new SQLiteConnection(this.ConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IconFile", IconFile);

                    try
                    {
                        conn.Open();
                        List<string> tags = new List<string>();

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                    tags.Add(reader.GetString(0));
                            }
                        }

                        return tags;
                    }
                    catch (Exception ex)
                    {
                        LastMessage = ex.Message;
                        LastException = ex;
                        return null;
                    }
                }
            }
        }

        public bool DeleteIconsFromVein(int vein)
        {
            string sql = "DELETE FROM IconTags AS It where It.Icon in (SELECT I.Id FROM Icons AS I WHERE I.Vein = @Vein@)";

            sql = sql.Replace("@Vein@", vein.ToString());
            var result1 = ExecuteNonQuery(sql);

            if (result1.IsOK)
            {
                sql = "DELETE FROM IconFiles AS Icf where Icf.Icon in (SELECT I.Id FROM Icons AS I WHERE I.Vein = @Vein@)";

                sql = sql.Replace("@Vein@", vein.ToString());
                var result2 = ExecuteNonQuery(sql);
                if (result2.IsOK)
                {
                    sql = "DELETE FROM Icons AS I WHERE I.Vein = @Vein@";

                    sql = sql.Replace("@Vein@", vein.ToString());
                    var result3 = ExecuteNonQuery(sql);

                    if (result3.IsOK)
                        return true;
                }
            }
            return false;
        }

        public void Initialize(string ConnectionString)
        {
            TimeOut = 0;
            Connection = new SQLiteConnection(ConnectionString);

            LastMessage = "OK";
            cmd = new SQLiteCommand();
            da = new SQLiteDataAdapter();
        }

        #region BufferZone Methods

        /// <summary>
        /// Creates the BufferZone table if it doesn't exist
        /// </summary>
        public bool CreateBufferZoneTable()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS BufferZone (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IconFile INTEGER NOT NULL,
                Project INTEGER,
                CreationDate TEXT NOT NULL,
                FOREIGN KEY (IconFile) REFERENCES IconFiles(Id),
                FOREIGN KEY (Project) REFERENCES Projects(Id)
            )";

            var result = ExecuteNonQuery(sql);
            return result.IsOK;
        }

        /// <summary>
        /// Adds an icon to the buffer zone
        /// </summary>
        public SqlResponse<int> BufferZone_Insert(int iconFileId, int? projectId)
        {
            // Check if already in buffer for this project
            string checkSql = "SELECT COUNT(*) FROM BufferZone WHERE IconFile = @iconFile AND " +
                             (projectId.HasValue ? "Project = @project" : "Project IS NULL");

            Dictionary<string, string> checkParams = new Dictionary<string, string>
            {
                { "@iconFile", iconFileId.ToString() }
            };

            if (projectId.HasValue)
                checkParams.Add("@project", projectId.Value.ToString());

            var checkResult = ExecuteScalar(checkSql, checkParams);

            if (checkResult.IsOK && Convert.ToInt64(checkResult.Result) > 0)
            {
                // Already in buffer
                return new SqlResponse<int>
                {
                    Result = -1,
                    Errors = new List<ErrorOnResponse>
                    {
                        new ErrorOnResponse { Message = "Icon already in buffer zone" }
                    }
                };
            }

            string sql = "INSERT INTO BufferZone (IconFile, Project, CreationDate) VALUES (@iconFile, @project, @date)";

            Dictionary<string, string> insertParams = new Dictionary<string, string>
            {
                { "@iconFile", iconFileId.ToString() },
                { "@project", projectId.HasValue ? projectId.Value.ToString() : null },
                { "@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            var result = ExecuteNonQuery(sql, insertParams);

            if (result.IsOK)
            {
                // Get the last inserted ID
                var idResult = ExecuteScalar("SELECT last_insert_rowid()");
                if (idResult.IsOK)
                {
                    return new SqlResponse<int> { Result = Convert.ToInt32(idResult.Result) };
                }
            }

            return new SqlResponse<int>
            {
                Result = -1,
                Errors = result.Errors
            };
        }

        /// <summary>
        /// Loads buffer zone icons for a specific project (or null for no project)
        /// </summary>
        public SqlResponse<DataTable> BufferZone_GetByProject(int? projectId)
        {
            string sql = @"SELECT bz.Id, bz.IconFile, bz.Project, bz.CreationDate,
                          f.FileName, f.Extension, f.Type, f.BinData, i.Name AS IconName
                          FROM BufferZone bz
                          INNER JOIN IconFiles f ON bz.IconFile = f.Id
                          INNER JOIN Icons i ON f.Icon = i.Id
                          WHERE " + (projectId.HasValue ? "(bz.Project = @project OR bz.Project IS NULL)" : "bz.Project IS NULL") +
                          " ORDER BY bz.CreationDate";

            if (projectId.HasValue)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "@project", projectId.Value.ToString() }
                };
                return ExecuteTable(sql, parameters);
            }
            else
            {
                return ExecuteTable(sql);
            }
        }

        /// <summary>
        /// Removes an icon from the buffer zone
        /// </summary>
        public SqlResponse<int> BufferZone_Delete(int bufferZoneId)
        {
            string sql = "DELETE FROM BufferZone WHERE Id = @id";
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "@id", bufferZoneId.ToString() }
            };
            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Removes an icon file from buffer zone for a specific project (or null project)
        /// </summary>
        public SqlResponse<int> BufferZone_DeleteByIconFile(int iconFileId, int? projectId)
        {
            string sql = "DELETE FROM BufferZone WHERE IconFile = @iconFile AND " +
                        (projectId.HasValue ? "Project = @project" : "Project IS NULL");

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "@iconFile", iconFileId.ToString() }
            };

            if (projectId.HasValue)
                parameters.Add("@project", projectId.Value.ToString());

            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Removes all icons from buffer zone for a specific project
        /// </summary>
        public SqlResponse<int> BufferZone_ClearForProject(int? projectId)
        {
            string sql = "DELETE FROM BufferZone WHERE " +
                        (projectId.HasValue ? "Project = @project" : "Project IS NULL");

            if (projectId.HasValue)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "@project", projectId.Value.ToString() }
                };
                return ExecuteNonQuery(sql, parameters);
            }
            else
            {
                return ExecuteNonQuery(sql);
            }
        }

        public List<string> GetTagsForIcon(string IconFile)
        {
            string sql = @"
SELECT DISTINCT
	It.Tag
FROM 
	Icons I INNER JOIN IconTags It on I.Id = It.Icon
WHERE
	I.Name =  @IconFile
";

            using (SQLiteConnection conn = new SQLiteConnection(this.ConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IconFile", IconFile);

                    try
                    {
                        conn.Open();
                        List<string> tags = new List<string>();

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                    tags.Add(reader.GetString(0));
                            }
                        }

                        return tags;
                    }
                    catch (Exception ex)
                    {
                        LastMessage = ex.Message;
                        LastException = ex;
                        return null;
                    }
                }
            }
        }

        #endregion
    }
}
