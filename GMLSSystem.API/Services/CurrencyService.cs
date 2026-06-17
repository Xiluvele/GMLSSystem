using System.Text.Json;

namespace GMLSSystem.API.Services
{
    public class ExchangeRateResponse
    {
        public string result { get; set; } = string.Empty;
        public string base_code { get; set; } = string.Empty;
        public Dictionary<string, decimal> conversion_rates { get; set; } = new();
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(HttpClient httpClient, IConfiguration configuration, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                var apiKey = _configuration["ExchangeRateApi:Key"] ?? "7129752a54948e90c423986d";
                var baseUrl = _configuration["ExchangeRateApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6/";
                var url = $"{baseUrl}{apiKey}/latest/{fromCurrency}";

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

                _logger.LogWarning("API call failed, using fallback rate");
                return GetFallbackRate(fromCurrency, toCurrency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return GetFallbackRate(fromCurrency, toCurrency);
            }
        }

        public async Task<decimal> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            var rate = await GetExchangeRate(fromCurrency, toCurrency);
            return amount * rate;
        }

        private decimal GetFallbackRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == "USD" && toCurrency == "ZAR") return 18.50m;
            if (fromCurrency == "EUR" && toCurrency == "ZAR") return 20.00m;
            if (fromCurrency == "GBP" && toCurrency == "ZAR") return 23.00m;
            return 1.0m;
        }
    }
}