using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;

namespace IconCommander.DataAccess
{
    /// <summary>
    /// Defines the contract for all database operations used by IconCommander.
    /// Implemented by <see cref="SqliteConnector"/> (SQLite) and <see cref="SqlConnector"/> (SQL Server),
    /// allowing the UI to remain storage-agnostic.
    /// </summary>
    public interface IIconCommanderDb
    {
        /// <summary>Gets or sets the active database connection string.</summary>
        string ConnectionString { get; set; }

        /// <summary>Gets the name of the connected database.</summary>
        string DataBase { get; }

        /// <summary>Gets a value indicating whether the last operation produced an error.</summary>
        bool Error { get; }

        /// <summary>Gets or sets a value indicating whether an asynchronous operation is currently running.</summary>
        bool Executing { get; set; }

        /// <summary>Gets the wall-clock duration of the last database operation.</summary>
        TimeSpan ExecutionLapse { get; }

        /// <summary>Gets or sets the exception thrown by the last failed operation, if any.</summary>
        Exception LastException { get; set; }

        /// <summary>Gets or sets the status message of the last operation (e.g. "OK" or an error description).</summary>
        string LastMessage { get; set; }

        /// <summary>Gets the <see cref="DataSet"/> produced by the last async query.</summary>
        DataSet Results { get; }

        /// <summary>Gets or sets the number of rows affected by the last non-query operation.</summary>
        int RowsAffected { get; set; }

        /// <summary>Gets or sets the number of rows read by the last query operation.</summary>
        int RowsRead { get; set; }

        /// <summary>Gets the server or file path for the connected database.</summary>
        string Server { get; }

        /// <summary>Gets or sets the command timeout in milliseconds.</summary>
        int TimeOut { get; set; }

        /// <summary>Gets or sets the auto-incremented ID produced by the most recent INSERT statement.</summary>
        int InsertedId { get; set; }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        /// <summary>Opens and validates the database connection using the supplied connection string.</summary>
        /// <param name="ConnectionString">ADO.NET connection string for the target database.</param>
        void Initialize(string ConnectionString);

        /// <summary>Executes a SELECT query asynchronously on a background thread; results are stored in <see cref="Results"/>.</summary>
        /// <param name="Query">The SQL SELECT statement to execute.</param>
        void AsyncExecuteDataSet(string Query);

        /// <summary>Signals a running async operation to stop at the next cancellation checkpoint.</summary>
        void CancelExecute();

        /// <summary>Forces an immediate halt of any in-progress operation.</summary>
        void ExtremeStop();

        /// <summary>Opens a dedicated bulk-insert connection with optional performance optimizations (PRAGMA settings).</summary>
        /// <param name="applyOptimizations">When <c>true</c>, applies PRAGMA journal_mode=MEMORY and synchronous=OFF.</param>
        /// <returns>An <see cref="IDbTransaction"/> to be passed to bulk-insert methods.</returns>
        IDbTransaction BeginBulkTransaction(bool applyOptimizations = false);

        /// <summary>Commits the supplied bulk transaction and releases associated resources.</summary>
        /// <param name="transaction">The transaction returned by <see cref="BeginBulkTransaction"/>.</param>
        void CommitBulkTransaction(IDbTransaction transaction);

        /// <summary>Rolls back the supplied bulk transaction.</summary>
        /// <param name="transaction">The transaction to roll back.</param>
        void RollbackBulkTransaction(IDbTransaction transaction);

        /// <summary>Closes the dedicated bulk-insert connection opened by <see cref="BeginBulkTransaction"/>.</summary>
        void CloseBulkConnection();

        /// <summary>Switches the connector into bulk-insert mode, which bypasses normal connection handling for high-throughput inserts.</summary>
        void EnableBulkInsertMode();

        /// <summary>Exits bulk-insert mode and restores normal connection handling.</summary>
        void DisableBulkInsertMode();

        // ── Core Query Execution ───────────────────────────────────────────────

        /// <summary>Executes a non-query SQL statement (INSERT / UPDATE / DELETE) and returns the rows-affected count.</summary>
        /// <param name="sql">The SQL statement to execute.</param>
        /// <param name="ps">Optional named parameters dictionary.</param>
        /// <param name="autoTransact">When <c>true</c>, wraps the statement in an automatic transaction.</param>
        SqlResponse<int> ExecuteNonQuery(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);

        /// <summary>Executes a scalar query and returns the single result value.</summary>
        /// <param name="sql">The SQL SELECT … scalar statement.</param>
        /// <param name="ps">Optional named parameters dictionary.</param>
        /// <param name="autoTransact">When <c>true</c>, wraps the query in an automatic transaction.</param>
        SqlResponse<object> ExecuteScalar(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);

        /// <summary>Executes a SELECT statement and returns results as a <see cref="DataTable"/>.</summary>
        /// <param name="sql">The SQL SELECT statement.</param>
        /// <param name="ps">Optional named parameters dictionary.</param>
        /// <param name="autoTransact">When <c>true</c>, wraps the query in an automatic transaction.</param>
        /// <param name="tableName">Optional name to assign to the returned <see cref="DataTable"/>.</param>
        SqlResponse<DataTable> ExecuteTable(string sql, Dictionary<string, string> ps = null, bool autoTransact = false, string tableName = "");

        /// <summary>Executes a SELECT statement and returns results as a <see cref="DataSet"/> (supports multi-result sets).</summary>
        /// <param name="sql">The SQL SELECT statement.</param>
        /// <param name="ps">Optional named parameters dictionary.</param>
        /// <param name="autoTransact">When <c>true</c>, wraps the query in an automatic transaction.</param>
        SqlResponse<DataSet> ExecuteDataSet(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);

        /// <summary>Executes a SELECT statement and returns the first column of every row as a <see cref="List{String}"/>.</summary>
        /// <param name="sql">The SQL SELECT statement.</param>
        /// <param name="ps">Optional named parameters dictionary.</param>
        /// <param name="autoTransact">When <c>true</c>, wraps the query in an automatic transaction.</param>
        SqlResponse<List<string>> ExecuteColumn(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);

        // ── Schema Management ──────────────────────────────────────────────────

        /// <summary>Validates the database schema to ensure all required tables and columns are present.</summary>
        /// <returns><c>true</c> if the schema is valid; otherwise <c>false</c>.</returns>
        bool ValidateSchema();

        /// <summary>Creates a new table in the database whose structure mirrors the supplied <see cref="DataTable"/>.</summary>
        /// <param name="table">The <see cref="DataTable"/> whose schema to replicate.</param>
        void CreateTableInSQL(DataTable table);

        /// <summary>Drops all indexes on the <c>IconFiles</c> table to accelerate bulk-insert operations.</summary>
        void DropIconFilesIndexes();

        /// <summary>Recreates all indexes on the <c>IconFiles</c> table after bulk inserts complete.</summary>
        void RecreateIconFilesIndexes();

        /// <summary>Drops all indexes on the <c>IconTags</c> table to accelerate bulk-insert operations.</summary>
        void DropIconTagsIndexes();

        /// <summary>Recreates all indexes on the <c>IconTags</c> table after bulk inserts complete.</summary>
        void RecreateIconTagsIndexes();

        // ── Connectivity ───────────────────────────────────────────────────────

        /// <summary>Opens a test connection and verifies the database is accessible.</summary>
        /// <returns><c>true</c> if the connection succeeds; otherwise <c>false</c>.</returns>
        bool TestConnection();

        // ── Logging ────────────────────────────────────────────────────────────

        /// <summary>Creates the <c>SystemLog</c> and <c>SystemExceptions</c> logging tables if they do not already exist.</summary>
        void CreateLogTables();

        /// <summary>Writes a log entry at the specified level.</summary>
        /// <param name="comment">Human-readable description of the event.</param>
        /// <param name="className">Name of the class recording the log entry.</param>
        /// <param name="methodName">Name of the method recording the log entry.</param>
        /// <param name="executor">User or process triggering the event.</param>
        /// <param name="logLevel">Log level string (DEBUG / INFO / WARN / ERROR).</param>
        /// <param name="processType">Category label for the process (e.g. "Import", "Export").</param>
        /// <param name="Ex">Optional exception to attach to the log entry.</param>
        SqlResponse<int> RegisterLog(string comment, string className, string methodName, string executor, string logLevel, string processType, Exception Ex = null);

        /// <summary>Writes a DEBUG-level log entry.</summary>
        SqlResponse<int> Debug(string comment, string className, string methodName, string executor, string processType);

        /// <summary>Writes an INFO-level log entry.</summary>
        SqlResponse<int> Info(string comment, string className, string methodName, string executor, string processType);

        /// <summary>Writes a WARN-level log entry with an optional exception.</summary>
        SqlResponse<int> Warning(string comment, string className, string methodName, string executor, string processType, Exception Ex = null);

        /// <summary>Writes an ERROR-level log entry with an optional exception.</summary>
        SqlResponse<int> Exception(string comment, string className, string methodName, string executor, string processType, Exception Ex = null);

        // ── Icon Operations ────────────────────────────────────────────────────

        /// <summary>Returns the database ID of an icon identified by its vein and filename.</summary>
        /// <param name="Vein">ID of the parent <c>Veins</c> row.</param>
        /// <param name="iconFile">Filename of the icon (without extension).</param>
        SqlResponse<int> GetIconId(int Vein, string iconFile);

        /// <summary>
        /// Registers a new icon or ensures an existing one is present in the <c>Icons</c> table.
        /// Sets <see cref="InsertedId"/> to the icon's database ID after the call.
        /// </summary>
        /// <param name="crudeFileName">Raw filename used as the icon's display name.</param>
        /// <param name="vein">ID of the parent <c>Veins</c> row.</param>
        /// <param name="isImage">1 if the icon has a raster image format; otherwise 0.</param>
        /// <param name="isIcon">1 if the icon has a .ico format; otherwise 0.</param>
        /// <param name="isSvg">1 if the icon has an SVG format; otherwise 0.</param>
        /// <returns><c>true</c> on success; <c>false</c> on failure.</returns>
        bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg);

        /// <summary>
        /// Registers a new icon within an existing bulk transaction for high-throughput imports.
        /// </summary>
        /// <param name="crudeFileName">Raw filename used as the icon's display name.</param>
        /// <param name="vein">ID of the parent <c>Veins</c> row.</param>
        /// <param name="isImage">1 if the icon has a raster image format; otherwise 0.</param>
        /// <param name="isIcon">1 if the icon has a .ico format; otherwise 0.</param>
        /// <param name="isSvg">1 if the icon has an SVG format; otherwise 0.</param>
        /// <param name="transaction">The active bulk transaction.</param>
        bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg, IDbTransaction transaction);

        /// <summary>
        /// Saves a binary icon file to the <c>IconFiles</c> table, performing SHA-256 deduplication.
        /// Skips the insert if a file with the same hash already exists.
        /// </summary>
        /// <param name="iconId">ID of the parent <c>Icons</c> row.</param>
        /// <param name="crudeFileName">Filename without extension.</param>
        /// <param name="extension">File extension including the leading dot.</param>
        /// <param name="type">File type label: "Icon", "SVG", "Image", or "*".</param>
        /// <param name="width">Image width in pixels (used as the size metric).</param>
        /// <param name="originalPath">Original filesystem path of the source file.</param>
        /// <param name="binData">Raw binary content of the file.</param>
        /// <param name="isMerged">1 if this file was created by a merge operation; otherwise 0.</param>
        /// <param name="fileHash">SHA-256 hash of <paramref name="binData"/> for deduplication.</param>
        void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash);

        /// <summary>
        /// Saves a binary icon file within an existing bulk transaction.
        /// </summary>
        void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash, IDbTransaction transaction);

        /// <summary>Returns the <c>IconFiles.Id</c> for a file with the given SHA-256 hash, or -1 if not found.</summary>
        /// <param name="fileHash">SHA-256 hash string to look up.</param>
        int GetIconFileId(string fileHash);

        /// <summary>Returns the parent <c>Icons.Id</c> for a given <c>IconFiles.Id</c>.</summary>
        /// <param name="iconFileId">ID of the <c>IconFiles</c> row.</param>
        int GetIconIdFromIconFileId(int iconFileId);

        /// <summary>Inserts a row into the temporary <c>IconBuffer</c> table during import.</summary>
        /// <param name="id">Keyword ID from the source JSON mapping.</param>
        /// <param name="fileName">Filename associated with this buffer entry.</param>
        bool Icons_Insert(int id, string fileName);

        /// <summary>Returns the buffer ID for an icon file by filename pattern match.</summary>
        /// <param name="iconFile">Filename to search for (LIKE query).</param>
        SqlResponse<int> GetIconBufferId(string iconFile);

        /// <summary>Deletes all icons (and their files and tags) that belong to the specified vein.</summary>
        /// <param name="vein">ID of the <c>Veins</c> row whose icons should be deleted.</param>
        /// <returns><c>true</c> if the cascade delete succeeded; otherwise <c>false</c>.</returns>
        bool DeleteIconsFromVein(int vein);

        // ── Tag Operations ─────────────────────────────────────────────────────

        /// <summary>Adds a single tag to an icon, ignoring duplicates.</summary>
        /// <param name="iconId">ID of the <c>Icons</c> row.</param>
        /// <param name="tag">Tag text to register.</param>
        bool RegisterIconTag(int iconId, string tag);

        /// <summary>Adds multiple tags to an icon in a single database round-trip, optionally within a transaction.</summary>
        /// <param name="iconId">ID of the <c>Icons</c> row.</param>
        /// <param name="tags">List of tag strings to register.</param>
        /// <param name="transaction">Optional transaction to enlist in; pass <c>null</c> for auto-transaction.</param>
        bool RegisterIconTagsBatch(int iconId, List<string> tags, IDbTransaction transaction = null);

        /// <summary>
        /// Performs a high-throughput batch insert of icon/tag pairs using a pre-opened bulk transaction.
        /// Used by the import background worker to avoid per-row transaction overhead.
        /// </summary>
        /// <param name="allTags">List of (iconId, tag) tuples to insert.</param>
        /// <param name="transaction">Active bulk transaction.</param>
        /// <param name="worker">BackgroundWorker used to check for cancellation requests.</param>
        void BulkInsertAllTags(List<(int iconId, string tag)> allTags, IDbTransaction transaction, BackgroundWorker worker);

        /// <summary>Removes a specific tag from an icon.</summary>
        /// <param name="iconId">ID of the <c>Icons</c> row.</param>
        /// <param name="tag">Tag text to remove.</param>
        /// <returns><c>true</c> if the tag was found and removed; otherwise <c>false</c>.</returns>
        bool RemoveTagFromIcon(int iconId, string tag);

        /// <summary>Returns all tags associated with the icon that owns the given icon file.</summary>
        /// <param name="IconFile">Filename (without extension) of the icon to query.</param>
        List<string> GetTagsForIcon(string IconFile);

        /// <summary>Returns all tags associated with a specific <c>IconFiles</c> row.</summary>
        /// <param name="iconFileId">ID of the <c>IconFiles</c> row.</param>
        List<string> GetTagsForIconFileId(int iconFileId);

        /// <summary>Returns every distinct tag stored across all icons in the database.</summary>
        List<string> GetAllTags();

        /// <summary>
        /// Queries the temporary import tables to return the tags mapped to a given icon filename.
        /// Used during JSON-based vein imports.
        /// </summary>
        /// <param name="IconFile">Filename to match in <c>IconBuffer</c>.</param>
        List<string> GetTagsFromTempTablesFor(string IconFile);

        // ── Keyword Buffer (Import Temp Tables) ────────────────────────────────

        /// <summary>Inserts a keyword into the temporary <c>KeywordBuffer</c> table during JSON import.</summary>
        /// <param name="id">Keyword ID from the source JSON mapping.</param>
        /// <param name="word">Keyword text.</param>
        bool Keywords_Insert(int id, string word);

        /// <summary>Links an icon-buffer entry to a keyword-buffer entry in <c>IconKeywordBuffer</c>.</summary>
        /// <param name="iconId">ID in <c>IconBuffer</c>.</param>
        /// <param name="keywordId">ID in <c>KeywordBuffer</c>.</param>
        bool RegisterIconRelationShip(int iconId, int keywordId);

        // ── Buffer Zone ────────────────────────────────────────────────────────

        /// <summary>Creates the <c>BufferZone</c> table if it does not already exist.</summary>
        /// <returns><c>true</c> on success; otherwise <c>false</c>.</returns>
        bool CreateBufferZoneTable();

        /// <summary>Adds an icon file to the buffer zone for the specified project (or the global zone when <paramref name="projectId"/> is <c>null</c>).</summary>
        /// <param name="iconFileId">ID of the <c>IconFiles</c> row to buffer.</param>
        /// <param name="projectId">ID of the active project, or <c>null</c> for the global buffer.</param>
        /// <returns>The new <c>BufferZone.Id</c> on success; a failure response otherwise.</returns>
        SqlResponse<int> BufferZone_Insert(int iconFileId, int? projectId);

        /// <summary>Returns all buffer zone entries for the specified project (or the global zone when <paramref name="projectId"/> is <c>null</c>).</summary>
        /// <param name="projectId">Project filter, or <c>null</c> for the global buffer.</param>
        SqlResponse<DataTable> BufferZone_GetByProject(int? projectId);

        /// <summary>Removes a single entry from the buffer zone by its row ID.</summary>
        /// <param name="bufferZoneId">ID of the <c>BufferZone</c> row to delete.</param>
        SqlResponse<int> BufferZone_Delete(int bufferZoneId);

        /// <summary>Removes all buffer zone entries for a specific icon file and project combination.</summary>
        /// <param name="iconFileId">ID of the <c>IconFiles</c> row.</param>
        /// <param name="projectId">Project filter, or <c>null</c> for the global buffer.</param>
        SqlResponse<int> BufferZone_DeleteByIconFile(int iconFileId, int? projectId);

        /// <summary>Clears every entry in the buffer zone for the specified project (or the global zone).</summary>
        /// <param name="projectId">Project filter, or <c>null</c> for the global buffer.</param>
        SqlResponse<int> BufferZone_ClearForProject(int? projectId);
    }
}
