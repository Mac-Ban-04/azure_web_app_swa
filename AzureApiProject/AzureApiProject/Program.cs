using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System;
using System.Threading.Tasks;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // === 1. KONFIGURACJA STORAGE (BEZPOŚREDNIO Z APLIKACJI) ===
        // To jest OK, ale sprawdź, czy STORAGE_CONNECTION_STRING jest poprawny w SWA
        string storageConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING") 
            ?? throw new InvalidOperationException("Missing STORAGE_CONNECTION_STRING.");

        // BlobServiceClient
        services.AddSingleton(provider => new BlobServiceClient(storageConnectionString));
        
        // TableClient (Table Storage)
        services.AddSingleton(provider => 
            new TableClient(storageConnectionString, "UploadLogs"));

        // === 2. KONFIGURACJA KEY VAULT I COSMOS (ASYNCHRONICZNA/SYNCHRONICZNA) ===
        
        // A. Rejestracja SecretClient
        services.AddSingleton(s => 
        {
            var kvUriString = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            if (string.IsNullOrEmpty(kvUriString) || !Uri.TryCreate(kvUriString, UriKind.Absolute, out var kvUri))
            {
                 throw new InvalidOperationException("KEY_VAULT_URI is missing or invalid.");
            }
            // DefaultAzureCredential używa Managed Identity w Azure
            return new SecretClient(kvUri, new DefaultAzureCredential());
        });

        // B. Rejestracja CosmosClient (Poprawka: używamy Task.Run dla lepszej kompatybilności przy synchronicznym oczekiwaniu)
        services.AddSingleton(s =>
        {
            try
            {
                var secretClient = s.GetRequiredService<SecretClient>();

                // Wymuś wykonanie asynchronicznego pobrania sekretu w osobnym zadaniu
                var connectionString = Task.Run(async () => {
                    var secretResponse = await secretClient.GetSecretAsync("CosmosConnectionString");
                    return secretResponse.Value.Value;
                }).GetAwaiter().GetResult(); // Oczekuj synchronicznie na wynik

                return new CosmosClient(connectionString);
            }
            catch (Exception ex)
            {
                // Musimy zalogować ten błąd, aby pomógł nam w diagnostyce
                Console.WriteLine($"FATAL ERROR during CosmosClient registration: {ex.Message}");
                // Rzucenie wyjątku zatrzyma Host, co jest pożądanym zachowaniem
                throw new InvalidOperationException("Failed to initialize CosmosClient. Check Key Vault URI, secret name, and RBAC permissions.", ex);
            }
        });
    })
    .Build();

host.Run();