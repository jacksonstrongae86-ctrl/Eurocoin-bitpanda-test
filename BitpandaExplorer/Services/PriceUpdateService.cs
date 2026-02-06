/*
 * ============================================================================
 * PRICE UPDATE BACKGROUND SERVICE
 * ============================================================================
 * 
 * Servicio en segundo plano que actualiza precios periódicamente
 * y los envía a clientes conectados via SignalR.
 * 
 * CONCEPTOS DE BACKGROUND SERVICES:
 * - IHostedService: Interfaz para servicios de larga duración
 * - BackgroundService: Clase base con ExecuteAsync
 * - Se inicia automáticamente con la aplicación
 * - Corre en un thread separado
 * 
 * PATRÓN:
 * 1. Loop infinito con delay
 * 2. Fetch datos de API
 * 3. Broadcast via SignalR Hub
 * 4. Repetir
 * ============================================================================
 */

using Microsoft.AspNetCore.SignalR;
using BitpandaExplorer.Hubs;

namespace BitpandaExplorer.Services
{
    /// <summary>
    /// Servicio en segundo plano para actualizar precios.
    /// </summary>
    public class PriceUpdateService : BackgroundService
    {
        private readonly ILogger<PriceUpdateService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<PriceHub> _hubContext;
        
        // Intervalo de actualización (30 segundos)
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

        public PriceUpdateService(
            ILogger<PriceUpdateService> logger,
            IServiceProvider serviceProvider,
            IHubContext<PriceHub> hubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Método principal del servicio.
        /// Se ejecuta en bucle hasta que la aplicación se detenga.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PriceUpdateService iniciado. Intervalo: {Interval}s", 
                _updateInterval.TotalSeconds);

            // Esperar un poco antes de empezar (dar tiempo a que la app inicie)
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await BroadcastPricesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting prices");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }

            _logger.LogInformation("PriceUpdateService detenido");
        }

        /// <summary>
        /// Obtiene precios y los envía a todos los clientes suscritos.
        /// </summary>
        private async Task BroadcastPricesAsync()
        {
            // Usar scope para obtener el servicio singleton
            using var scope = _serviceProvider.CreateScope();
            var bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();

            var ticker = await bitpandaService.GetTickerAsync();

            if (!ticker.IsSuccess || !ticker.Items.Any())
            {
                _logger.LogWarning("No se pudieron obtener precios para broadcast");
                return;
            }

            // Preparar datos para enviar (solo top 20 para reducir payload)
            var topPrices = ticker.Items
                .Take(20)
                .Select(t => new
                {
                    symbol = t.Symbol,
                    priceEUR = t.PriceEUR,
                    priceUSD = t.PriceUSD,
                    timestamp = DateTime.UtcNow
                })
                .ToList();

            // Enviar a todos los suscritos al grupo "price_all"
            await _hubContext.Clients.Group("price_all")
                .SendAsync("ReceivePrices", topPrices);

            // Enviar actualizaciones individuales a grupos por símbolo
            foreach (var price in ticker.Items.Take(50))
            {
                var groupName = $"price_{price.Symbol}";
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceivePrice", new
                    {
                        symbol = price.Symbol,
                        priceEUR = price.PriceEUR,
                        priceUSD = price.PriceUSD,
                        timestamp = DateTime.UtcNow
                    });
            }

            _logger.LogDebug("Broadcast de precios completado: {Count} activos", topPrices.Count);
        }
    }
}
