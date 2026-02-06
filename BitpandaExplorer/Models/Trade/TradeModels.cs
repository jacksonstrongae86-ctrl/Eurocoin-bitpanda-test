/*
 * ============================================================================
 * TRADE MODELS - Bitpanda Trading History
 * ============================================================================
 * 
 * Este archivo contiene los modelos para el endpoint de trades:
 * 
 * Endpoint: GET /trades
 * Autenticación: REQUERIDA via header X-Api-Key
 * 
 * Parámetros de consulta:
 * - type: "buy" o "sell" (filtrar por tipo)
 * - cursor: ID del último trade para paginación
 * - page_size: Número de resultados (default: 25)
 * 
 * Los trades representan operaciones de compra/venta ejecutadas
 * a través de la plataforma Bitpanda.
 * ============================================================================
 */

using System.Text.Json.Serialization;

namespace BitpandaExplorer.Models.Trade
{
    /// <summary>
    /// Respuesta del endpoint /trades.
    /// Incluye datos de trades y metadatos de paginación.
    /// </summary>
    public class TradesResponse
    {
        /// <summary>Lista de trades</summary>
        [JsonPropertyName("data")]
        public List<TradeData>? Data { get; set; }

        /// <summary>Metadatos de paginación</summary>
        [JsonPropertyName("meta")]
        public TradesMeta? Meta { get; set; }
    }

    /// <summary>
    /// Metadatos de la respuesta de trades.
    /// Contiene información de paginación.
    /// </summary>
    public class TradesMeta
    {
        /// <summary>Número total de trades</summary>
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        /// <summary>Cursor para la siguiente página</summary>
        [JsonPropertyName("next_cursor")]
        public string? NextCursor { get; set; }

        /// <summary>Tamaño de página actual</summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Estructura de un trade individual.
    /// Sigue el patrón JSON:API.
    /// </summary>
    public class TradeData
    {
        /// <summary>Tipo de recurso (siempre "trade")</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>ID único del trade (UUID)</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Atributos detallados del trade</summary>
        [JsonPropertyName("attributes")]
        public TradeAttributes? Attributes { get; set; }
    }

    /// <summary>
    /// Atributos de un trade.
    /// Contiene toda la información de la operación.
    /// </summary>
    public class TradeAttributes
    {
        /// <summary>
        /// Estado del trade.
        /// Valores posibles: "finished", "pending", "canceled"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de operación.
        /// Valores: "buy" (compra) o "sell" (venta)
        /// </summary>
        [JsonPropertyName("type")]
        public string TradeType { get; set; } = string.Empty;

        /// <summary>ID interno de la criptomoneda</summary>
        [JsonPropertyName("cryptocoin_id")]
        public string CryptocoinId { get; set; } = string.Empty;

        /// <summary>ID interno de la moneda fiat</summary>
        [JsonPropertyName("fiat_id")]
        public string FiatId { get; set; } = string.Empty;

        /// <summary>Cantidad en fiat de la operación</summary>
        [JsonPropertyName("amount_fiat")]
        public string AmountFiat { get; set; } = "0";

        /// <summary>Cantidad en crypto de la operación</summary>
        [JsonPropertyName("amount_cryptocoin")]
        public string AmountCryptocoin { get; set; } = "0";

        /// <summary>Tasa de conversión de la moneda fiat a EUR</summary>
        [JsonPropertyName("fiat_to_eur_rate")]
        public string FiatToEurRate { get; set; } = "1";

        /// <summary>ID del wallet asociado</summary>
        [JsonPropertyName("wallet_id")]
        public string WalletId { get; set; } = string.Empty;

        /// <summary>Precio al que se ejecutó el trade</summary>
        [JsonPropertyName("price")]
        public string Price { get; set; } = "0";

        /// <summary>Indica si fue un swap (intercambio directo crypto-crypto)</summary>
        [JsonPropertyName("is_swap")]
        public bool IsSwap { get; set; }

        /// <summary>Información temporal del trade</summary>
        [JsonPropertyName("time")]
        public TradeTime? Time { get; set; }
    }

    /// <summary>
    /// Información temporal de un trade.
    /// </summary>
    public class TradeTime
    {
        /// <summary>Fecha y hora en formato ISO8601</summary>
        [JsonPropertyName("date_iso8601")]
        public string DateIso8601 { get; set; } = string.Empty;

        /// <summary>Timestamp Unix</summary>
        [JsonPropertyName("unix")]
        public string Unix { get; set; } = "0";
    }

    // ========================================================================
    // VIEW MODELS
    // ========================================================================

    /// <summary>
    /// Modelo simplificado de trade para las vistas.
    /// </summary>
    public class TradeDisplayItem
    {
        /// <summary>ID del trade</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Tipo de operación (Buy/Sell)</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Estado del trade</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Símbolo de la crypto</summary>
        public string CryptoSymbol { get; set; } = string.Empty;

        /// <summary>Cantidad de crypto</summary>
        public decimal CryptoAmount { get; set; }

        /// <summary>Símbolo del fiat</summary>
        public string FiatSymbol { get; set; } = "EUR";

        /// <summary>Cantidad de fiat</summary>
        public decimal FiatAmount { get; set; }

        /// <summary>Precio de ejecución</summary>
        public decimal Price { get; set; }

        /// <summary>Fecha del trade</summary>
        public DateTime Date { get; set; }

        /// <summary>Es un swap</summary>
        public bool IsSwap { get; set; }

        /// <summary>Color CSS según el tipo</summary>
        public string TypeColor => Type.ToLower() == "buy" ? "text-success" : "text-danger";

        /// <summary>Icono según el tipo</summary>
        public string TypeIcon => Type.ToLower() == "buy" ? "↓" : "↑";
    }

    /// <summary>
    /// ViewModel para la página de Trades.
    /// </summary>
    public class TradesViewModel
    {
        /// <summary>Lista de trades</summary>
        public List<TradeDisplayItem> Trades { get; set; } = new();

        /// <summary>Total de trades</summary>
        public int TotalCount { get; set; }

        /// <summary>Cursor para siguiente página</summary>
        public string? NextCursor { get; set; }

        /// <summary>Tamaño de página</summary>
        public int PageSize { get; set; } = 25;

        /// <summary>Filtro de tipo actual</summary>
        public string? TypeFilter { get; set; }

        /// <summary>Mensaje de error</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Indica si la consulta fue exitosa</summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        /// <summary>Indica si hay API Key configurada</summary>
        public bool HasApiKey { get; set; }

        /// <summary>Fecha de última actualización</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Hay más páginas disponibles</summary>
        public bool HasMorePages => !string.IsNullOrEmpty(NextCursor);
    }
}
