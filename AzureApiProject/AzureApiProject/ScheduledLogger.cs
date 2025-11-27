using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace AzureApiProject;

public class ScheduledLogger
{
    private readonly ILogger<ScheduledLogger> _logger;
    
    // Wstrzyknięcie logera do konstruktora (teraz już wiesz jak to działa!)
    public ScheduledLogger(ILogger<ScheduledLogger> logger)
    {
        _logger = logger;
    }

    [Function("ScheduleLogFunction")]
    public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        // 1. Definicja harmonogramu (Cron Expression)
        // "0 */1 * * * *" oznacza: uruchom co 1 minutę.

        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        
        if (myTimer.IsPastDue)
        {
            _logger.LogWarning("Timer is running late!");
        }
    }
}