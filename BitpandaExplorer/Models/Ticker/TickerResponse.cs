/*
 * ============================================================================
 * TICKER MODELS - Bitpanda Public Price Data
 * ============================================================================
 * 
 * Este archivo contiene los modelos para el endpoint público /ticker
 * que devuelve los precios actuales de todas las criptomonedas.
 * 
 * Endpoint: GET https://api.bitpanda.com/v1/ticker
 * Autenticación: NO requerida (endpoint público)
 * 
 * La respuesta es un diccionario donde:
 * - Key: Símbolo del activo (BTC, ETH, etc.)
 * - Value: Diccionario de precios en diferentes monedas fiat
 * 
 * Ejemplo de respuesta:
 * {
 *   "BTC": { "EUR": "62569.65", "USD": "73932.03" },
 *   "ETH": { "EUR": "1809.82", "USD": "2138.48" }
 * }
 * ============================================================================
 */

namespace BitpandaExplorer.Models.Ticker
{
    /// <summary>
    /// Representa el precio de un activo en diferentes monedas fiat.
    /// Cada propiedad corresponde a una moneda fiat soportada por Bitpanda.
    /// </summary>
    public class AssetPrices
    {
        /// <summary>Precio en Euros</summary>
        public string? EUR { get; set; }
        
        /// <summary>Precio en Dólares estadounidenses</summary>
        public string? USD { get; set; }
        
        /// <summary>Precio en Francos suizos</summary>
        public string? CHF { get; set; }
        
        /// <summary>Precio en Libras esterlinas</summary>
        public string? GBP { get; set; }
        
        /// <summary>Precio en Liras turcas</summary>
        public string? TRY { get; set; }
        
        /// <summary>Precio en Zlotys polacos</summary>
        public string? PLN { get; set; }
        
        /// <summary>Precio en Florines húngaros</summary>
        public string? HUF { get; set; }
        
        /// <summary>Precio en Coronas checas</summary>
        public string? CZK { get; set; }
        
        /// <summary>Precio en Coronas suecas</summary>
        public string? SEK { get; set; }
        
        /// <summary>Precio en Coronas danesas</summary>
        public string? DKK { get; set; }
        
        /// <summary>Precio en Leus rumanos</summary>
        public string? RON { get; set; }
    }

    /// <summary>
    /// Modelo para mostrar información de un activo con su precio.
    /// Usado en las vistas para presentar datos de forma legible.
    /// </summary>
    public class TickerItem
    {
        /// <summary>Símbolo del activo (ej: BTC, ETH)</summary>
        public string Symbol { get; set; } = string.Empty;
        
        /// <summary>Precio en EUR como decimal</summary>
        public decimal PriceEUR { get; set; }
        
        /// <summary>Precio en USD como decimal</summary>
        public decimal PriceUSD { get; set; }
        
        /// <summary>Precios originales en todas las monedas</summary>
        public AssetPrices? Prices { get; set; }
    }

    /// <summary>
    /// ViewModel para la página del Ticker.
    /// Contiene la lista de activos y metadatos de la consulta.
    /// </summary>
    public class TickerViewModel
    {
        /// <summary>Lista de activos con sus precios</summary>
        public List<TickerItem> Items { get; set; } = new();
        
        /// <summary>Fecha y hora de la última actualización</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        /// <summary>Número total de activos</summary>
        public int TotalAssets => Items.Count;
        
        /// <summary>Mensaje de error si la consulta falló</summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>Indica si la consulta fue exitosa</summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    }
}
