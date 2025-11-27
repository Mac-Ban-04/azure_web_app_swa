using Azure;
using Azure.Data.Tables;

namespace AzureApiProject;

public class FileLogEntry : ITableEntity
{
    public string PartitionKey { get; set; } = "FileLogs"; 
    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public string FileName { get; set; }
    public long FileSize { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}