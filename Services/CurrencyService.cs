using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GMLSSystem.Services
{
    public class ExchangeRateResponse
    {
        public string result { get; set; }
        public string base_code { get; set; }
        public Dictionary<string, decimal> conversion_rates { get; set; }
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public CurrencyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ExchangeRateApi:Key"] ?? "7129752a54948e90c423986d";
            _baseUrl = configuration["ExchangeRateApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6/";
        }

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                var url = $"{_baseUrl}{_apiKey}/latest/{fromCurrency}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var exchangeData = JsonSerializer.Deserialize<ExchangeRateResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (exchangeData?.conversion_rates != null &&
                        exchangeData.conversion_rates.ContainsKey(toCurrency))
                    {
                        return exchangeData.conversion_rates[toCurrency];
                    }
                }

                // Fallback rate if API fails
                return fromCurrency == "USD" && toCurrency == "ZAR" ? 18.50m : 1.0m;
            }
            catch (Exception)
            {
                // Log exception and return fallback rate
                return fromCurrency == "USD" && toCurrency == "ZAR" ? 18.50m : 1.0m;
            }
        }

        public async Task<decimal> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            var rate = await GetExchangeRate(fromCurrency, toCurrency);
            return amount * rate;
        }
    }
}