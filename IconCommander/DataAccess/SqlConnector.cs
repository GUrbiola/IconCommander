using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IconCommander.DataAccess
{

    /// <summary>
    /// Provides helper methods to execute SQL Server commands using ADO.NET,
    /// offering sync and async execution, transaction support, and simple logging.
    /// </summary>
    public partial class SqlConnector : IIconCommanderDb
    {
        #region Variables
        private DateTime _startTime, _endingTime;
        SqlCommand cmd = null;
        SqlDataAdapter da = null;
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
        /// Gets or sets the underlying SQL connection used by commands.
        /// </summary>
        public SqlConnection Connection { get; set; }

        /// <summary>
        /// Gets a new SQL connection instance using the current connection string.
        /// </summary>
        public SqlConnection NewConnection
        {
            get
            {
                return new SqlConnection(ConnectionString);
            }
        }

        /// <summary>
        /// Gets or sets the connection string used to establish SQL Server connections.
        /// </summary>
        public string ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }

        /// <summary>
        /// Gets or sets the current transaction associated with the command.
        /// </summary>
        public SqlTransaction Transaction
        {
            get { return cmd == null ? null : cmd.Transaction; }
            set { cmd.Transaction = value; }
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
        /// Gets or sets the last thrown SQL exception.
        /// </summary>
        public SqlException LastSqlException { get; set; }

        /// <summary>
        /// Gets the SQL Server instance (data source) from the current connection.
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
        /// Gets the current database name from the connection.
        /// </summary>
        public string DataBase
        {
            get
            {
                if (Connection != null)
                    return $"{Connection.DataSource} -> {Connection.Database}";

                return String.Empty;
            }
        }

        public int InsertedId { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SqlConnector with an empty connection.
        /// </summary>
        public SqlConnector()
        {
            TimeOut = 0;
            Connection = new SqlConnection();

            LastMessage = "OK";
            cmd = new SqlCommand();
            da = new SqlDataAdapter();
        }

        /// <summary>
        /// Initializes a new instance of the SqlConnector with the provided connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        public SqlConnector(string connectionString)
        {
            TimeOut = 0;
            Connection = new SqlConnection(connectionString);

            LastMessage = "OK";
            cmd = new SqlCommand();
            da = new SqlDataAdapter();
        }

        /// <summary>
        /// Initializes a new instance of the SqlConnector with the provided SqlConnection.
        /// </summary>
        /// <param name="connection">The SQL connection to use. If null, a new connection is created.</param>
        public SqlConnector(SqlConnection connection)
        {
            TimeOut = 0;
            Connection = (connection == null ? new SqlConnection() : Connection);

            LastMessage = "OK";
            cmd = new SqlCommand();
            da = new SqlDataAdapter();
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
                {
                    BeginTransaction();
                }
                cmd.CommandType = CommandType.Text;

                RowsAffected = cmd.ExecuteNonQuery();
                if (autoTransact)
                {
                    CommitTransaction();
                }

                back = SqlResponse<int>.Successful(1);

                LastMessage = "OK";
            }
            catch (SqlException sqlex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;

                back = SqlResponse<int>.Failure(LastMessage, sqlex);
            }
            catch (Exception ex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = ex.Message;
                LastException = ex;

                back = SqlResponse<int>.Failure(LastMessage, ex);
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
            SqlResponse<object> back = null;
            object result = null;
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
            catch (SqlException sqlex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                result = null;
                back = SqlResponse<object>.Failure(LastMessage, sqlex);
            }
            catch (Exception ex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = ex.Message;
                LastException = ex;
                result = null;
                back = SqlResponse<object>.Failure(LastMessage, ex);
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

                da = new SqlDataAdapter(cmd);
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
            catch (SqlException sqlex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                result = null;
                back = SqlResponse<DataSet>.Failure(LastMessage, sqlex);
            }
            catch (Exception ex)
            {
                if (autoTransact)
                {
                    RollbackTransaction();
                }
                LastMessage = ex.Message;
                LastException = ex;
                result = null;
                back = SqlResponse<DataSet>.Failure(LastMessage, ex);
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
        /// Creates a table in SQL Server based on the provided DataTable schema.
        /// </summary>
        /// <param name="table">The DataTable whose schema is used to generate a CREATE TABLE statement.</param>
        public void CreateTableInSQL(DataTable table)
        {
            string sql;
            sql = GetTableScript(table);
            ExecuteScalar(sql);
        }

        /// <summary>
        /// Builds a SQL Server CREATE TABLE script from a DataTable schema,
        /// including default values and primary key constraints.
        /// </summary>
        /// <param name="table">The DataTable to inspect.</param>
        /// <returns>A string containing the SQL CREATE TABLE and ALTER statements.</returns>
        public static string GetTableScript(DataTable table)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", table.TableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                bool isNumeric = false;
                bool usesColumnDefault = true;

                sql.AppendFormat("\n\t[{0}]", table.Columns[i].ColumnName);

                switch (table.Columns[i].DataType.ToString())
                {
                    case "System.Boolean":
                        sql.AppendFormat(" bit");
                        break;
                    case "System.Byte":
                        sql.AppendFormat(" smallint");
                        break;
                    case "System.Char":
                        sql.AppendFormat(" nvarchar(5)");
                        break;
                    case "System.Int16":
                        sql.Append(" smallint");
                        isNumeric = true;
                        break;
                    case "System.Int32":
                        sql.Append(" int");
                        isNumeric = true;
                        break;
                    case "System.Int64":
                        sql.Append(" bigint");
                        isNumeric = true;
                        break;
                    case "System.DateTime":
                        sql.Append(" datetime");
                        usesColumnDefault = false;
                        break;
                    case "System.Single":
                        sql.Append(" single");
                        isNumeric = true;
                        break;
                    case "System.Double":
                        sql.Append(" double");
                        isNumeric = true;
                        break;
                    case "System.Decimal":
                        sql.AppendFormat(" decimal(18, 6)");
                        isNumeric = true;
                        break;
                    default:
                    case "System.String":
                        if (table.Columns[i].MaxLength == -1)
                            sql.AppendFormat(" nvarchar(2000)");
                        else
                            sql.AppendFormat(" nvarchar({0})", table.Columns[i].MaxLength);
                        break;
                }

                if (table.Columns[i].AutoIncrement)
                {
                    sql.AppendFormat(" IDENTITY({0},{1})",
                        table.Columns[i].AutoIncrementSeed,
                        table.Columns[i].AutoIncrementStep);
                }
                else
                {
                    // DataColumns will add a blank DefaultValue for any AutoIncrement column. 
                    // We only want to create an ALTER statement for those columns that are not set to AutoIncrement. 
                    if (table.Columns[i].DefaultValue != null && !String.IsNullOrEmpty(table.Columns[i].DefaultValue.ToString()))
                    {
                        if (usesColumnDefault)
                        {
                            if (isNumeric)
                            {
                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    table.TableName,
                                    table.Columns[i].ColumnName,
                                    table.Columns[i].DefaultValue);
                            }
                            else
                            {
                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ('{2}') FOR [{1}];",
                                    table.TableName,
                                    table.Columns[i].ColumnName,
                                    table.Columns[i].DefaultValue);
                            }
                        }
                        else
                        {
                            // Default values on Date columns, e.g., "DateTime.Now" will not translate to SQL.
                            // This inspects the caption for a simple XML string to see if there is a SQL compliant default value, e.g., "GETDATE()".
                            try
                            {
                                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();

                                xml.LoadXml(table.Columns[i].Caption);

                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    table.TableName,
                                    table.Columns[i].ColumnName,
                                    xml.GetElementsByTagName("defaultValue")[0].InnerText);
                            }
                            catch
                            {
                                // Handle
                            }
                        }
                    }
                }

                if (!table.Columns[i].AllowDBNull)
                {
                    sql.Append(" NOT NULL");
                }

                sql.Append(",");
            }

            if (table.PrimaryKey.Length > 0)
            {
                StringBuilder primaryKeySql = new StringBuilder();

                primaryKeySql.AppendFormat("\n\tCONSTRAINT PK_{0} PRIMARY KEY (", table.TableName);

                for (int i = 0; i < table.PrimaryKey.Length; i++)
                {
                    primaryKeySql.AppendFormat("{0},", table.PrimaryKey[i].ColumnName);
                }

                primaryKeySql.Remove(primaryKeySql.Length - 1, 1);
                primaryKeySql.Append(")");

                sql.Append(primaryKeySql);
            }
            else
            {
                sql.Remove(sql.Length - 1, 1);
            }

            sql.AppendFormat("\n);\n{0}", alterSql.ToString());

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
        public SqlCommand AsyncCmd = null;

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
                LastSqlException = null;
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

                AsyncCmd = new SqlCommand(curquery, Connection);
                AsyncCmd.Connection.Open();
                using (SqlDataReader AsyncReader = AsyncCmd.ExecuteReader(CommandBehavior.CloseConnection))
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
            catch (SqlException sqlex)
            {
                AsyncResult = -1;
                LastMessage = sqlex.Message;
                LastSqlException = sqlex;
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
IF OBJECT_ID('dbo.SystemLog', 'U') IS NULL 
BEGIN
	CREATE TABLE dbo.SystemLog
	(
		Id INT NOT NULL  identity( 1, 1 )
		,Comment NVARCHAR(100) NOT NULL 
		,ClassName NVARCHAR(100) NULL
		,MethodName NVARCHAR(100) NULL
		,Executor NVARCHAR(50) NULL 
		,ExecutionTime DATETIME NOT NULL 
		,LogLevel  NVARCHAR(10) NOT NULL
		,ProcessType NVARCHAR(50) NULL
		,Exception INT NULL
		,CONSTRAINT PK_SystemLog PRIMARY KEY(Id)
	)
END

IF OBJECT_ID('dbo.SystemExceptions', 'U') IS NULL 
BEGIN
	CREATE TABLE dbo.SystemExceptions
	(
		Id INT NOT NULL  identity( 1, 1 )
		,Message NVARCHAR(500) NULL 
		,StackTrace NVARCHAR(MAX) NOT NULL 
		,Source NVARCHAR(150) NULL 
		,ExecutionTime DATETIME NOT NULL 
		,CONSTRAINT PK_SystemExceptions PRIMARY KEY(Id)
	)
END
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
    SET NOCOUNT ON
	DECLARE 
	
	SELECT 
		@Message = '@Message@', 
		@StackTrace = '@StackTrace@', 
		@Source = '@Source@', 
		@ExecutionTime = GETDATE()
	
	BEGIN TRANSACTION SystemExceptions_Insert WITH MARK N'Inserting new record into SystemExceptions';  
		BEGIN TRY
			
			INSERT INTO [dbo].[SystemExceptions]( [Message], [StackTrace], [Source], [ExecutionTime] )
			VALUES ( @Message, @StackTrace, @Source, @ExecutionTime )
			
			SELECT
				SCOPE_IDENTITY() AS Result
		
		END TRY
		BEGIN CATCH
			--return the data of the error
			SELECT 
				-1 AS Result
		        
			IF @@TRANCOUNT > 0
		        ROLLBACK TRANSACTION SystemExceptions_Insert;
		END CATCH
		
		IF @@TRANCOUNT > 0
		    COMMIT TRANSACTION SystemExceptions_Insert;
";

            sql = sql.Replace("@Message@", msg)
                     .Replace("@StackTrace@", stackTrace)
                     .Replace("@Source@", source);
            int back = -1;

            var buff = ExecuteScalar(sql);
            back = buff == null ? -1 : Convert.ToInt32(buff);
            if (back <= 0)
            {
                return SqlResponse<int>.Failure("Error while registering exception on DB.", LastException ?? (Exception)LastSqlException);
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
    SET NOCOUNT ON
	DECLARE @Comment NVARCHAR(500), @ClassName NVARCHAR(100), @MethodName NVARCHAR(100), @Executor NVARCHAR(50), @ExecutionTime DATETIME, @LogLevel NVARCHAR(10), @ProcessType NVARCHAR(50), @Exception INT
	SELECT 
		@Comment = '@Comment@', 
		@ClassName = '@ClassName@', 
		@MethodName = '@MethodName@', 
		@Executor = '@Executor@', 
		@ExecutionTime = GETDATE(), 
		@LogLevel = '@LogLevel@', 
		@ProcessType = '@ProcessType@', 
		@Exception = @Exception@
	
	BEGIN TRANSACTION SystemLog_Insert WITH MARK N'Inserting new record into SystemLog';  
		BEGIN TRY
			
			INSERT INTO [dbo].[SystemLog]( [Comment], [ClassName], [MethodName], [Executor], [ExecutionTime], [LogLevel], [ProcessType], [Exception] )
			VALUES ( @Comment, @ClassName, @MethodName, @Executor, @ExecutionTime, @LogLevel, @ProcessType, @Exception )
			
			SELECT
				SCOPE_IDENTITY() AS Result
		
		END TRY
		BEGIN CATCH
			--return the data of the error
			SELECT 
				-1 AS Result
		        
			IF @@TRANCOUNT > 0
		        ROLLBACK TRANSACTION SystemLog_Insert;
		END CATCH
		
		IF @@TRANCOUNT > 0
		    COMMIT TRANSACTION SystemLog_Insert;
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

            sql = sql.Replace("@Comment@", comment)
                     .Replace("@ClassName@", className)
                     .Replace("@MethodName@", methodName)
                     .Replace("@Executor@", executor)
                     .Replace("@LogLevel@", logLevel)
                     .Replace("@ProcessType@", processType)
                     .Replace("@Exception@", Ex == null ? "NULL" : exceptionId.ToString());

            var buff = ExecuteScalar(sql);
            back = buff == null ? -1 : Convert.ToInt32(buff);
            if (back <= 0)
            {
                return SqlResponse<int>.Failure("Error while registering log on DB.", LastException ?? (Exception)LastSqlException);
            }
            return SqlResponse<int>.Successful(back);
        }

        /// <summary>
        /// Begins a public transaction for bulk operations.
        /// SQL Server doesn't use PRAGMA optimizations like SQLite.
        /// Performance is controlled via connection string and server configuration.
        /// </summary>
        /// <param name="applyOptimizations">If true, applies optimizations for maximum speed (not applicable to SQL Server)</param>
        /// <returns>The IDbTransaction object for bulk operations.</returns>
        public IDbTransaction BeginBulkTransaction(bool applyOptimizations = false)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            // SQL Server doesn't use PRAGMA settings like SQLite
            // Performance is controlled via connection string and server configuration
            return Connection.BeginTransaction();
        }

        /// <summary>
        /// ULTRA-FAST BULK INSERT - Inserts thousands/millions of tags in chunks for SQL Server.
        /// </summary>
        /// <param name="allTags">List of (iconId, tag) tuples to insert</param>
        /// <param name="transaction">Transaction to use</param>
        /// <param name="worker">Background worker for progress reporting</param>
        public void BulkInsertAllTags(List<(int iconId, string tag)> allTags, IDbTransaction transaction, BackgroundWorker worker)
        {
            if (allTags == null || allTags.Count == 0)
                return;

            // Cast IDbTransaction to SqlTransaction for SQL Server
            SqlTransaction sqlTransaction = null;
            SqlConnection conn = null;
            bool shouldCloseConnection = false;

            if (transaction == null)
            {
                conn = new SqlConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }
            else
            {
                sqlTransaction = transaction as SqlTransaction;
                conn = sqlTransaction?.Connection;
            }

            // SQL Server can handle ~2100 parameters, with 2 params per row (iconId, tag)
            // we can safely do ~1000 rows per INSERT. Use 500 to be safe.
            int chunkSize = 500;
            int totalChunks = (int)Math.Ceiling((double)allTags.Count / chunkSize);

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                List<(int iconId, string tag)> chunk = allTags
                    .Skip(chunkIndex * chunkSize)
                    .Take(chunkSize)
                    .ToList();

                // Build massive multi-row INSERT with MERGE or INSERT IF NOT EXISTS pattern
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine("INSERT INTO IconTags(Icon, Tag)");
                sqlBuilder.AppendLine("VALUES");

                List<string> valuesClauses = new List<string>();
                for (int i = 0; i < chunk.Count; i++)
                {
                    valuesClauses.Add($"(@Icon{i}, @Tag{i})");
                }
                sqlBuilder.Append(string.Join(", ", valuesClauses));

                using (SqlCommand cmd = new SqlCommand(sqlBuilder.ToString(), conn, sqlTransaction))
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
        /// Closes the connection if it's open. Useful after bulk transactions.
        /// </summary>
        public void CloseBulkConnection()
        {
            if (Connection != null && Connection.State == ConnectionState.Open)
                Connection.Close();
        }

        /// <summary>
        /// Commits a public transaction.
        /// </summary>
        /// <param name="transaction">The transaction to commit.</param>
        public void CommitBulkTransaction(IDbTransaction transaction)
        {
            if (transaction != null)
                transaction.Commit();
        }

        /// <summary>
        /// Deletes all icons and related data from a specific vein.
        /// Cascades to IconTags and IconFiles.
        /// </summary>
        public bool DeleteIconsFromVein(int vein)
        {
            string sql = @"
                DELETE FROM IconTags WHERE Icon IN (SELECT Id FROM Icons WHERE Vein = @Vein);
                DELETE FROM IconFiles WHERE Icon IN (SELECT Id FROM Icons WHERE Vein = @Vein);
                DELETE FROM Icons WHERE Vein = @Vein;";

            var ps = new Dictionary<string, string> { { "@Vein", vein.ToString() } };
            var result = ExecuteNonQuery(sql, ps, autoTransact: true);

            return result.IsOK;
        }

        /// <summary>
        /// SQL Server doesn't use PRAGMA settings. This is a no-op for compatibility.
        /// </summary>
        public void DisableBulkInsertMode()
        {
            // SQL Server doesn't have equivalent to SQLite PRAGMA settings
            // Performance is controlled via connection string and server configuration
            // This is a no-op for compatibility with the interface
        }

        /// <summary>
        /// Drops indexes on IconFiles table for faster bulk insert.
        /// </summary>
        public void DropIconFilesIndexes()
        {
            ExecuteNonQuery("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IconFiles') DROP INDEX IX_IconFiles ON IconFiles");
        }

        /// <summary>
        /// Recreates indexes on IconFiles table after bulk insert.
        /// </summary>
        public void RecreateIconFilesIndexes()
        {
            ExecuteNonQuery("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IconFiles') CREATE NONCLUSTERED INDEX IX_IconFiles ON IconFiles(Icon)");
        }

        /// <summary>
        /// Drops search indexes on IconTags, keeps composite unique index.
        /// This allows fast inserts while preventing duplicates.
        /// </summary>
        public void DropIconTagsIndexes()
        {
            ExecuteNonQuery("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IconTags') DROP INDEX IX_IconTags ON IconTags");
            ExecuteNonQuery("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IconTags_1') DROP INDEX IX_IconTags_1 ON IconTags");
            ExecuteNonQuery("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IconTags_2') DROP INDEX IX_IconTags_2 ON IconTags");
        }


        /// <summary>
        /// Recreates search indexes on IconTags table after bulk insert.
        /// Also ensures the composite UNIQUE index exists.
        /// </summary>
        public void RecreateIconTagsIndexes()
        {
            // Recreate search indexes
            ExecuteNonQuery("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_IconTags') CREATE NONCLUSTERED INDEX idx_IconTags ON IconTags(Icon)");
            ExecuteNonQuery("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_IconTags1') CREATE NONCLUSTERED INDEX idx_IconTags2 ON IconTags(Tag)");
            ExecuteNonQuery("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_IconTags2') CREATE UNIQUE NONCLUSTERED INDEX idx_IconTags3 ON IconTags(Icon, Tag)");
        }

        /// <summary>
        /// SQL Server doesn't use PRAGMA settings. This is a no-op for compatibility.
        /// </summary>
        public void EnableBulkInsertMode()
        {
            // SQL Server doesn't have equivalent to SQLite PRAGMA settings
            // For bulk operations, consider using SqlBulkCopy instead
            // This is a no-op for compatibility with the interface
        }

        /// <summary>
        /// Gets the IconBuffer ID for a specific icon file.
        /// </summary>
        public SqlResponse<int> GetIconBufferId(string iconFile)
        {
            string sql = "SELECT Id FROM IconBuffer WHERE IconFile = @IconFile";
            var ps = new Dictionary<string, string> { { "@IconFile", iconFile } };
            var result = ExecuteScalar(sql, ps);

            if (result.IsOK && result.Result != null && result.Result != DBNull.Value)
            {
                int iconBufferId = Convert.ToInt32(result.Result);
                return SqlResponse<int>.Successful(iconBufferId);
            }

            return SqlResponse<int>.Successful(int.MinValue);
        }

        /// <summary>
        /// Gets the IconFile ID by hash (for deduplication).
        /// </summary>
        public int GetIconFileId(string fileHash)
        {
            string sql = "SELECT Id FROM IconFiles WHERE Hash = @Hash";

            using (SqlConnection cc = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, cc))
                {
                    cmd.Parameters.AddWithValue("@Hash", fileHash);

                    try
                    {
                        cc.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        LastMessage = ex.Message;
                        LastException = ex;
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Icon ID for a specific name and vein.
        /// Uses separate connection to avoid closing transaction's connection.
        /// </summary>
        public SqlResponse<int> GetIconId(int Vein, string iconFile)
        {
            string sql = "SELECT Id FROM Icons WHERE Name = @Name AND Vein = @Vein";

            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
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
                        return SqlResponse<int>.Failure(LastMessage, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets tags from temporary import tables for a specific icon file.
        /// Uses separate connection to avoid closing transaction's connection.
        /// </summary>
        public List<string> GetTagsFromTempTablesFor(string IconFile)
        {
            string sql = @"
                SELECT K.Keyword
                FROM IconKeywordBuffer Ik
                INNER JOIN IconBuffer I ON Ik.IconBuffer = I.Id
                INNER JOIN KeywordBuffer K ON Ik.KeywordBuffer = K.Id
                WHERE I.IconFile = @IconFile";

            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IconFile", IconFile);

                    try
                    {
                        conn.Open();
                        List<string> tags = new List<string>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
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

        /// <summary>
        /// Gets tags from temporary import tables for a specific icon file.
        /// Uses separate connection to avoid closing transaction's connection.
        /// </summary>
        public List<string> GetTagsForIcon(string IconFile)
        {
            string sql = @"
SELECT DISTINCT
	It.Tag
FROM
	dbo.Icons I INNER JOIN dbo.IconTags It on I.Id = It.Icon
WHERE
	I.Name = @IconFile
";

            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IconFile", IconFile);

                    try
                    {
                        conn.Open();
                        List<string> tags = new List<string>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
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

        /// <summary>
        /// Gets all tags for a specific IconFile ID (not Icon ID)
        /// </summary>
        public List<string> GetTagsForIconFileId(int iconFileId)
        {
            string sql = @"
SELECT DISTINCT
    It.Tag
FROM
    dbo.IconFiles Ifil
    INNER JOIN dbo.Icons I ON Ifil.Icon = I.Id
    INNER JOIN dbo.IconTags It ON I.Id = It.Icon
WHERE
    Ifil.Id = @IconFileId
ORDER BY It.Tag
";

            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IconFileId", iconFileId);

                    try
                    {
                        conn.Open();
                        List<string> tags = new List<string>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
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
                        return new List<string>();
                    }
                }
            }
        }

        /// <summary>
        /// Gets all unique tags from the database
        /// </summary>
        public List<string> GetAllTags()
        {
            string sql = "SELECT DISTINCT Tag FROM dbo.IconTags ORDER BY Tag";

            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        conn.Open();
                        List<string> tags = new List<string>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
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
                        return new List<string>();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Icon ID from an IconFile ID
        /// </summary>
        public int GetIconIdFromIconFileId(int iconFileId)
        {
            string sql = "SELECT Icon FROM dbo.IconFiles WHERE Id = @IconFileId";

            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IconFileId", iconFileId);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        LastMessage = ex.Message;
                        LastException = ex;
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a specific tag from an icon
        /// </summary>
        public bool RemoveTagFromIcon(int iconId, string tag)
        {
            string sql = "DELETE FROM dbo.IconTags WHERE Icon = @Icon AND Tag = @Tag";

            var ps = new Dictionary<string, string>
            {
                { "@Icon", iconId.ToString() },
                { "@Tag", tag }
            };

            var result = ExecuteNonQuery(sql, ps);
            return result.IsOK;
        }

        /// <summary>
        /// Inserts an icon file entry into IconBuffer (temporary import table).
        /// </summary>
        public bool Icons_Insert(int id, string fileName)
        {
            string sql = @"
IF NOT EXISTS (SELECT 1 FROM IconBuffer WHERE Id = @Id AND IconFile = @IconFile)
BEGIN
	INSERT INTO IconBuffer (Id, IconFile)
	VALUES (@Id, @IconFile)
END
";

            var ps = new Dictionary<string, string>
            {
                { "@Id", id.ToString() },
                { "@IconFile", fileName }
            };

            var result = ExecuteNonQuery(sql, ps);
            return result.IsOK;
        }

        /// <summary>
        /// Inserts a keyword entry into KeywordBuffer (temporary import table).
        /// </summary>
        public bool Keywords_Insert(int id, string word)
        {
            string sql = @"
IF NOT EXISTS (SELECT 1 FROM KeywordBuffer WHERE Id = @Id AND Keyword = @Word)
BEGIN
	INSERT INTO KeywordBuffer (Id, Keyword) 
	VALUES (@Id, @Word)
END
";

            var ps = new Dictionary<string, string>
            {
                { "@Id", id.ToString() },
                { "@Word", word }
            };

            var result = ExecuteNonQuery(sql, ps);
            return result.IsOK;
        }

        /// <summary>
        /// Registers an icon in the Icons table (without transaction).
        /// </summary>
        public bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg)
        {
            string sql = @"
IF NOT EXISTS (SELECT 1 FROM Icons WHERE Name = @Name AND Vein = @Vein)
BEGIN
    INSERT INTO Icons(Name, IsImage, IsIco, IsSvg, Vein)
    VALUES (@Name, @IsImage, @IsIco, @IsSvg, @Vein)
    
    SELECT SCOPE_IDENTITY() as InsertedId
END";

            var ps = new Dictionary<string, string>
            {
                { "@Name", crudeFileName },
                { "@Vein", vein.ToString() },
                { "@IsImage", isImage.ToString() },
                { "@IsIco", isIcon.ToString() },
                { "@IsSvg", isSvg.ToString() }
            };

            var result = ExecuteScalar(sql, ps);
            
            if(result.IsOK)
                InsertedId = Convert.ToInt32(result.Result);
            
            return result.IsOK;
        }

        /// <summary>
        /// Registers an icon with transaction support for bulk operations.
        /// </summary>
        /// <param name="crudeFileName">The icon file name</param>
        /// <param name="vein">The vein ID</param>
        /// <param name="isImage">Is image flag</param>
        /// <param name="isIcon">Is icon flag</param>
        /// <param name="isSvg">Is SVG flag</param>
        /// <param name="transaction">The transaction to use</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg, IDbTransaction transaction)
        {
            // Cast IDbTransaction to SqlTransaction for SQL Server
            SqlTransaction sqlTransaction = null;
            SqlConnection conn = null;
            bool shouldCloseConnection = false;

            if (transaction == null)
            {
                conn = new SqlConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }
            else
            {
                sqlTransaction = transaction as SqlTransaction;
                conn = sqlTransaction?.Connection;
            }

            try
            {
                // CRITICAL FIX: First try to insert, capture result within transaction
                string insertSql = @"
IF NOT EXISTS (SELECT 1 FROM Icons WHERE Name = @Name AND Vein = @Vein)
BEGIN
    INSERT INTO Icons(Name, IsImage, IsIco, IsSvg, Vein)
    VALUES (@Name, @IsImage, @IsIco, @IsSvg, @Vein)

    SELECT CAST(SCOPE_IDENTITY() AS INT) as InsertedId
END
ELSE
BEGIN
    SELECT Id as InsertedId FROM Icons WHERE Name = @Name AND Vein = @Vein
END";

                using (SqlCommand cmd = new SqlCommand(insertSql, conn, sqlTransaction))
                {
                    cmd.Parameters.AddWithValue("@Name", crudeFileName);
                    cmd.Parameters.AddWithValue("@Vein", vein);
                    cmd.Parameters.AddWithValue("@IsImage", isImage);
                    cmd.Parameters.AddWithValue("@IsIco", isIcon);
                    cmd.Parameters.AddWithValue("@IsSvg", isSvg);

                    // CRITICAL: This now ALWAYS returns the icon ID, whether inserted or existing
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        InsertedId = Convert.ToInt32(result);
                    }
                    else
                    {
                        InsertedId = 0;
                        LastMessage = $"Failed to get icon ID for {crudeFileName}";
                        return false;
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

        /// <summary>
        /// Registers a relationship between an icon and a keyword in temporary import tables.
        /// </summary>
        public bool RegisterIconRelationShip(int iconId, int keywordId)
        {
            string sql = @"
	INSERT INTO IconKeywordBuffer (IconBuffer, KeywordBuffer) 
	VALUES (@IconBuffer, @KeywordBuffer)";

            var ps = new Dictionary<string, string>
            {
                { "@IconBuffer", iconId.ToString() },
                { "@KeywordBuffer", keywordId.ToString() }
            };

            var result = ExecuteNonQuery(sql, ps);
            return result.IsOK;
        }

        /// <summary>
        /// Registers a single tag for an icon.
        /// </summary>
        public bool RegisterIconTag(int iconId, string tag)
        {
            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM IconTags WHERE Icon = @Icon AND Tag = @Tag)
                    INSERT INTO IconTags(Icon, Tag) VALUES (@Icon, @Tag)";

            var ps = new Dictionary<string, string>
            {
                { "@Icon", iconId.ToString() },
                { "@Tag", tag }
            };

            var result = ExecuteNonQuery(sql, ps);
            return result.IsOK;
        }

        /// <summary>
        /// TRUE BATCH insert - inserts ALL tags for an icon in a SINGLE SQL statement.
        /// </summary>
        /// <param name="iconId">The icon ID</param>
        /// <param name="tags">List of tags to insert</param>
        /// <param name="transaction">Optional transaction to use</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RegisterIconTagsBatch(int iconId, List<string> tags, IDbTransaction transaction = null)
        {
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

            // Cast IDbTransaction to SqlTransaction for SQL Server
            SqlTransaction sqlTransaction = transaction as SqlTransaction;
            SqlConnection conn = sqlTransaction?.Connection;
            bool shouldCloseConnection = false;

            if (conn == null)
            {
                conn = new SqlConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }

            try
            {
                // Build multi-row INSERT statement using VALUES and subquery to filter duplicates
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine("INSERT INTO IconTags(Icon, Tag)");
                sqlBuilder.AppendLine("SELECT Icon, Tag FROM (VALUES");

                List<string> valuesClauses = new List<string>();
                for (int i = 0; i < validTags.Count; i++)
                {
                    valuesClauses.Add($"(@Icon, @Tag{i})");
                }
                sqlBuilder.Append(string.Join(", ", valuesClauses));
                sqlBuilder.AppendLine(") AS Source(Icon, Tag)");
                sqlBuilder.AppendLine("WHERE NOT EXISTS (");
                sqlBuilder.AppendLine("    SELECT 1 FROM IconTags");
                sqlBuilder.AppendLine("    WHERE IconTags.Icon = Source.Icon AND IconTags.Tag = Source.Tag");
                sqlBuilder.AppendLine(")");

                using (SqlCommand cmd = new SqlCommand(sqlBuilder.ToString(), conn, sqlTransaction))
                {
                    cmd.Parameters.AddWithValue("@Icon", iconId);

                    for (int i = 0; i < validTags.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@Tag{i}", validTags[i]);
                    }

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

        /// <summary>
        /// Rolls back a public transaction.
        /// </summary>
        /// <param name="transaction">The transaction to roll back.</param>
        public void RollbackBulkTransaction(IDbTransaction transaction)
        {
            if (transaction != null)
                transaction.Rollback();
        }

        /// <summary>
        /// Saves an icon file with binary data (without transaction).
        /// </summary>
        public void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash)
        {
            string sql = @"
IF NOT EXISTS (SELECT 1 FROM IconFiles WHERE Hash = @Hash)
BEGIN
	INSERT INTO IconFiles(Icon, FileName, Extension, Type, Size, OriginalPath, BinData, IsMerged, Hash)
	VALUES (@Icon, @FileName, @Extension, @Type, @Size, @OriginalPath, @BinData, @IsMerged, @Hash)
	
	SELECT SCOPE_IDENTITY() as InsertedId
END
";
            InsertedId = -1;

            using (SqlConnection cc = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, cc))
                {
                    cmd.Parameters.AddWithValue("@Icon", iconId);
                    cmd.Parameters.AddWithValue("@FileName", crudeFileName);
                    cmd.Parameters.AddWithValue("@Extension", extension);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Size", width);
                    cmd.Parameters.AddWithValue("@OriginalPath", originalPath);
                    cmd.Parameters.Add("@BinData", SqlDbType.VarBinary, -1).Value = binData;
                    cmd.Parameters.AddWithValue("@IsMerged", isMerged);
                    cmd.Parameters.AddWithValue("@Hash", fileHash);

                    try
                    {
                        cc.Open();
                        var result = cmd.ExecuteScalar();

                        InsertedId = Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        LastMessage = ex.Message;
                        LastException = ex;
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Saves an icon file with binary data and transaction support.
        /// </summary>
        /// <param name="iconId">The icon ID</param>
        /// <param name="crudeFileName">The file name</param>
        /// <param name="extension">The file extension</param>
        /// <param name="type">The file type</param>
        /// <param name="width">The file size (width * height)</param>
        /// <param name="originalPath">The original file path</param>
        /// <param name="binData">The binary data</param>
        /// <param name="isMerged">Is merged flag</param>
        /// <param name="fileHash">The SHA256 hash</param>
        /// <param name="transaction">Optional transaction to use</param>
        public void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash, IDbTransaction transaction)
        {
            string sql = @"
IF NOT EXISTS (SELECT 1 FROM IconFiles WHERE Hash = @Hash)
BEGIN
	INSERT INTO IconFiles(Icon, FileName, Extension, Type, Size, OriginalPath, BinData, IsMerged, Hash)
	VALUES (@Icon, @FileName, @Extension, @Type, @Size, @OriginalPath, @BinData, @IsMerged, @Hash)
	
	SELECT SCOPE_IDENTITY() as InsertedId
END
";
            InsertedId = -1;

            // Cast IDbTransaction to SqlTransaction for SQL Server
            SqlTransaction sqlTransaction = null;
            SqlConnection conn = null;
            bool shouldCloseConnection = false;

            if (transaction == null)
            {
                conn = new SqlConnection(this.ConnectionString);
                conn.Open();
                shouldCloseConnection = true;
            }
            else
            {
                sqlTransaction = transaction as SqlTransaction;
                conn = sqlTransaction?.Connection;
            }

            using (SqlCommand cmd = new SqlCommand(sql, conn, sqlTransaction))
            {
                cmd.Parameters.AddWithValue("@Icon", iconId);
                cmd.Parameters.AddWithValue("@FileName", crudeFileName);
                cmd.Parameters.AddWithValue("@Extension", extension);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@Size", width);
                cmd.Parameters.AddWithValue("@OriginalPath", originalPath);
                cmd.Parameters.Add("@BinData", SqlDbType.VarBinary, -1).Value = binData;
                cmd.Parameters.AddWithValue("@IsMerged", isMerged);
                cmd.Parameters.AddWithValue("@Hash", fileHash);

                try
                {
                    var result = cmd.ExecuteScalar();
                    InsertedId = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    LastMessage = ex.Message;
                    LastException = ex;
                    throw;
                }
                finally
                {
                    if (shouldCloseConnection && conn != null && conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }
        }

        /// <summary>
        /// Validates database schema (stub implementation).
        /// </summary>
        public bool ValidateSchema()
        {
            return true;
        }

        public void Initialize(string ConnectionString)
        {
            TimeOut = 0;
            Connection = new SqlConnection(ConnectionString);

            LastMessage = "OK";
            cmd = new SqlCommand();
            da = new SqlDataAdapter();
        }

        #region BufferZone Methods

        /// <summary>
        /// Creates the BufferZone table if it doesn't exist
        /// </summary>
        public bool CreateBufferZoneTable()
        {
            string sql = @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BufferZone')
                BEGIN
                    CREATE TABLE BufferZone (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        IconFile INT NOT NULL,
                        Project INT,
                        CreationDate DATETIME NOT NULL,
                        FOREIGN KEY (IconFile) REFERENCES IconFiles(Id),
                        FOREIGN KEY (Project) REFERENCES Projects(Id)
                    )
                END";

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

            if (checkResult.IsOK && Convert.ToInt32(checkResult.Result) > 0)
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

            string sql = "INSERT INTO BufferZone (IconFile, Project, CreationDate) VALUES (@iconFile, @project, @date); SELECT SCOPE_IDENTITY()";

            Dictionary<string, string> insertParams = new Dictionary<string, string>
            {
                { "@iconFile", iconFileId.ToString() },
                { "@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            if (projectId.HasValue)
                insertParams.Add("@project", projectId.Value.ToString());
            else
                sql = "INSERT INTO BufferZone (IconFile, Project, CreationDate) VALUES (@iconFile, null, @date); SELECT SCOPE_IDENTITY()";

            var result = ExecuteScalar(sql, insertParams);

            if (result.IsOK)
            {
                return new SqlResponse<int> { Result = Convert.ToInt32(result.Result) };
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
                          WHERE ";

            if(projectId.HasValue)
            {
                sql += $"(bz.Project = {projectId.Value} OR bz.Project IS NULL)" + " ORDER BY bz.CreationDate"; ;
            }
            else
            {
                sql += "(bz.Project IS NULL)" + " ORDER BY bz.CreationDate";
            }

            return ExecuteTable(sql);
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
            string sql = "DELETE FROM BufferZone WHERE IconFile = @iconFile AND ";

            if (projectId.HasValue)
            {
                sql += $"Project = {projectId.Value}" + " ORDER BY bz.CreationDate"; ;
            }
            else
            {
                sql += "(bz.Project IS NULL)" + " ORDER BY bz.CreationDate";
            }

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
            string sql = "DELETE FROM BufferZone WHERE ";
                        

            if (projectId.HasValue)
            {
                sql += $"Project = {projectId.Value}";
            }
            else
            {
                sql += $"Project IS NULL";
            }
            return ExecuteNonQuery(sql);
        }

        #endregion
    }
}
