/*
 * ============================================================================
 * BITPANDA SERVICE IMPLEMENTATION
 * ============================================================================
 * 
 * Esta clase implementa IBitpandaService y contiene toda la lógica
 * para interactuar con la API de Bitpanda.
 * 
 * CONCEPTOS CLAVE DE .NET DEMOSTRADOS:
 * 
 * 1. DEPENDENCY INJECTION (DI):
 *    - HttpClient se inyecta via IHttpClientFactory
 *    - IConfiguration se inyecta para acceder a appsettings.json
 *    - ILogger se inyecta para logging
 * 
 * 2. ASYNC/AWAIT:
 *    - Todas las operaciones de red son asíncronas
 *    - Evita bloquear threads durante I/O
 * 
 * 3. HTTPCLIENT:
 *    - Usa IHttpClientFactory (patrón recomendado)
 *    - Maneja headers, JSON serialization, errores
 * 
 * 4. NULL SAFETY:
 *    - Usa nullable reference types (?)
 *    - Null-coalescing (??, ??=)
 *    - Null-conditional (?.))
 * 
 * 5. LINQ:
 *    - Transformación de datos con Select, Where
 *    - Agregaciones con Sum, Count
 * ============================================================================
 */

using System.Net.Http.Headers;
using System.Text.Json;
using BitpandaExplorer.Models.Ticker;
using BitpandaExplorer.Models.Wallet;
using BitpandaExplorer.Models.Trade;
using BitpandaExplorer.Models.Transaction;

namespace BitpandaExplorer.Services
{
    /// <summary>
    /// Implementación del servicio de Bitpanda.
    /// Maneja todas las comunicaciones con la API.
    /// </summary>
    public class BitpandaService : IBitpandaService
    {
        // ====================================================================
        // CAMPOS PRIVADOS
        // ====================================================================

        /// <summary>Factory para crear instancias de HttpClient</summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>Configuración de la aplicación</summary>
        private readonly IConfiguration _configuration;

        /// <summary>Logger para diagnóstico</summary>
        private readonly ILogger<BitpandaService> _logger;

        /// <summary>URL base de la API de Bitpanda (trailing slash requerido)</summary>
        private const string BASE_URL = "https://api.bitpanda.com/v1/";

        /// <summary>API Key actual (puede ser null)</summary>
        private string? _apiKey;

        /// <summary>Cache del ticker para evitar peticiones excesivas</summary>
        private Dictionary<string, AssetPrices>? _tickerCache;
        private DateTime _tickerCacheTime = DateTime.MinValue;
        private readonly TimeSpan _tickerCacheDuration = TimeSpan.FromSeconds(30);

        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// ASP.NET Core automáticamente resuelve estas dependencias.
        /// </summary>
        /// <param name="httpClientFactory">Factory para HttpClient</param>
        /// <param name="configuration">Configuración de appsettings</param>
        /// <param name="logger">Logger</param>
        public BitpandaService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<BitpandaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            // Intentar cargar API Key de configuración
            _apiKey = _configuration["Bitpanda:ApiKey"];
        }

        // ====================================================================
        // PROPIEDADES
        // ====================================================================

        /// <inheritdoc/>
        public bool HasApiKey => !string.IsNullOrWhiteSpace(_apiKey);

        // ====================================================================
        // CONFIGURACIÓN
        // ====================================================================

        /// <inheritdoc/>
        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
            _logger.LogInformation("API Key actualizada");
        }

        // ====================================================================
        // MÉTODOS PRIVADOS - HELPERS
        // ====================================================================

        /// <summary>
        /// Crea un HttpClient configurado para la API de Bitpanda.
        /// </summary>
        /// <param name="authenticated">Si true, añade header de API Key</param>
        private HttpClient CreateClient(bool authenticated = false)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(BASE_URL);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (authenticated && HasApiKey)
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
            }

            return client;
        }

        /// <summary>
        /// Realiza una petición GET y deserializa la respuesta.
        /// </summary>
        /// <typeparam name="T">Tipo de respuesta esperado</typeparam>
        /// <param name="endpoint">Endpoint relativo</param>
        /// <param name="authenticated">Si requiere autenticación</param>
        private async Task<T?> GetAsync<T>(string endpoint, bool authenticated = false) 
            where T : class
        {
            try
            {
                using var client = CreateClient(authenticated);
                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "API request failed: {StatusCode} - {Endpoint}",
                        response.StatusCode, endpoint);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Bitpanda API: {Endpoint}", endpoint);
                return null;
            }
        }

        /// <summary>
        /// Parsea un string a decimal de forma segura.
        /// </summary>
        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            return decimal.TryParse(value, 
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, 
                out var result) ? result : 0;
        }

        /// <summary>
        /// Parsea timestamp Unix a DateTime.
        /// </summary>
        private static DateTime ParseUnixTime(string? unix)
        {
            if (string.IsNullOrWhiteSpace(unix)) return DateTime.UtcNow;
            if (long.TryParse(unix, out var timestamp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
            return DateTime.UtcNow;
        }

        // ====================================================================
        // TICKER (PÚBLICO)
        // ====================================================================

        /// <inheritdoc/>
        public async Task<TickerViewModel> GetTickerAsync()
        {
            var viewModel = new TickerViewModel();

            try
            {
                // El ticker devuelve un Dictionary<string, Dictionary<string, string>>
                using var client = CreateClient(authenticated: false);
                var response = await client.GetAsync("ticker");

                if (!response.IsSuccessStatusCode)
                {
                    viewModel.ErrorMessage = $"Error: {response.StatusCode}";
                    return viewModel;
                }

                var json = await response.Content.ReadAsStringAsync();
                var ticker = JsonSerializer.Deserialize<Dictionary<string, AssetPrices>>(json);

                if (ticker == null)
                {
                    viewModel.ErrorMessage = "No se pudo parsear la respuesta";
                    return viewModel;
                }

                // Actualizar cache
                _tickerCache = ticker;
                _tickerCacheTime = DateTime.UtcNow;

                // Convertir a lista de items
                viewModel.Items = ticker
                    .Select(kvp => new TickerItem
                    {
                        Symbol = kvp.Key,
                        PriceEUR = ParseDecimal(kvp.Value.EUR),
                        PriceUSD = ParseDecimal(kvp.Value.USD),
                        Prices = kvp.Value
                    })
                    .OrderByDescending(x => x.PriceEUR)
                    .ToList();

                viewModel.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ticker");
                viewModel.ErrorMessage = ex.Message;
            }

            return viewModel;
        }

        /// <summary>
        /// Obtiene el precio EUR de un símbolo desde el cache o API.
        /// </summary>
        private async Task<decimal> GetPriceEURAsync(string symbol)
        {
            // Refrescar cache si expiró
            if (_tickerCache == null || 
                DateTime.UtcNow - _tickerCacheTime > _tickerCacheDuration)
            {
                await GetTickerAsync();
            }

            if (_tickerCache != null && 
                _tickerCache.TryGetValue(symbol, out var prices))
            {
                return ParseDecimal(prices.EUR);
            }

            return 0;
        }

        // ====================================================================
        // WALLETS (REQUIERE AUTH)
        // ====================================================================

        /// <inheritdoc/>
        public async Task<WalletsViewModel> GetWalletsAsync()
        {
            var viewModel = new WalletsViewModel
            {
                HasApiKey = HasApiKey
            };

            if (!HasApiKey)
            {
                viewModel.ErrorMessage = "API Key no configurada";
                return viewModel;
            }

            try
            {
                // Obtener ambos tipos de wallets en paralelo
                var cryptoTask = GetCryptoWalletsAsync();
                var fiatTask = GetFiatWalletsAsync();
                var tickerTask = GetTickerAsync();

                await Task.WhenAll(cryptoTask, fiatTask, tickerTask);

                viewModel.CryptoWallets = await cryptoTask;
                viewModel.FiatWallets = await fiatTask;

                // Calcular valores en EUR
                foreach (var wallet in viewModel.CryptoWallets)
                {
                    var priceEUR = await GetPriceEURAsync(wallet.Symbol);
                    wallet.ValueEUR = wallet.Balance * priceEUR;
                }

                foreach (var wallet in viewModel.FiatWallets)
                {
                    // Para EUR, el valor es el balance directamente
                    // Para otros fiat, habría que convertir (simplificamos a EUR)
                    wallet.ValueEUR = wallet.Symbol == "EUR" ? wallet.Balance : 0;
                }

                // Calcular total
                viewModel.TotalValueEUR = 
                    viewModel.CryptoWallets.Sum(w => w.ValueEUR ?? 0) +
                    viewModel.FiatWallets.Sum(w => w.ValueEUR ?? 0);

                viewModel.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching wallets");
                viewModel.ErrorMessage = ex.Message;
            }

            return viewModel;
        }

        /// <inheritdoc/>
        public async Task<List<WalletDisplayItem>> GetCryptoWalletsAsync()
        {
            var response = await GetAsync<CryptoWalletsResponse>("wallets", authenticated: true);
            
            if (response?.Data == null) return new List<WalletDisplayItem>();

            return response.Data
                .Where(w => w.Attributes != null && !w.Attributes.Deleted)
                .Select(w => new WalletDisplayItem
                {
                    Id = w.Id,
                    WalletType = "crypto",
                    Symbol = w.Attributes!.CryptocoinSymbol,
                    Name = w.Attributes.Name,
                    Balance = ParseDecimal(w.Attributes.Balance),
                    PendingTransactions = w.Attributes.PendingTransactionsCount,
                    IsDefault = w.Attributes.IsDefault
                })
                .Where(w => w.Balance > 0) // Solo wallets con balance
                .OrderByDescending(w => w.Balance)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<WalletDisplayItem>> GetFiatWalletsAsync()
        {
            var response = await GetAsync<FiatWalletsResponse>("fiatwallets", authenticated: true);
            
            if (response?.Data == null) return new List<WalletDisplayItem>();

            return response.Data
                .Where(w => w.Attributes != null)
                .Select(w => new WalletDisplayItem
                {
                    Id = w.Id,
                    WalletType = "fiat",
                    Symbol = w.Attributes!.FiatSymbol,
                    Name = w.Attributes.Name,
                    Balance = ParseDecimal(w.Attributes.Balance),
                    PendingTransactions = w.Attributes.PendingTransactionsCount,
                    IsDefault = false
                })
                .OrderByDescending(w => w.Balance)
                .ToList();
        }

        // ====================================================================
        // TRADES (REQUIERE AUTH)
        // ====================================================================

        /// <inheritdoc/>
        public async Task<TradesViewModel> GetTradesAsync(
            string? type = null,
            string? cursor = null,
            int pageSize = 25)
        {
            var viewModel = new TradesViewModel
            {
                HasApiKey = HasApiKey,
                TypeFilter = type,
                PageSize = pageSize
            };

            if (!HasApiKey)
            {
                viewModel.ErrorMessage = "API Key no configurada";
                return viewModel;
            }

            try
            {
                // Construir query string
                var queryParams = new List<string> { $"page_size={pageSize}" };
                if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={type}");
                if (!string.IsNullOrEmpty(cursor)) queryParams.Add($"cursor={cursor}");
                
                var endpoint = $"trades?{string.Join("&", queryParams)}";
                var response = await GetAsync<TradesResponse>(endpoint, authenticated: true);

                if (response?.Data == null)
                {
                    viewModel.ErrorMessage = "No se pudieron obtener los trades";
                    return viewModel;
                }

                viewModel.Trades = response.Data
                    .Where(t => t.Attributes != null)
                    .Select(t => new TradeDisplayItem
                    {
                        Id = t.Id,
                        Type = t.Attributes!.TradeType,
                        Status = t.Attributes.Status,
                        CryptoAmount = ParseDecimal(t.Attributes.AmountCryptocoin),
                        FiatAmount = ParseDecimal(t.Attributes.AmountFiat),
                        Price = ParseDecimal(t.Attributes.Price),
                        IsSwap = t.Attributes.IsSwap,
                        Date = t.Attributes.Time != null 
                            ? ParseUnixTime(t.Attributes.Time.Unix)
                            : DateTime.UtcNow
                    })
                    .ToList();

                viewModel.TotalCount = response.Meta?.TotalCount ?? viewModel.Trades.Count;
                viewModel.NextCursor = response.Meta?.NextCursor;
                viewModel.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trades");
                viewModel.ErrorMessage = ex.Message;
            }

            return viewModel;
        }

        // ====================================================================
        // TRANSACTIONS (REQUIERE AUTH)
        // ====================================================================

        /// <inheritdoc/>
        public async Task<List<TransactionDisplayItem>> GetCryptoTransactionsAsync(
            string? type = null,
            string? status = null,
            string? cursor = null,
            int pageSize = 25)
        {
            var queryParams = new List<string> { $"page_size={pageSize}" };
            if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={type}");
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
            if (!string.IsNullOrEmpty(cursor)) queryParams.Add($"cursor={cursor}");

            var endpoint = $"wallets/transactions?{string.Join("&", queryParams)}";
            var response = await GetAsync<CryptoTransactionsResponse>(endpoint, authenticated: true);

            if (response?.Data == null) return new List<TransactionDisplayItem>();

            return response.Data
                .Where(t => t.Attributes != null)
                .Select(t => new TransactionDisplayItem
                {
                    Id = t.Id,
                    Category = "crypto",
                    Type = t.Attributes!.TransactionType,
                    Status = t.Attributes.Status,
                    Symbol = t.Attributes.CryptocoinSymbol,
                    Amount = ParseDecimal(t.Attributes.Amount),
                    Fee = !string.IsNullOrEmpty(t.Attributes.Fee) 
                        ? ParseDecimal(t.Attributes.Fee) : null,
                    TxHash = t.Attributes.TxId,
                    Confirmations = t.Attributes.Confirmations,
                    Date = t.Attributes.Time != null 
                        ? ParseUnixTime(t.Attributes.Time.Unix) 
                        : DateTime.UtcNow
                })
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<TransactionDisplayItem>> GetFiatTransactionsAsync(
            string? type = null,
            string? status = null,
            string? cursor = null,
            int pageSize = 25)
        {
            var queryParams = new List<string> { $"page_size={pageSize}" };
            if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={type}");
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
            if (!string.IsNullOrEmpty(cursor)) queryParams.Add($"cursor={cursor}");

            var endpoint = $"fiatwallets/transactions?{string.Join("&", queryParams)}";
            var response = await GetAsync<FiatTransactionsResponse>(endpoint, authenticated: true);

            if (response?.Data == null) return new List<TransactionDisplayItem>();

            return response.Data
                .Where(t => t.Attributes != null)
                .Select(t => new TransactionDisplayItem
                {
                    Id = t.Id,
                    Category = "fiat",
                    Type = t.Attributes!.TransactionType,
                    Status = t.Attributes.Status,
                    Symbol = "EUR", // Simplificado
                    Amount = ParseDecimal(t.Attributes.Amount),
                    Fee = !string.IsNullOrEmpty(t.Attributes.Fee) 
                        ? ParseDecimal(t.Attributes.Fee) : null,
                    Date = t.Attributes.Time != null 
                        ? ParseUnixTime(t.Attributes.Time.Unix) 
                        : DateTime.UtcNow
                })
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<TransactionsViewModel> GetTransactionsAsync(
            string category = "all",
            string? type = null,
            string? status = null,
            int pageSize = 25)
        {
            var viewModel = new TransactionsViewModel
            {
                HasApiKey = HasApiKey,
                CategoryFilter = category,
                TypeFilter = type,
                StatusFilter = status,
                PageSize = pageSize
            };

            if (!HasApiKey)
            {
                viewModel.ErrorMessage = "API Key no configurada";
                return viewModel;
            }

            try
            {
                var transactions = new List<TransactionDisplayItem>();

                if (category == "all" || category == "crypto")
                {
                    var cryptoTxs = await GetCryptoTransactionsAsync(type, status, null, pageSize);
                    transactions.AddRange(cryptoTxs);
                }

                if (category == "all" || category == "fiat")
                {
                    var fiatTxs = await GetFiatTransactionsAsync(type, status, null, pageSize);
                    transactions.AddRange(fiatTxs);
                }

                viewModel.Transactions = transactions
                    .OrderByDescending(t => t.Date)
                    .Take(pageSize)
                    .ToList();

                viewModel.TotalCount = viewModel.Transactions.Count;
                viewModel.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions");
                viewModel.ErrorMessage = ex.Message;
            }

            return viewModel;
        }

        // ====================================================================
        // UTILIDADES
        // ====================================================================

        /// <inheritdoc/>
        public async Task<decimal> CalculateTotalPortfolioValueAsync()
        {
            var wallets = await GetWalletsAsync();
            return wallets.TotalValueEUR;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateApiKeyAsync()
        {
            if (!HasApiKey) return false;

            try
            {
                var wallets = await GetAsync<CryptoWalletsResponse>("wallets", authenticated: true);
                return wallets != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
