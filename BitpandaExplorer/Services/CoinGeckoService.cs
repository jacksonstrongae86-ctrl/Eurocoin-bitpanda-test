/*
 * ============================================================================
 * COINGECKO SERVICE IMPLEMENTATION
 * ============================================================================
 * 
 * Implementación del servicio de CoinGecko para datos históricos.
 * 
 * API BASE: https://api.coingecko.com/api/v3
 * 
 * LÍMITES (sin API key):
 * - 10-30 llamadas por minuto
 * - Datos con ligero retraso
 * 
 * MAPEO DE SÍMBOLOS:
 * Bitpanda usa símbolos (BTC), CoinGecko usa IDs (bitcoin).
 * Mantenemos un diccionario de mapeo para las monedas principales.
 * ============================================================================
 */

using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using BitpandaExplorer.Models.History;

namespace BitpandaExplorer.Services
{
    /// <summary>
    /// Servicio para obtener datos históricos de CoinGecko.
    /// </summary>
    public class CoinGeckoService : ICoinGeckoService
    {
        // ====================================================================
        // CAMPOS PRIVADOS
        // ====================================================================

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CoinGeckoService> _logger;
        private readonly IMemoryCache _cache;
        private const string BASE_URL = "https://api.coingecko.com/api/v3/";
        
        // Cache durations varían según el período solicitado
        private static TimeSpan GetCacheDuration(int days) => days switch
        {
            1 => TimeSpan.FromMinutes(5),      // 24h data: cache 5 min
            7 => TimeSpan.FromMinutes(15),     // 7 days: cache 15 min
            30 => TimeSpan.FromMinutes(30),    // 30 days: cache 30 min
            _ => TimeSpan.FromHours(1)         // 90+ days: cache 1 hour
        };

        /// <summary>
        /// Mapeo de símbolos de Bitpanda a IDs de CoinGecko.
        /// CoinGecko usa identificadores únicos en lugar de símbolos.
        /// </summary>
        private static readonly Dictionary<string, (string Id, string Name)> SymbolMapping = new()
        {
            // Criptomonedas principales
            { "BTC", ("bitcoin", "Bitcoin") },
            { "ETH", ("ethereum", "Ethereum") },
            { "XRP", ("ripple", "XRP") },
            { "ADA", ("cardano", "Cardano") },
            { "SOL", ("solana", "Solana") },
            { "DOT", ("polkadot", "Polkadot") },
            { "DOGE", ("dogecoin", "Dogecoin") },
            { "SHIB", ("shiba-inu", "Shiba Inu") },
            { "MATIC", ("matic-network", "Polygon") },
            { "LTC", ("litecoin", "Litecoin") },
            { "LINK", ("chainlink", "Chainlink") },
            { "UNI", ("uniswap", "Uniswap") },
            { "AVAX", ("avalanche-2", "Avalanche") },
            { "XLM", ("stellar", "Stellar") },
            { "ATOM", ("cosmos", "Cosmos") },
            { "ETC", ("ethereum-classic", "Ethereum Classic") },
            { "XMR", ("monero", "Monero") },
            { "BCH", ("bitcoin-cash", "Bitcoin Cash") },
            { "ALGO", ("algorand", "Algorand") },
            { "VET", ("vechain", "VeChain") },
            { "FIL", ("filecoin", "Filecoin") },
            { "AAVE", ("aave", "Aave") },
            { "SAND", ("the-sandbox", "The Sandbox") },
            { "MANA", ("decentraland", "Decentraland") },
            { "AXS", ("axie-infinity", "Axie Infinity") },
            { "APE", ("apecoin", "ApeCoin") },
            { "CRO", ("crypto-com-chain", "Cronos") },
            { "NEAR", ("near", "NEAR Protocol") },
            { "TRX", ("tron", "TRON") },
            { "EOS", ("eos", "EOS") },
            { "BEST", ("bitpanda-ecosystem-token", "Bitpanda Ecosystem Token") },
            { "PAN", ("pantos", "Pantos") },
            // Stablecoins
            { "USDT", ("tether", "Tether") },
            { "USDC", ("usd-coin", "USD Coin") },
            { "DAI", ("dai", "Dai") },
            // Metales (CoinGecko también tiene algunos)
            { "XAU", ("pax-gold", "Gold (PAXG)") },
            { "XAG", ("silver-token", "Silver") },
        };

        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        public CoinGeckoService(
            IHttpClientFactory httpClientFactory,
            ILogger<CoinGeckoService> logger,
            IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cache = cache;
        }

        // ====================================================================
        // MÉTODOS PÚBLICOS
        // ====================================================================

        /// <inheritdoc/>
        public string? GetCoinGeckoId(string symbol)
        {
            var upperSymbol = symbol.ToUpperInvariant();
            return SymbolMapping.TryGetValue(upperSymbol, out var info) ? info.Id : null;
        }

        /// <inheritdoc/>
        public async Task<PriceHistoryViewModel> GetPriceHistoryAsync(
            string symbol, 
            int days = 7, 
            string currency = "eur")
        {
            // Crear cache key única para esta combinación
            var cacheKey = $"coingecko_history_{symbol.ToUpperInvariant()}_{days}_{currency.ToLowerInvariant()}";
            
            // Intentar obtener del cache
            if (_cache.TryGetValue(cacheKey, out PriceHistoryViewModel? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Price history for {Symbol} obtained from cache", symbol);
                return cachedResult;
            }
            
            var viewModel = new PriceHistoryViewModel
            {
                Symbol = symbol.ToUpperInvariant(),
                Currency = currency.ToUpperInvariant(),
                Days = days
            };

            try
            {
                _logger.LogInformation("Fetching price history for {Symbol} from CoinGecko (cache miss)", symbol);
                
                // Obtener el ID de CoinGecko
                var upperSymbol = symbol.ToUpperInvariant();
                if (!SymbolMapping.TryGetValue(upperSymbol, out var coinInfo))
                {
                    viewModel.ErrorMessage = $"Símbolo '{symbol}' no soportado para historial";
                    return viewModel;
                }

                viewModel.Name = coinInfo.Name;

                // Llamar a la API de CoinGecko
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(BASE_URL);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                // User-Agent requerido por CoinGecko para evitar 403
                client.DefaultRequestHeaders.Add("User-Agent", "BitpandaExplorer/1.0");

                var endpoint = $"coins/{coinInfo.Id}/market_chart?vs_currency={currency.ToLower()}&days={days}";
                
                _logger.LogInformation("Fetching price history: {Endpoint}", endpoint);
                
                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    viewModel.ErrorMessage = $"Error de CoinGecko: {response.StatusCode}";
                    _logger.LogWarning("CoinGecko API error: {StatusCode}", response.StatusCode);
                    return viewModel;
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<CoinGeckoMarketChartResponse>(json, options);

                if (data?.Prices == null || !data.Prices.Any())
                {
                    viewModel.ErrorMessage = "No hay datos de precios disponibles";
                    return viewModel;
                }

                // Convertir los datos
                viewModel.Prices = data.Prices
                    .Where(p => p.Count >= 2)
                    .Select(p => new PricePoint
                    {
                        TimestampMs = (long)p[0],
                        Date = DateTimeOffset.FromUnixTimeMilliseconds((long)p[0]).DateTime,
                        Price = p[1]
                    })
                    .OrderBy(p => p.Date)
                    .ToList();

                // Calcular estadísticas
                if (viewModel.Prices.Any())
                {
                    viewModel.CurrentPrice = viewModel.Prices.Last().Price;
                    viewModel.HighPrice = viewModel.Prices.Max(p => p.Price);
                    viewModel.LowPrice = viewModel.Prices.Min(p => p.Price);
                    
                    var firstPrice = viewModel.Prices.First().Price;
                    if (firstPrice > 0)
                    {
                        viewModel.ChangePercent = ((viewModel.CurrentPrice - firstPrice) / firstPrice) * 100;
                    }
                }

                viewModel.LastUpdated = DateTime.UtcNow;
                
                // Guardar en cache con duración apropiada según el período
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(GetCacheDuration(days));
                _cache.Set(cacheKey, viewModel, cacheOptions);
                
                _logger.LogInformation("Cached {Count} price points for {Symbol} ({Days}d, expires in {Duration})", 
                    viewModel.Prices.Count, symbol, days, GetCacheDuration(days));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price history for {Symbol}", symbol);
                viewModel.ErrorMessage = $"Error: {ex.Message}";
            }

            return viewModel;
        }
    }
}
