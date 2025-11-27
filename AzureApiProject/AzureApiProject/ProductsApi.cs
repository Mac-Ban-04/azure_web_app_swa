using Microsoft.AspNetCore.Http; // Używamy standardowego ASP.NET Core
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// Model
public class Product
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    
    [JsonProperty("category")]
    public string Category { get; set; }
    public double Price { get; set; }
}

public class ProductsApi
{
    private readonly ILogger<ProductsApi> _logger;
    private readonly Container _container;

    public ProductsApi(ILogger<ProductsApi> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        // Upewnij się, że nazwy bazy i kontenera są takie same jak w Azure Portal
        _container = cosmosClient.GetContainer("ProjectDB", "Products");
    }

    // --- ENDPOINT 1: POST (Dodawanie) ---
    [Function("CreateProduct")]
    public async Task<IActionResult> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequest req)
    {
        _logger.LogInformation("Dodawanie produktu...");

        // 1. Czytanie body (ASP.NET Core style)
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var product = JsonConvert.DeserializeObject<Product>(requestBody);

        // 2. Walidacja
        if (string.IsNullOrEmpty(product?.Name) || string.IsNullOrEmpty(product?.Category))
        {
            return new BadRequestObjectResult("Brak nazwy lub kategorii.");
        }

        // 3. Zapis do Cosmos DB
        await _container.CreateItemAsync(product, new PartitionKey(product.Category));

        // 4. Zwrot OK z obiektem
        return new OkObjectResult(product);
    }

    // --- ENDPOINT 2: GET (Pobieranie) ---
    [Function("GetProducts")]
    public async Task<IActionResult> GetProducts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequest req)
    {
        _logger.LogInformation("Pobieranie produktów...");

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<Product>(query);
        var results = new List<Product>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return new OkObjectResult(results);
    }
}