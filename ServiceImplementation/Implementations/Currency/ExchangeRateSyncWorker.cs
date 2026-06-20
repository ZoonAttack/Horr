using Entities;
using Entities.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ServiceImplementation.Implementations.Currency
{
    public class ExchangeRateSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExchangeRateSyncWorker> _logger;
        private readonly HttpClient _httpClient;

        public ExchangeRateSyncWorker(IServiceProvider serviceProvider, ILogger<ExchangeRateSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Exchange Rate Sync Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncExchangeRatesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing exchange rates.");
                }

                // Run once every 12 hours
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task SyncExchangeRatesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var apiKey = configuration["ExchangeRateApi:ApiKey"];
            var baseUrl = configuration["ExchangeRateApi:BaseUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogWarning("ExchangeRateApi settings are missing. Skipping sync.");
                return;
            }

            // Using USD as base
            var url = $"{baseUrl}{apiKey}/latest/USD";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch exchange rates. Status code: {response.StatusCode}");
                return;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ExchangeRateResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.Conversion_Rates == null)
            {
                _logger.LogError("Failed to deserialize exchange rate response.");
                return;
            }

            var now = DateTime.UtcNow;

            foreach (var rate in result.Conversion_Rates)
            {
                var currencyCode = rate.Key.ToUpper();
                var exchangeRate = await context.ExchangeRates.FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode, cancellationToken);

                if (exchangeRate == null)
                {
                    exchangeRate = new ExchangeRate
                    {
                        CurrencyCode = currencyCode,
                        Rate = rate.Value,
                        LastUpdated = now
                    };
                    context.ExchangeRates.Add(exchangeRate);
                }
                else
                {
                    exchangeRate.Rate = rate.Value;
                    exchangeRate.LastUpdated = now;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Successfully synced {result.Conversion_Rates.Count} exchange rates.");
        }

        private class ExchangeRateResponse
        {
            public string Result { get; set; }
            public System.Collections.Generic.Dictionary<string, decimal> Conversion_Rates { get; set; }
        }
    }
}
