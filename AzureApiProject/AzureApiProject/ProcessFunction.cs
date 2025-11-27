using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;


namespace AzureApiProject;

public class ProcessFunction
{
    private readonly TableClient _tableClient;
    private readonly ILogger<ProcessFunction> _logger;

    public ProcessFunction(TableClient tableClient, ILogger<ProcessFunction> logger)
    {
        _tableClient = tableClient;
        _logger = logger;
    }

    [Function("ProcessNewBlob")]
    public async Task Run(
        // Wyzwalacz: gdy plik pojawi się w kontenerze 'uploads'
        [BlobTrigger("uploads/{name}", Connection = "STORAGE_CONNECTION_STRING")] Stream stream, 
        string name) // Nazwa pliku (uzyskana z {name}))
    {
        await _tableClient.CreateIfNotExistsAsync();
        
        _logger.LogInformation($"Przetwarzanie pliku {name}...");
        // 1. Tworzenie encji logu
        var logEntry = new FileLogEntry
        {
            FileName = name,
            FileSize = stream.Length,
            Timestamp = DateTimeOffset.UtcNow
        };

        // 2. Dodanie encji do Table Storage
        await _tableClient.AddEntityAsync(logEntry);

        _logger.LogInformation($"Zalogowano przetworzenie pliku: {name}. Rozmiar: {stream.Length} bajtów.");
    }
}