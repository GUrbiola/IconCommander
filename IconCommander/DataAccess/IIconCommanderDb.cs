using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;

namespace IconCommander.DataAccess
{
    public interface IIconCommanderDb
    {
        string ConnectionString { get; set; }
        string DataBase { get; }
        bool Error { get; }
        bool Executing { get; set; }
        TimeSpan ExecutionLapse { get; }
        Exception LastException { get; set; }
        string LastMessage { get; set; }
        DataSet Results { get; }
        int RowsAffected { get; set; }
        int RowsRead { get; set; }
        string Server { get; }
        int TimeOut { get; set; }
        int InsertedId { get; set; }

        void Initialize(string ConnectionString);
        void AsyncExecuteDataSet(string Query);
        IDbTransaction BeginBulkTransaction(bool applyOptimizations = false);
        void BulkInsertAllTags(List<(int iconId, string tag)> allTags, IDbTransaction transaction, BackgroundWorker worker);
        void CancelExecute();
        void CloseBulkConnection();
        void CommitBulkTransaction(IDbTransaction transaction);
        void CreateLogTables();
        void CreateTableInSQL(DataTable table);
        SqlResponse<int> Debug(string comment, string className, string methodName, string executor, string processType);
        bool DeleteIconsFromVein(int vein);
        void DisableBulkInsertMode();
        void DropIconFilesIndexes();
        void DropIconTagsIndexes();
        void EnableBulkInsertMode();
        SqlResponse<int> Exception(string comment, string className, string methodName, string executor, string processType, Exception Ex = null);
        SqlResponse<List<string>> ExecuteColumn(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);
        SqlResponse<DataSet> ExecuteDataSet(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);
        SqlResponse<int> ExecuteNonQuery(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);
        SqlResponse<object> ExecuteScalar(string sql, Dictionary<string, string> ps = null, bool autoTransact = false);
        SqlResponse<DataTable> ExecuteTable(string sql, Dictionary<string, string> ps = null, bool autoTransact = false, string tableName = "");
        void ExtremeStop();
        SqlResponse<int> GetIconBufferId(string iconFile);
        int GetIconFileId(string fileHash);
        SqlResponse<int> GetIconId(int Vein, string iconFile);
        List<string> GetTagsFromTempTablesFor(string IconFile);
        List<string> GetTagsForIcon(string IconFile);
        bool Icons_Insert(int id, string fileName);
        SqlResponse<int> Info(string comment, string className, string methodName, string executor, string processType);
        bool Keywords_Insert(int id, string word);
        void RecreateIconFilesIndexes();
        void RecreateIconTagsIndexes();
        bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg);
        bool RegisterIcon(string crudeFileName, int vein, int isImage, int isIcon, int isSvg, IDbTransaction transaction);
        bool RegisterIconRelationShip(int iconId, int keywordId);
        bool RegisterIconTag(int iconId, string tag);
        bool RegisterIconTagsBatch(int iconId, List<string> tags, IDbTransaction transaction = null);
        SqlResponse<int> RegisterLog(string comment, string className, string methodName, string executor, string logLevel, string processType, Exception Ex = null);
        void RollbackBulkTransaction(IDbTransaction transaction);
        void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash);
        void SaveIconFile(int iconId, string crudeFileName, string extension, string type, int width, string originalPath, byte[] binData, int isMerged, string fileHash, IDbTransaction transaction);
        bool TestConnection();
        bool ValidateSchema();
        SqlResponse<int> Warning(string comment, string className, string methodName, string executor, string processType, Exception Ex = null);

        // BufferZone methods
        bool CreateBufferZoneTable();
        SqlResponse<int> BufferZone_Insert(int iconFileId, int? projectId);
        SqlResponse<DataTable> BufferZone_GetByProject(int? projectId);
        SqlResponse<int> BufferZone_Delete(int bufferZoneId);
        SqlResponse<int> BufferZone_DeleteByIconFile(int iconFileId, int? projectId);
        SqlResponse<int> BufferZone_ClearForProject(int? projectId);
    }
}