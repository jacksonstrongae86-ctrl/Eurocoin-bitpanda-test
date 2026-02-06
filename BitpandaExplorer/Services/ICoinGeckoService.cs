/*
 * ============================================================================
 * COINGECKO SERVICE INTERFACE
 * ============================================================================
 * 
 * Define el contrato para obtener datos históricos de precios.
 * CoinGecko ofrece una API gratuita sin necesidad de API key.
 * 
 * ENDPOINTS UTILIZADOS:
 * - /coins/{id}/market_chart — Historial de precios
 * - /coins/list — Lista de todas las monedas con sus IDs
 * ============================================================================
 */

using BitpandaExplorer.Models.History;

namespace BitpandaExplorer.Services
{
    /// <summary>
    /// Servicio para obtener datos históricos de CoinGecko.
    /// </summary>
    public interface ICoinGeckoService
    {
        /// <summary>
        /// Obtiene el historial de precios de una moneda.
        /// </summary>
        /// <param name="symbol">Símbolo de la moneda (BTC, ETH, etc.)</param>
        /// <param name="days">Número de días de historial (1, 7, 30, 90, 365)</param>
        /// <param name="currency">Moneda de referencia (eur, usd)</param>
        Task<PriceHistoryViewModel> GetPriceHistoryAsync(string symbol, int days = 7, string currency = "eur");

        /// <summary>
        /// Obtiene el ID de CoinGecko para un símbolo.
        /// </summary>
        /// <param name="symbol">Símbolo (BTC, ETH, etc.)</param>
        string? GetCoinGeckoId(string symbol);
    }
}
