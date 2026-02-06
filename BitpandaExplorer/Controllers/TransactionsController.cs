/*
 * ============================================================================
 * TRANSACTIONS CONTROLLER
 * ============================================================================
 * 
 * Controlador para historial de transacciones (depósitos, retiros, etc.).
 * 
 * REQUIERE API KEY para funcionar.
 * 
 * Demuestra:
 * - Múltiples filtros combinados
 * - Unión de datos de diferentes endpoints
 * - Enums y valores válidos
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Services;
using BitpandaExplorer.Models.Transaction;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para historial de transacciones.
    /// </summary>
    public class TransactionsController : Controller
    {
        private readonly IBitpandaService _bitpandaService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            IBitpandaService bitpandaService,
            ILogger<TransactionsController> logger)
        {
            _bitpandaService = bitpandaService;
            _logger = logger;
        }

        /// <summary>
        /// Lista de transacciones con filtros.
        /// URL: /Transactions?category=crypto&type=deposit&status=finished
        /// </summary>
        /// <param name="category">Categoría: "all", "crypto", "fiat"</param>
        /// <param name="type">Tipo: buy, sell, deposit, withdrawal, etc.</param>
        /// <param name="status">Estado: pending, finished, canceled, etc.</param>
        /// <param name="pageSize">Resultados por página</param>
        public async Task<IActionResult> Index(
            string category = "all",
            string? type = null,
            string? status = null,
            int pageSize = 25)
        {
            var model = await _bitpandaService.GetTransactionsAsync(
                category, type, status, pageSize);

            // Pasar opciones de filtro a la vista
            ViewBag.AvailableTypes = TransactionsViewModel.AvailableTypes;
            ViewBag.AvailableStatuses = TransactionsViewModel.AvailableStatuses;

            return View(model);
        }

        /// <summary>
        /// Solo transacciones crypto.
        /// URL: /Transactions/Crypto
        /// </summary>
        public async Task<IActionResult> Crypto(
            string? type = null,
            string? status = null,
            int pageSize = 25)
        {
            var model = await _bitpandaService.GetTransactionsAsync(
                "crypto", type, status, pageSize);

            ViewBag.AvailableTypes = TransactionsViewModel.AvailableTypes;
            ViewBag.AvailableStatuses = TransactionsViewModel.AvailableStatuses;

            return View("Index", model);
        }

        /// <summary>
        /// Solo transacciones fiat.
        /// URL: /Transactions/Fiat
        /// </summary>
        public async Task<IActionResult> Fiat(
            string? type = null,
            string? status = null,
            int pageSize = 25)
        {
            var model = await _bitpandaService.GetTransactionsAsync(
                "fiat", type, status, pageSize);

            ViewBag.AvailableTypes = TransactionsViewModel.AvailableTypes;
            ViewBag.AvailableStatuses = TransactionsViewModel.AvailableStatuses;

            return View("Index", model);
        }

        /// <summary>
        /// Solo depósitos.
        /// URL: /Transactions/Deposits
        /// </summary>
        public async Task<IActionResult> Deposits(int pageSize = 25)
        {
            var model = await _bitpandaService.GetTransactionsAsync(
                "all", "deposit", null, pageSize);

            ViewBag.AvailableTypes = TransactionsViewModel.AvailableTypes;
            ViewBag.AvailableStatuses = TransactionsViewModel.AvailableStatuses;

            return View("Index", model);
        }

        /// <summary>
        /// Solo retiros.
        /// URL: /Transactions/Withdrawals
        /// </summary>
        public async Task<IActionResult> Withdrawals(int pageSize = 25)
        {
            var model = await _bitpandaService.GetTransactionsAsync(
                "all", "withdrawal", null, pageSize);

            ViewBag.AvailableTypes = TransactionsViewModel.AvailableTypes;
            ViewBag.AvailableStatuses = TransactionsViewModel.AvailableStatuses;

            return View("Index", model);
        }

        /// <summary>
        /// API JSON para transacciones.
        /// URL: /Transactions/Api?category=crypto&type=deposit
        /// </summary>
        public async Task<IActionResult> Api(
            string category = "all",
            string? type = null,
            string? status = null,
            int pageSize = 25)
        {
            if (!_bitpandaService.HasApiKey)
            {
                return Unauthorized(new { error = "API Key no configurada" });
            }

            var transactions = await _bitpandaService.GetTransactionsAsync(
                category, type, status, pageSize);

            return Json(transactions);
        }
    }
}
