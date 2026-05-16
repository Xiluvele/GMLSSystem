using System;
using System.Threading.Tasks;
using GMLSSystem.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net.Http;
using Xunit;

namespace GMLSSystem.Tests.Services
{
    public class CurrencyServiceTests
    {
        private readonly CurrencyService _currencyService;

        public CurrencyServiceTests()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["ExchangeRateApi:Key"]).Returns("test_key");
            mockConfig.Setup(x => x["ExchangeRateApi:BaseUrl"]).Returns("https://test.api.com/");

            _currencyService = new CurrencyService(new HttpClient(), mockConfig.Object);
        }

        [Fact]
        public async Task ConvertCurrency_SameCurrency_ReturnsSameAmount()
        {
            // Arrange
            decimal amount = 100;

            // Act
            var result = await _currencyService.ConvertCurrency(amount, "USD", "USD");

            // Assert
            Assert.Equal(amount, result);
        }

        [Fact]
        public async Task ConvertCurrency_USDToZAR_ReturnsCorrectConversion()
        {
            // Arrange
            decimal usdAmount = 100;
            decimal expectedZAR = 1850; // 100 * 18.50

            // Act
            var result = await _currencyService.ConvertCurrency(usdAmount, "USD", "ZAR");

            // Assert
            Assert.Equal(expectedZAR, result);
        }

        [Fact]
        public async Task ConvertCurrency_ZeroAmount_ReturnsZero()
        {
            // Arrange
            decimal usdAmount = 0;

            // Act
            var result = await _currencyService.ConvertCurrency(usdAmount, "USD", "ZAR");

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetExchangeRate_ValidPair_ReturnsPositiveRate()
        {
            // Act
            var rate = await _currencyService.GetExchangeRate("USD", "ZAR");

            // Assert
            Assert.True(rate > 0);
        }
    }
}