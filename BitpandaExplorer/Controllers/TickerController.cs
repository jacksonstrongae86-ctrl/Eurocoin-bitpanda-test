/*
 * ============================================================================
 * TICKER CONTROLLER
 * ============================================================================
 * 
 * Controlador para el endpoint público de precios (Ticker).
 * 
 * Este endpoint NO requiere autenticación, por lo que cualquier
 * usuario puede acceder a los precios actuales.
 * 
 * Demuestra:
 * - Llamadas a API pública
 * - Retorno de JSON para APIs
 * - Uso de async/await
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para visualización de precios.
    /// </summary>
    public class TickerController : Controller
    {
        private readonly IBitpandaService _bitpandaService;
        private readonly ILogger<TickerController> _logger;

        public TickerController(
            IBitpandaService bitpandaService,
            ILogger<TickerController> logger)
        {
            _bitpandaService = bitpandaService;
            _logger = logger;
        }

        /// <summary>
        /// Lista de todos los precios.
        /// URL: /Ticker o /Ticker/Index
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var model = await _bitpandaService.GetTickerAsync();
            return View(model);
        }

        /// <summary>
        /// API JSON para obtener precios.
        /// URL: /Ticker/Api
        /// Útil para llamadas AJAX o integraciones.
        /// </summary>
        public async Task<IActionResult> Api()
        {
            var ticker = await _bitpandaService.GetTickerAsync();
            return Json(ticker);
        }

        /// <summary>
        /// Buscar un activo específico.
        /// URL: /Ticker/Search?symbol=BTC
        /// </summary>
        public async Task<IActionResult> Search(string symbol)
        {
            var ticker = await _bitpandaService.GetTickerAsync();
            
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return View("Index", ticker);
            }

            var filtered = ticker.Items
                .Where(x => x.Symbol.Contains(symbol.ToUpper()))
                .ToList();

            ticker.Items = filtered;
            ViewBag.SearchQuery = symbol;
            
            return View("Index", ticker);
        }
    }
}
