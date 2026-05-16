namespace GMLSSystem.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency);
        Task<decimal> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency);
    }
}
