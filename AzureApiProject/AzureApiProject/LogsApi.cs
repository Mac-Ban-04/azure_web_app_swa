using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureApiProject;

public class UploadLogEntity : ITableEntity
{
    // Identyfikator partycji - używamy stałej wartości dla tej aplikacji
    public string PartitionKey { get; set; } = "LogPartition"; 
    // Identyfikator wiersza - unikalny GUID dla każdego logu
    public string RowKey { get; set; } = System.Guid.NewGuid().ToString(); 
    
    // Dodatkowe dane
    public string FileName { get; set; }
    public string BlobUrl { get; set; }

    // Wymagane przez ITableEntity
    public System.DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }
}

public class LogsApi
{
    private readonly ILogger<LogsApi> _logger;
    private readonly TableClient _tableClient;

    public LogsApi(ILogger<LogsApi> logger, TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
        _tableClient.CreateIfNotExists();
    }

    // --- ENDPOINT 1: GET (Pobieranie listy logów) ---
    // Endpoint: GET /api/logs
    [Function("GetUploadLogs")]
    public IActionResult GetUploadLogs(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "logs")] HttpRequest req)
    {
        _logger.LogInformation("Pobieranie logów przesyłania plików z Table Storage.");

        try
        {
            // Zapytanie do Table Storage: pobierz wszystkie wiersze z danej partycji
            var logs = _tableClient.Query<UploadLogEntity>(filter: $"PartitionKey eq 'LogPartition'");
            
            var logList = new List<object>();
            foreach (var log in logs)
            {
                // Tworzymy anonimowy obiekt do zwrócenia (bez zbędnych pól, jak ETag)
                logList.Add(new {
                    fileName = log.FileName,
                    url = log.BlobUrl,
                    uploadedAt = log.Timestamp?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                });
            }

            return new OkObjectResult(logList);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania logów z Table Storage.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}