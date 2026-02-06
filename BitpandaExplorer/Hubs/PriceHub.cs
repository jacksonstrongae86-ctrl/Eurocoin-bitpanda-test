/*
 * ============================================================================
 * SIGNALR PRICE HUB
 * ============================================================================
 * 
 * Hub de SignalR para enviar actualizaciones de precios en tiempo real.
 * 
 * CONCEPTOS DE SIGNALR:
 * - Hub: Punto central de comunicación cliente-servidor
 * - Clients.All: Envía a todos los clientes conectados
 * - Clients.Caller: Envía solo al cliente que llamó
 * - Groups: Permite agrupar clientes por interés
 * 
 * FLUJO:
 * 1. Cliente se conecta via WebSocket
 * 2. Cliente puede suscribirse a símbolos específicos
 * 3. BackgroundService envía actualizaciones periódicas
 * 4. Cliente recibe precios sin recargar página
 * ============================================================================
 */

using Microsoft.AspNetCore.SignalR;

namespace BitpandaExplorer.Hubs
{
    /// <summary>
    /// Hub de SignalR para precios en tiempo real.
    /// </summary>
    public class PriceHub : Hub
    {
        private readonly ILogger<PriceHub> _logger;

        public PriceHub(ILogger<PriceHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Llamado cuando un cliente se conecta.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Llamado cuando un cliente se desconecta.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Suscribirse a actualizaciones de un símbolo específico.
        /// El cliente llama: connection.invoke("SubscribeToSymbol", "BTC")
        /// </summary>
        public async Task SubscribeToSymbol(string symbol)
        {
            var groupName = $"price_{symbol.ToUpperInvariant()}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("Cliente {ConnectionId} suscrito a {Symbol}", 
                Context.ConnectionId, symbol);
        }

        /// <summary>
        /// Desuscribirse de un símbolo.
        /// </summary>
        public async Task UnsubscribeFromSymbol(string symbol)
        {
            var groupName = $"price_{symbol.ToUpperInvariant()}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("Cliente {ConnectionId} desuscrito de {Symbol}", 
                Context.ConnectionId, symbol);
        }

        /// <summary>
        /// Suscribirse a todas las actualizaciones de precios.
        /// </summary>
        public async Task SubscribeToAll()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "price_all");
            _logger.LogDebug("Cliente {ConnectionId} suscrito a todos los precios", 
                Context.ConnectionId);
        }
    }
}
