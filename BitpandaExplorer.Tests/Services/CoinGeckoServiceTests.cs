/*
 * ============================================================================
 * TESTS PARA COINGECKO SERVICE
 * ============================================================================
 * 
 * Tests unitarios para el servicio de CoinGecko.
 * 
 * CONCEPTOS DE TESTING DEMOSTRADOS:
 * - [Fact]: Test sin parámetros
 * - [Theory]: Test con múltiples conjuntos de datos
 * - [InlineData]: Datos para Theory
 * - Assert: Verificaciones
 * - Moq: Mocking de dependencias
 * ============================================================================
 */

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Tests.Services
{
    /// <summary>
    /// Tests para CoinGeckoService.
    /// </summary>
    public class CoinGeckoServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<CoinGeckoService>> _loggerMock;
        private readonly IMemoryCache _cache;
        private readonly CoinGeckoService _service;

        public CoinGeckoServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<CoinGeckoService>>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            
            _service = new CoinGeckoService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object,
                _cache);
        }

        // ====================================================================
        // Tests para GetCoinGeckoId
        // ====================================================================

        [Fact]
        public void GetCoinGeckoId_WithValidSymbol_ReturnsCorrectId()
        {
            // Arrange
            var symbol = "BTC";

            // Act
            var result = _service.GetCoinGeckoId(symbol);

            // Assert
            Assert.Equal("bitcoin", result);
        }

        [Fact]
        public void GetCoinGeckoId_WithLowerCaseSymbol_ReturnsCorrectId()
        {
            // Arrange
            var symbol = "eth";

            // Act
            var result = _service.GetCoinGeckoId(symbol);

            // Assert
            Assert.Equal("ethereum", result);
        }

        [Fact]
        public void GetCoinGeckoId_WithInvalidSymbol_ReturnsNull()
        {
            // Arrange
            var symbol = "INVALID_SYMBOL_12345";

            // Act
            var result = _service.GetCoinGeckoId(symbol);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("BTC", "bitcoin")]
        [InlineData("ETH", "ethereum")]
        [InlineData("SOL", "solana")]
        [InlineData("ADA", "cardano")]
        [InlineData("DOT", "polkadot")]
        [InlineData("DOGE", "dogecoin")]
        [InlineData("BEST", "bitpanda-ecosystem-token")]
        public void GetCoinGeckoId_WithKnownSymbols_ReturnsExpectedIds(
            string symbol, string expectedId)
        {
            // Act
            var result = _service.GetCoinGeckoId(symbol);

            // Assert
            Assert.Equal(expectedId, result);
        }

        // ====================================================================
        // Tests para GetPriceHistoryAsync - Validación de parámetros
        // ====================================================================

        [Fact]
        public async Task GetPriceHistoryAsync_WithInvalidSymbol_ReturnsErrorViewModel()
        {
            // Arrange
            var symbol = "INVALID_XYZ";

            // Act
            var result = await _service.GetPriceHistoryAsync(symbol);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("no soportado", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPriceHistoryAsync_SetsCorrectSymbolInViewModel()
        {
            // Arrange
            var symbol = "btc";

            // Configurar mock para devolver error (no queremos hacer llamada real)
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new FakeHttpMessageHandler()));

            // Act
            var result = await _service.GetPriceHistoryAsync(symbol);

            // Assert
            Assert.Equal("BTC", result.Symbol); // Debe estar en mayúsculas
        }

        [Theory]
        [InlineData("eur", "EUR")]
        [InlineData("usd", "USD")]
        [InlineData("EUR", "EUR")]
        public async Task GetPriceHistoryAsync_NormalizesCurrency(
            string inputCurrency, string expectedCurrency)
        {
            // Arrange
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new FakeHttpMessageHandler()));

            // Act
            var result = await _service.GetPriceHistoryAsync("INVALID", 7, inputCurrency);

            // Assert
            Assert.Equal(expectedCurrency, result.Currency);
        }
    }

    /// <summary>
    /// Handler falso para simular respuestas HTTP en tests.
    /// </summary>
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            // Devolver respuesta vacía por defecto
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}
