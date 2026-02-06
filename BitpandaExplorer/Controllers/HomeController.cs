/*
 * ============================================================================
 * HOME CONTROLLER
 * ============================================================================
 * 
 * Este es el controlador principal que maneja la página de inicio.
 * 
 * CONCEPTOS DE ASP.NET MVC:
 * 
 * 1. CONTROLLER:
 *    - Recibe peticiones HTTP
 *    - Procesa la lógica (usando servicios)
 *    - Devuelve una Vista (View) o datos (JSON)
 * 
 * 2. ROUTING:
 *    - [Route] define la URL base
 *    - Métodos mapean a acciones: /Home/Index, /Home/Privacy
 *    - Convención: {Controller}/{Action}/{id?}
 * 
 * 3. DEPENDENCY INJECTION:
 *    - Servicios se inyectan en el constructor
 *    - ASP.NET Core resuelve automáticamente
 * 
 * 4. VIEWS:
 *    - IActionResult devuelve View()
 *    - Busca en Views/{Controller}/{Action}.cshtml
 *    - Puede pasar modelo a la vista: View(modelo)
 * ============================================================================
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Models;
using BitpandaExplorer.Services;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para la página principal y configuración.
    /// Hereda de Controller que provee métodos helper como View(), Json(), etc.
    /// </summary>
    public class HomeController : Controller
    {
        // ====================================================================
        // CAMPOS PRIVADOS
        // ====================================================================

        /// <summary>Logger para diagnóstico</summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>Servicio de Bitpanda</summary>
        private readonly IBitpandaService _bitpandaService;

        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        /// <param name="logger">Logger inyectado por DI</param>
        /// <param name="bitpandaService">Servicio de Bitpanda inyectado por DI</param>
        public HomeController(
            ILogger<HomeController> logger,
            IBitpandaService bitpandaService)
        {
            _logger = logger;
            _bitpandaService = bitpandaService;
        }

        // ====================================================================
        // ACCIONES
        // ====================================================================

        /// <summary>
        /// Página principal - Dashboard con resumen.
        /// URL: / o /Home/Index
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Crear modelo para la vista
            var model = new DashboardViewModel
            {
                HasApiKey = _bitpandaService.HasApiKey
            };

            // Siempre cargar ticker (público)
            var ticker = await _bitpandaService.GetTickerAsync();
            model.TopAssets = ticker.Items.Take(10).ToList();
            model.TickerUpdated = ticker.LastUpdated;

            // Si hay API key, cargar datos privados
            if (_bitpandaService.HasApiKey)
            {
                try
                {
                    var wallets = await _bitpandaService.GetWalletsAsync();
                    model.TotalPortfolioEUR = wallets.TotalValueEUR;
                    model.WalletCount = wallets.CryptoWallets.Count + wallets.FiatWallets.Count;

                    var trades = await _bitpandaService.GetTradesAsync(pageSize: 5);
                    model.RecentTradesCount = trades.TotalCount;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard data");
                    model.ErrorMessage = "Error cargando datos. Verifica tu API Key.";
                }
            }

            return View(model);
        }

        /// <summary>
        /// Página de configuración - Para establecer API Key.
        /// URL: /Home/Settings
        /// </summary>
        public IActionResult Settings()
        {
            var model = new SettingsViewModel
            {
                HasApiKey = _bitpandaService.HasApiKey
            };
            return View(model);
        }

        /// <summary>
        /// Procesar configuración de API Key.
        /// Método POST para recibir datos del formulario.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección CSRF
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.ApiKey))
            {
                _bitpandaService.SetApiKey(model.ApiKey);
                
                // Validar la key
                var isValid = await _bitpandaService.ValidateApiKeyAsync();
                
                if (isValid)
                {
                    model.SuccessMessage = "✅ API Key configurada correctamente";
                    model.HasApiKey = true;
                }
                else
                {
                    model.ErrorMessage = "❌ API Key inválida o sin permisos";
                    model.HasApiKey = false;
                }
            }
            else
            {
                model.ErrorMessage = "Por favor ingresa una API Key";
            }

            return View(model);
        }

        /// <summary>
        /// Página de documentación de la estructura del proyecto.
        /// URL: /Home/Docs
        /// </summary>
        public IActionResult Docs()
        {
            return View();
        }

        /// <summary>
        /// Página de privacidad (requerida por template MVC).
        /// URL: /Home/Privacy
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Página de error.
        /// Muestra información del error de forma segura.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }

    // ========================================================================
    // VIEW MODELS ESPECÍFICOS DE ESTE CONTROLLER
    // ========================================================================

    /// <summary>
    /// ViewModel para el Dashboard principal.
    /// </summary>
    public class DashboardViewModel
    {
        public bool HasApiKey { get; set; }
        public decimal TotalPortfolioEUR { get; set; }
        public int WalletCount { get; set; }
        public int RecentTradesCount { get; set; }
        public List<Models.Ticker.TickerItem> TopAssets { get; set; } = new();
        public DateTime TickerUpdated { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// ViewModel para la página de Settings.
    /// </summary>
    public class SettingsViewModel
    {
        public string? ApiKey { get; set; }
        public bool HasApiKey { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
