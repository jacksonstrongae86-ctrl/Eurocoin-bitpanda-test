/*
 * ============================================================================
 * BITPANDA SERVICE INTERFACE
 * ============================================================================
 * 
 * Esta interfaz define el contrato para el servicio de Bitpanda.
 * 
 * En C# y .NET, usamos interfaces para:
 * 1. ABSTRACCIÓN: Definir qué hace un servicio sin especificar cómo
 * 2. TESTING: Facilitar mock/stub en pruebas unitarias
 * 3. DEPENDENCY INJECTION: Permitir diferentes implementaciones
 * 4. DESACOPLAMIENTO: Reducir dependencias entre clases
 * 
 * El patrón común en ASP.NET Core es:
 * - Definir interfaz (ISomethingService)
 * - Implementar clase concreta (SomethingService)
 * - Registrar en DI container (Program.cs)
 * - Inyectar en controllers vía constructor
 * ============================================================================
 */

using BitpandaExplorer.Models.Ticker;
using BitpandaExplorer.Models.Wallet;
using BitpandaExplorer.Models.Trade;
using BitpandaExplorer.Models.Transaction;

namespace BitpandaExplorer.Services
{
    /// <summary>
    /// Interfaz que define todas las operaciones disponibles
    /// para interactuar con la API de Bitpanda.
    /// </summary>
    public interface IBitpandaService
    {
        // ====================================================================
        // CONFIGURACIÓN
        // ====================================================================

        /// <summary>
        /// Verifica si hay una API Key configurada.
        /// </summary>
        bool HasApiKey { get; }

        /// <summary>
        /// Configura la API Key para las peticiones autenticadas.
        /// </summary>
        /// <param name="apiKey">La API Key de Bitpanda</param>
        void SetApiKey(string apiKey);

        // ====================================================================
        // ENDPOINTS PÚBLICOS (sin autenticación)
        // ====================================================================

        /// <summary>
        /// Obtiene los precios actuales de todos los activos.
        /// Este endpoint es PÚBLICO y no requiere autenticación.
        /// </summary>
        /// <returns>ViewModel con todos los precios</returns>
        Task<TickerViewModel> GetTickerAsync();

        // ====================================================================
        // ENDPOINTS PRIVADOS (requieren API Key)
        // ====================================================================

        /// <summary>
        /// Obtiene todos los wallets (crypto + fiat).
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <returns>ViewModel con wallets y totales</returns>
        Task<WalletsViewModel> GetWalletsAsync();

        /// <summary>
        /// Obtiene solo los wallets de criptomonedas.
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <returns>Lista de wallets crypto</returns>
        Task<List<WalletDisplayItem>> GetCryptoWalletsAsync();

        /// <summary>
        /// Obtiene solo los wallets de fiat.
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <returns>Lista de wallets fiat</returns>
        Task<List<WalletDisplayItem>> GetFiatWalletsAsync();

        /// <summary>
        /// Obtiene el historial de trades con paginación.
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <param name="type">Filtrar por tipo: "buy" o "sell" (opcional)</param>
        /// <param name="cursor">Cursor para paginación (opcional)</param>
        /// <param name="pageSize">Número de resultados por página</param>
        /// <returns>ViewModel con trades y metadatos de paginación</returns>
        Task<TradesViewModel> GetTradesAsync(
            string? type = null, 
            string? cursor = null, 
            int pageSize = 25);

        /// <summary>
        /// Obtiene las transacciones de crypto wallets.
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <param name="type">Filtrar por tipo (opcional)</param>
        /// <param name="status">Filtrar por estado (opcional)</param>
        /// <param name="cursor">Cursor para paginación (opcional)</param>
        /// <param name="pageSize">Número de resultados</param>
        /// <returns>Lista de transacciones crypto</returns>
        Task<List<TransactionDisplayItem>> GetCryptoTransactionsAsync(
            string? type = null,
            string? status = null,
            string? cursor = null,
            int pageSize = 25);

        /// <summary>
        /// Obtiene las transacciones de fiat wallets.
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <param name="type">Filtrar por tipo (opcional)</param>
        /// <param name="status">Filtrar por estado (opcional)</param>
        /// <param name="cursor">Cursor para paginación (opcional)</param>
        /// <param name="pageSize">Número de resultados</param>
        /// <returns>Lista de transacciones fiat</returns>
        Task<List<TransactionDisplayItem>> GetFiatTransactionsAsync(
            string? type = null,
            string? status = null,
            string? cursor = null,
            int pageSize = 25);

        /// <summary>
        /// Obtiene todas las transacciones (crypto + fiat) combinadas.
        /// REQUIERE autenticación con API Key.
        /// </summary>
        /// <param name="category">Filtrar por categoría: "crypto", "fiat", o "all"</param>
        /// <param name="type">Filtrar por tipo (opcional)</param>
        /// <param name="status">Filtrar por estado (opcional)</param>
        /// <param name="pageSize">Número de resultados</param>
        /// <returns>ViewModel con transacciones combinadas</returns>
        Task<TransactionsViewModel> GetTransactionsAsync(
            string category = "all",
            string? type = null,
            string? status = null,
            int pageSize = 25);

        // ====================================================================
        // UTILIDADES
        // ====================================================================

        /// <summary>
        /// Calcula el valor total de todos los wallets en EUR.
        /// Combina wallets crypto (convertidos con ticker) + wallets fiat.
        /// </summary>
        /// <returns>Valor total en EUR</returns>
        Task<decimal> CalculateTotalPortfolioValueAsync();

        /// <summary>
        /// Valida que la API Key sea correcta haciendo una petición de prueba.
        /// </summary>
        /// <returns>True si la key es válida</returns>
        Task<bool> ValidateApiKeyAsync();
    }
}
