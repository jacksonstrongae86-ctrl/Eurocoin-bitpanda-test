/*
 * ============================================================================
 * MODELOS DE HISTORIAL DE PRECIOS
 * ============================================================================
 * 
 * Modelos para representar datos históricos de precios desde CoinGecko.
 * 
 * ESTRUCTURA DE RESPUESTA DE COINGECKO:
 * {
 *   "prices": [[timestamp, price], [timestamp, price], ...],
 *   "market_caps": [[timestamp, cap], ...],
 *   "total_volumes": [[timestamp, volume], ...]
 * }
 * ============================================================================
 */

namespace BitpandaExplorer.Models.History
{
    /// <summary>
    /// ViewModel para la vista de historial de precios.
    /// </summary>
    public class PriceHistoryViewModel
    {
        /// <summary>Símbolo de la moneda (BTC, ETH, etc.)</summary>
        public string Symbol { get; set; } = "";

        /// <summary>Nombre completo de la moneda</summary>
        public string Name { get; set; } = "";

        /// <summary>Moneda de referencia (EUR, USD)</summary>
        public string Currency { get; set; } = "EUR";

        /// <summary>Días de historial solicitados</summary>
        public int Days { get; set; } = 7;

        /// <summary>Puntos de datos de precios</summary>
        public List<PricePoint> Prices { get; set; } = new();

        /// <summary>Precio actual</summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>Precio más alto en el período</summary>
        public decimal HighPrice { get; set; }

        /// <summary>Precio más bajo en el período</summary>
        public decimal LowPrice { get; set; }

        /// <summary>Cambio porcentual en el período</summary>
        public decimal ChangePercent { get; set; }

        /// <summary>Última actualización</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Mensaje de error si hubo problemas</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Indica si la operación fue exitosa</summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Datos formateados para Chart.js (JSON).
        /// </summary>
        public string ChartLabelsJson => System.Text.Json.JsonSerializer.Serialize(
            Prices.Select(p => p.Date.ToString("dd/MM HH:mm")).ToList());

        /// <summary>
        /// Precios formateados para Chart.js (JSON).
        /// </summary>
        public string ChartDataJson => System.Text.Json.JsonSerializer.Serialize(
            Prices.Select(p => Math.Round(p.Price, 2)).ToList());
    }

    /// <summary>
    /// Un punto de datos de precio en el tiempo.
    /// </summary>
    public class PricePoint
    {
        /// <summary>Fecha/hora del punto de datos</summary>
        public DateTime Date { get; set; }

        /// <summary>Precio en ese momento</summary>
        public decimal Price { get; set; }

        /// <summary>Timestamp Unix (milisegundos)</summary>
        public long TimestampMs { get; set; }
    }

    /// <summary>
    /// Respuesta raw de CoinGecko /market_chart.
    /// </summary>
    public class CoinGeckoMarketChartResponse
    {
        /// <summary>Array de [timestamp_ms, price]</summary>
        public List<List<decimal>>? Prices { get; set; }

        /// <summary>Array de [timestamp_ms, market_cap]</summary>
        public List<List<decimal>>? Market_caps { get; set; }

        /// <summary>Array de [timestamp_ms, volume]</summary>
        public List<List<decimal>>? Total_volumes { get; set; }
    }
}
