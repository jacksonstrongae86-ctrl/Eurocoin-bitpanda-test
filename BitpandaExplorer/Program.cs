/*
 * ============================================================================
 * PROGRAM.CS - PUNTO DE ENTRADA DE LA APLICACIÓN
 * ============================================================================
 * 
 * Este es el archivo principal que configura y arranca la aplicación.
 * 
 * CONCEPTOS CLAVE:
 * 
 * 1. BUILDER PATTERN:
 *    - WebApplication.CreateBuilder() crea el builder
 *    - Configuramos servicios (DI)
 *    - Build() crea la aplicación
 * 
 * 2. DEPENDENCY INJECTION (DI):
 *    - builder.Services.AddXxx() registra servicios
 *    - Singleton: Una instancia para toda la app
 *    - Scoped: Una instancia por request HTTP
 *    - Transient: Nueva instancia cada vez
 * 
 * 3. MIDDLEWARE PIPELINE:
 *    - app.UseXxx() configura middleware
 *    - Se ejecutan en orden para cada request
 *    - Ejemplos: routing, static files, auth, etc.
 * 
 * 4. CONFIGURACIÓN:
 *    - appsettings.json para valores por defecto
 *    - Variables de entorno para producción
 *    - IConfiguration permite acceder a valores
 * ============================================================================
 */

using BitpandaExplorer.Services;

// ============================================================================
// 1. CREAR EL BUILDER
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 2. CONFIGURAR SERVICIOS (DEPENDENCY INJECTION)
// ============================================================================

// Añadir MVC (Controllers + Views)
// Esto registra todos los servicios necesarios para MVC
builder.Services.AddControllersWithViews();

// Añadir MemoryCache para caching de datos
// Reduce llamadas a APIs externas y mejora rendimiento
builder.Services.AddMemoryCache();

// Añadir HttpClientFactory
// Patrón recomendado para crear HttpClient de forma eficiente
// Evita problemas de socket exhaustion
builder.Services.AddHttpClient();

// Registrar nuestro servicio de Bitpanda
// Singleton: Una sola instancia compartida (mantiene cache del ticker)
builder.Services.AddSingleton<IBitpandaService, BitpandaService>();

// Registrar servicio de CoinGecko para datos históricos
// Singleton: Compartir la misma instancia
builder.Services.AddSingleton<ICoinGeckoService, CoinGeckoService>();

// ============================================================================
// 3. CONFIGURAR KESTREL (SERVIDOR WEB)
// ============================================================================

// Configurar el puerto 3337
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(3337); // Escuchar en todas las interfaces, puerto 3337
});

// ============================================================================
// 4. BUILD - CREAR LA APLICACIÓN
// ============================================================================

var app = builder.Build();

// ============================================================================
// 5. CONFIGURAR MIDDLEWARE PIPELINE
// ============================================================================

// En desarrollo: mostrar página de error detallada
// En producción: mostrar página de error genérica
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS: HTTP Strict Transport Security (seguridad)
    app.UseHsts();
}

// Servir archivos estáticos (wwwroot: CSS, JS, imágenes)
app.UseStaticFiles();

// Habilitar routing
app.UseRouting();

// Autorización (aunque no la usamos, es buena práctica)
app.UseAuthorization();

// ============================================================================
// 6. CONFIGURAR RUTAS
// ============================================================================

// Ruta por defecto: {controller=Home}/{action=Index}/{id?}
// Esto significa:
// - / → HomeController.Index()
// - /Ticker → TickerController.Index()
// - /Wallets/Crypto → WalletsController.Crypto()
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============================================================================
// 7. EJECUTAR LA APLICACIÓN
// ============================================================================

Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║   🚀 BITPANDA EXPLORER                                          ║
║                                                                  ║
║   Servidor iniciado en: http://localhost:3337                   ║
║                                                                  ║
║   Endpoints disponibles:                                        ║
║   • /           - Dashboard principal                           ║
║   • /Ticker     - Precios actuales (público)                   ║
║   • /History    - Historial de precios (CoinGecko)             ║
║   • /Wallets    - Tus wallets (requiere API Key)               ║
║   • /Trades     - Historial de trades (requiere API Key)       ║
║   • /Transactions - Historial de transacciones                 ║
║   • /Home/Settings - Configurar API Key                        ║
║   • /Home/Docs  - Documentación del proyecto                   ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
");

app.Run();
