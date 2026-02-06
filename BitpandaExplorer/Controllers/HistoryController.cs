/*
 * ============================================================================
 * HISTORY CONTROLLER
 * ============================================================================
 * 
 * Controlador para visualizar historial de precios.
 * Usa CoinGecko API para obtener datos históricos.
 * 
 * RUTAS:
 * - /History/{symbol}?days=7&currency=eur — Historial de un activo
 * - /History/Api/{symbol}?days=7 — JSON para AJAX
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para historial de precios.
    /// </summary>
    public class HistoryController : Controller
    {
        private readonly ICoinGeckoService _coinGeckoService;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(
            ICoinGeckoService coinGeckoService,
            ILogger<HistoryController> logger)
        {
            _coinGeckoService = coinGeckoService;
            _logger = logger;
        }

        /// <summary>
        /// Muestra el historial de precios de un activo.
        /// URL: /History/BTC?days=7&currency=eur
        /// </summary>
        /// <param name="symbol">Símbolo del activo (BTC, ETH, etc.)</param>
        /// <param name="days">Días de historial (1, 7, 30, 90, 365)</param>
        /// <param name="currency">Moneda (eur, usd)</param>
        [Route("History/{symbol}")]
        public async Task<IActionResult> Index(string symbol, int days = 7, string currency = "eur")
        {
            // Validar días permitidos
            var allowedDays = new[] { 1, 7, 30, 90, 365 };
            if (!allowedDays.Contains(days))
            {
                days = 7;
            }

            var model = await _coinGeckoService.GetPriceHistoryAsync(symbol, days, currency);
            
            // Pasar opciones a la vista
            ViewBag.AllowedDays = allowedDays;
            ViewBag.Currencies = new[] { "eur", "usd" };
            
            return View(model);
        }

        /// <summary>
        /// Página de selección de activo.
        /// URL: /History
        /// </summary>
        [Route("History")]
        public IActionResult Select()
        {
            // Lista de activos populares para mostrar
            var popularAssets = new[]
            {
                ("BTC", "Bitcoin"),
                ("ETH", "Ethereum"),
                ("XRP", "XRP"),
                ("SOL", "Solana"),
                ("ADA", "Cardano"),
                ("DOGE", "Dogecoin"),
                ("DOT", "Polkadot"),
                ("MATIC", "Polygon"),
                ("LINK", "Chainlink"),
                ("LTC", "Litecoin"),
                ("AVAX", "Avalanche"),
                ("UNI", "Uniswap"),
                ("ATOM", "Cosmos"),
                ("XLM", "Stellar"),
                ("AAVE", "Aave"),
                ("BEST", "Bitpanda Token")
            };

            ViewBag.PopularAssets = popularAssets;
            return View();
        }

        /// <summary>
        /// API JSON para obtener historial.
        /// URL: /History/Api/BTC?days=7
        /// </summary>
        [Route("History/Api/{symbol}")]
        public async Task<IActionResult> Api(string symbol, int days = 7, string currency = "eur")
        {
            var data = await _coinGeckoService.GetPriceHistoryAsync(symbol, days, currency);
            return Json(data);
        }
    }
}
