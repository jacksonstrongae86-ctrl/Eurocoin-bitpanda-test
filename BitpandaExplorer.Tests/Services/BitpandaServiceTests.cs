/*
 * ============================================================================
 * TESTS PARA BITPANDA SERVICE
 * ============================================================================
 * 
 * Tests unitarios para el servicio de Bitpanda.
 * 
 * ESTRATEGIA DE TESTING:
 * - Mockear HttpClientFactory para no hacer llamadas reales
 * - Verificar comportamiento de cache
 * - Verificar parsing de respuestas
 * - Verificar manejo de errores
 * ============================================================================
 */

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Tests.Services
{
    /// <summary>
    /// Tests para BitpandaService.
    /// </summary>
    public class BitpandaServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<BitpandaService>> _loggerMock;
        private readonly IMemoryCache _cache;

        public BitpandaServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<BitpandaService>>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            // Configurar mock de IConfiguration para que devuelva null por defecto
            _configurationMock.Setup(x => x[It.IsAny<string>()]).Returns((string?)null);
        }

        private BitpandaService CreateService(string? apiKey = null)
        {
            if (apiKey != null)
            {
                _configurationMock.Setup(x => x["Bitpanda:ApiKey"]).Returns(apiKey);
            }

            return new BitpandaService(
                _httpClientFactoryMock.Object,
                _configurationMock.Object,
                _loggerMock.Object,
                _cache);
        }

        // ====================================================================
        // Tests para HasApiKey
        // ====================================================================

        [Fact]
        public void HasApiKey_WhenNoKeyConfigured_ReturnsFalse()
        {
            // Arrange
            var service = CreateService(null);

            // Act
            var result = service.HasApiKey;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasApiKey_WhenEmptyKeyConfigured_ReturnsFalse()
        {
            // Arrange
            var service = CreateService("");

            // Act
            var result = service.HasApiKey;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasApiKey_WhenWhitespaceKeyConfigured_ReturnsFalse()
        {
            // Arrange
            var service = CreateService("   ");

            // Act
            var result = service.HasApiKey;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasApiKey_WhenValidKeyConfigured_ReturnsTrue()
        {
            // Arrange
            var service = CreateService("valid-api-key-12345");

            // Act
            var result = service.HasApiKey;

            // Assert
            Assert.True(result);
        }

        // ====================================================================
        // Tests para SetApiKey
        // ====================================================================

        [Fact]
        public void SetApiKey_UpdatesHasApiKey()
        {
            // Arrange
            var service = CreateService(null);
            Assert.False(service.HasApiKey);

            // Act
            service.SetApiKey("new-api-key");

            // Assert
            Assert.True(service.HasApiKey);
        }

        // ====================================================================
        // Tests para GetWalletsAsync
        // ====================================================================

        [Fact]
        public async Task GetWalletsAsync_WithoutApiKey_ReturnsError()
        {
            // Arrange
            var service = CreateService(null);

            // Act
            var result = await service.GetWalletsAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.HasApiKey);
            Assert.Contains("no configurada", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        // ====================================================================
        // Tests para GetTradesAsync
        // ====================================================================

        [Fact]
        public async Task GetTradesAsync_WithoutApiKey_ReturnsError()
        {
            // Arrange
            var service = CreateService(null);

            // Act
            var result = await service.GetTradesAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.HasApiKey);
            Assert.Contains("no configurada", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        // ====================================================================
        // Tests para GetTransactionsAsync
        // ====================================================================

        [Fact]
        public async Task GetTransactionsAsync_WithoutApiKey_ReturnsError()
        {
            // Arrange
            var service = CreateService(null);

            // Act
            var result = await service.GetTransactionsAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.HasApiKey);
            Assert.Contains("no configurada", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        // ====================================================================
        // Tests para ValidateApiKeyAsync
        // ====================================================================

        [Fact]
        public async Task ValidateApiKeyAsync_WithoutApiKey_ReturnsFalse()
        {
            // Arrange
            var service = CreateService(null);

            // Act
            var result = await service.ValidateApiKeyAsync();

            // Assert
            Assert.False(result);
        }
    }
}
