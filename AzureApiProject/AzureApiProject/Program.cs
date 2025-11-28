using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        // Konfiguracja BlobServiceClient
        services.AddSingleton(provider => 
            new BlobServiceClient(Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING")));
        
        // Konfiguracja TableClient (Table Storage)
        services.AddSingleton(provider =>
            new TableClient(Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING"), "UploadLogs"));
        
        services.AddSingleton(s => 
        {
            var kvUri = new Uri(Environment.GetEnvironmentVariable("KEY_VAULT_URI"));
            // DefaultAzureCredential automatycznie używa Managed Identity w Azure
            return new SecretClient(kvUri, new DefaultAzureCredential());
        });

        // 2. Rejestracja CosmosClient (Dependency Injection)
        services.AddSingleton(s =>
        {
            var secretClient = s.GetRequiredService<SecretClient>();
            var secretResponse = secretClient.GetSecretAsync("CosmosConnectionString")
                .GetAwaiter().GetResult();
            var connectionString = secretResponse.Value.Value;
            return new CosmosClient(connectionString);
        });
    })
    .Build();

host.Run();

