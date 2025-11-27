//README: funkcja z zadania pierwszego, niepotrzebna teraz do projektu

// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.Logging;
//
// namespace AzureApiProject;
// public class GreetingApi
// {
//     private readonly ILogger<GreetingApi> _logger;
//
//     public GreetingApi(ILogger<GreetingApi> logger)
//     {
//         _logger = logger;
//     }
//
//     [Function("GetGreeting")]
//     public IActionResult Run(
//         [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
//     {
//         _logger.LogInformation("Przetwarzanie zapytania HTTP...");
//
//         // 1. Pobieranie parametru 'name' z Query String
//         string? name = req.Query["name"];
//
//         // 2. Prosta walidacja
//         if (string.IsNullOrWhiteSpace(name))
//         {
//             // Zwracamy kod 400 Bad Request
//             return new BadRequestObjectResult(new 
//             { 
//                 Error = "Brakuje parametru name.",
//                 Usage = "?name=TwojeImie"
//             });
//         }
//
//         // 3. Tworzenie obiektu odpowiedzi (anonimowy typ)
//         var responseData = new
//         {
//             Message = $"Cześć, {name}!",
//             Timestamp = DateTime.UtcNow,
//             ProcessingNode = System.Environment.MachineName // Ciekawostka: nazwa serwera w chmurze
//         };
//
//         // 4. Zwracamy kod 200 OK z obiektem (automatyczna serializacja do JSON)
//         return new OkObjectResult(responseData);
//     }
// }