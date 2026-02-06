/*
 * ============================================================================
 * WALLETS CONTROLLER
 * ============================================================================
 * 
 * Controlador para visualización de wallets (crypto y fiat).
 * 
 * REQUIERE API KEY para funcionar.
 * 
 * Demuestra:
 * - Endpoints que requieren autenticación
 * - Manejo de errores de autorización
 * - Combinación de datos de múltiples endpoints
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para visualización de wallets.
    /// </summary>
    public class WalletsController : Controller
    {
        private readonly IBitpandaService _bitpandaService;
        private readonly ILogger<WalletsController> _logger;

        public WalletsController(
            IBitpandaService bitpandaService,
            ILogger<WalletsController> logger)
        {
            _bitpandaService = bitpandaService;
            _logger = logger;
        }

        /// <summary>
        /// Lista de todos los wallets.
        /// URL: /Wallets o /Wallets/Index
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var model = await _bitpandaService.GetWalletsAsync();
            return View(model);
        }

        /// <summary>
        /// Solo wallets crypto.
        /// URL: /Wallets/Crypto
        /// </summary>
        public async Task<IActionResult> Crypto()
        {
            if (!_bitpandaService.HasApiKey)
            {
                return RedirectToAction("Settings", "Home");
            }

            var wallets = await _bitpandaService.GetCryptoWalletsAsync();
            return View(wallets);
        }

        /// <summary>
        /// Solo wallets fiat.
        /// URL: /Wallets/Fiat
        /// </summary>
        public async Task<IActionResult> Fiat()
        {
            if (!_bitpandaService.HasApiKey)
            {
                return RedirectToAction("Settings", "Home");
            }

            var wallets = await _bitpandaService.GetFiatWalletsAsync();
            return View(wallets);
        }

        /// <summary>
        /// API JSON para wallets.
        /// URL: /Wallets/Api
        /// </summary>
        public async Task<IActionResult> Api()
        {
            if (!_bitpandaService.HasApiKey)
            {
                return Unauthorized(new { error = "API Key no configurada" });
            }

            var wallets = await _bitpandaService.GetWalletsAsync();
            return Json(wallets);
        }
    }
}
