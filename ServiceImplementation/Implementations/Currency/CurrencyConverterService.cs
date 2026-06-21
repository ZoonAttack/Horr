using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.Currency;
using System;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Currency
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly AppDbContext _context;

        public CurrencyConverterService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency)) fromCurrency = "USD";
            if (string.IsNullOrWhiteSpace(toCurrency)) toCurrency = "USD";

            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
                return amount;

            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            return amount * rate;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency)) fromCurrency = "USD";
            if (string.IsNullOrWhiteSpace(toCurrency)) toCurrency = "USD";

            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
                return 1.0m;

            var fromRate = await _context.ExchangeRates.FirstOrDefaultAsync(r => r.CurrencyCode == fromCurrency.ToUpper());
            var toRate = await _context.ExchangeRates.FirstOrDefaultAsync(r => r.CurrencyCode == toCurrency.ToUpper());

            if (fromRate == null || toRate == null)
            {
                // Fallback or error handling
                // Assuming base is USD, if one is missing, we might have a problem.
                // For now, if missing, throw or return 1.0
                throw new InvalidOperationException($"Exchange rate not found for {fromCurrency} or {toCurrency}");
            }

            // Cross rate: (1 / fromRate.Rate) * toRate.Rate
            // E.g., USD -> EGP. 
            // If base is USD. USD rate = 1.0. EGP rate = 47.5.
            // USD -> EGP: (1 / 1.0) * 47.5 = 47.5
            // EGP -> USD: (1 / 47.5) * 1.0 = 0.021

            return (1.0m / fromRate.Rate) * toRate.Rate;
        }
    }
}
