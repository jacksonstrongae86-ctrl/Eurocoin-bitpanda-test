/*
 * ============================================================================
 * TRADES CONTROLLER
 * ============================================================================
 * 
 * Controlador para historial de operaciones de compra/venta.
 * 
 * REQUIERE API KEY para funcionar.
 * 
 * Demuestra:
 * - Paginación con cursor
 * - Filtros en consultas
 * - Query strings en ASP.NET
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para historial de trades.
    /// </summary>
    public class TradesController : Controller
    {
        private readonly IBitpandaService _bitpandaService;
        private readonly ILogger<TradesController> _logger;

        public TradesController(
            IBitpandaService bitpandaService,
            ILogger<TradesController> logger)
        {
            _bitpandaService = bitpandaService;
            _logger = logger;
        }

        /// <summary>
        /// Lista de trades con filtros y paginación.
        /// URL: /Trades?type=buy&cursor=xxx&pageSize=25
        /// </summary>
        /// <param name="type">Filtro: "buy" o "sell"</param>
        /// <param name="cursor">Cursor de paginación</param>
        /// <param name="pageSize">Resultados por página</param>
        public async Task<IActionResult> Index(
            string? type = null, 
            string? cursor = null, 
            int pageSize = 25)
        {
            var model = await _bitpandaService.GetTradesAsync(type, cursor, pageSize);
            return View(model);
        }

        /// <summary>
        /// Solo trades de compra.
        /// URL: /Trades/Buys
        /// </summary>
        public async Task<IActionResult> Buys(string? cursor = null, int pageSize = 25)
        {
            var model = await _bitpandaService.GetTradesAsync("buy", cursor, pageSize);
            return View("Index", model);
        }

        /// <summary>
        /// Solo trades de venta.
        /// URL: /Trades/Sells
        /// </summary>
        public async Task<IActionResult> Sells(string? cursor = null, int pageSize = 25)
        {
            var model = await _bitpandaService.GetTradesAsync("sell", cursor, pageSize);
            return View("Index", model);
        }

        /// <summary>
        /// API JSON para trades.
        /// URL: /Trades/Api?type=buy&pageSize=50
        /// </summary>
        public async Task<IActionResult> Api(
            string? type = null,
            string? cursor = null,
            int pageSize = 25)
        {
            if (!_bitpandaService.HasApiKey)
            {
                return Unauthorized(new { error = "API Key no configurada" });
            }

            var trades = await _bitpandaService.GetTradesAsync(type, cursor, pageSize);
            return Json(trades);
        }
    }
}
