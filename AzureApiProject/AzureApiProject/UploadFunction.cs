using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureApiProject;

public class UploadFunction
{
    private const string CONTAINER_NAME = "uploads";
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<UploadFunction> _logger;

    public UploadFunction(BlobServiceClient blobServiceClient, ILogger<UploadFunction> logger)
    {
        // Wstrzyknięcie klienta dzięki Dependency Injection
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    [Function("FileUploadApi")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")] HttpRequest req)
    {
        
        // 1. Walidacja
        if (req.ContentLength == 0)
        {
            return new BadRequestObjectResult("Brak danych pliku do przesłania.");
        }
        _logger.LogInformation("Rozpoczęto upload pliku...");
        // 2. Generowanie nazwy i klienta
        var fileName = $"{Guid.NewGuid().ToString()}-{req.Headers["X-File-Name"]}.dat";
        var containerClient = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(fileName);

        // 3. Przesłanie pliku do Blob Storage
        await blobClient.UploadAsync(req.Body, overwrite: true);

        // // 4. Generowanie URL z SAS (dla dostępu na 1h)
        // // Uwaga: Generowanie SAS jest złożone, dlatego podaję uproszczoną ścieżkę.
        // // W prawdziwym projekcie użyj BlobClient.GenerateSasUri(...)
        // // https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-account-sas-create-dotnet
        // var fileUrl = blobClient.Uri.ToString(); 

        _logger.LogInformation($"Plik {fileName} przesłany.");
        
        // 5. Zwracanie sukcesu i ścieżki
        return new OkObjectResult(new 
        {
            Status = "Success",
            FileName = fileName,
            BlobUrl = blobClient.Uri.ToString() 
        });
    }
}