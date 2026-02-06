/*
 * ============================================================================
 * TRANSACTION MODELS - Bitpanda Wallet Transactions
 * ============================================================================
 * 
 * Este archivo contiene los modelos para los endpoints de transacciones:
 * 
 * 1. GET /wallets/transactions       - Transacciones crypto
 * 2. GET /fiatwallets/transactions   - Transacciones fiat
 * 
 * Autenticación: REQUERIDA via header X-Api-Key
 * 
 * Parámetros de consulta:
 * - type: Tipo de transacción (buy, sell, deposit, withdrawal, etc.)
 * - status: Estado (pending, finished, canceled, etc.)
 * - cursor: Para paginación
 * - page_size: Número de resultados
 * 
 * TIPOS DE TRANSACCIÓN:
 * - buy: Compra de crypto
 * - sell: Venta de crypto
 * - deposit: Depósito (entrada de fondos)
 * - withdrawal: Retiro (salida de fondos)
 * - transfer: Transferencia interna
 * - refund: Reembolso
 * - ico: Participación en ICO
 * 
 * ESTADOS:
 * - pending: Pendiente de procesamiento
 * - processing: En proceso
 * - unconfirmed_transaction_out: Tx saliente sin confirmar
 * - open_invitation: Invitación abierta (referidos)
 * - finished: Completada
 * - canceled: Cancelada
 * ============================================================================
 */

using System.Text.Json.Serialization;

namespace BitpandaExplorer.Models.Transaction
{
    /// <summary>
    /// Respuesta del endpoint /wallets/transactions.
    /// </summary>
    public class CryptoTransactionsResponse
    {
        /// <summary>Lista de transacciones</summary>
        [JsonPropertyName("data")]
        public List<CryptoTransactionData>? Data { get; set; }

        /// <summary>Metadatos de paginación</summary>
        [JsonPropertyName("meta")]
        public TransactionMeta? Meta { get; set; }
    }

    /// <summary>
    /// Metadatos de respuesta de transacciones.
    /// </summary>
    public class TransactionMeta
    {
        /// <summary>Total de transacciones</summary>
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        /// <summary>Cursor para siguiente página</summary>
        [JsonPropertyName("next_cursor")]
        public string? NextCursor { get; set; }

        /// <summary>Tamaño de página</summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Estructura de una transacción crypto.
    /// </summary>
    public class CryptoTransactionData
    {
        /// <summary>Tipo de recurso</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>ID único de la transacción</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Atributos de la transacción</summary>
        [JsonPropertyName("attributes")]
        public CryptoTransactionAttributes? Attributes { get; set; }
    }

    /// <summary>
    /// Atributos de una transacción crypto.
    /// </summary>
    public class CryptoTransactionAttributes
    {
        /// <summary>
        /// Tipo de transacción.
        /// Valores: buy, sell, deposit, withdrawal, transfer, refund, ico
        /// </summary>
        [JsonPropertyName("type")]
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>
        /// Estado de la transacción.
        /// Valores: pending, processing, finished, canceled, etc.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Cantidad de la transacción</summary>
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = "0";

        /// <summary>ID de la criptomoneda</summary>
        [JsonPropertyName("cryptocoin_id")]
        public string CryptocoinId { get; set; } = string.Empty;

        /// <summary>Símbolo de la criptomoneda</summary>
        [JsonPropertyName("cryptocoin_symbol")]
        public string CryptocoinSymbol { get; set; } = string.Empty;

        /// <summary>ID del wallet</summary>
        [JsonPropertyName("wallet_id")]
        public string WalletId { get; set; } = string.Empty;

        /// <summary>Comisión de la transacción</summary>
        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        /// <summary>Precio actual al momento de la tx</summary>
        [JsonPropertyName("current_fiat_amount")]
        public string? CurrentFiatAmount { get; set; }

        /// <summary>ID de moneda fiat</summary>
        [JsonPropertyName("current_fiat_id")]
        public string? CurrentFiatId { get; set; }

        /// <summary>Hash de la transacción blockchain</summary>
        [JsonPropertyName("tx_id")]
        public string? TxId { get; set; }

        /// <summary>Número de confirmaciones</summary>
        [JsonPropertyName("confirmations")]
        public int? Confirmations { get; set; }

        /// <summary>Información temporal</summary>
        [JsonPropertyName("time")]
        public TransactionTime? Time { get; set; }
    }

    /// <summary>
    /// Respuesta del endpoint /fiatwallets/transactions.
    /// </summary>
    public class FiatTransactionsResponse
    {
        /// <summary>Lista de transacciones fiat</summary>
        [JsonPropertyName("data")]
        public List<FiatTransactionData>? Data { get; set; }

        /// <summary>Metadatos de paginación</summary>
        [JsonPropertyName("meta")]
        public TransactionMeta? Meta { get; set; }
    }

    /// <summary>
    /// Estructura de una transacción fiat.
    /// </summary>
    public class FiatTransactionData
    {
        /// <summary>Tipo de recurso</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>ID único de la transacción</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Atributos de la transacción</summary>
        [JsonPropertyName("attributes")]
        public FiatTransactionAttributes? Attributes { get; set; }
    }

    /// <summary>
    /// Atributos de una transacción fiat.
    /// </summary>
    public class FiatTransactionAttributes
    {
        /// <summary>Tipo de transacción</summary>
        [JsonPropertyName("type")]
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>Estado de la transacción</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Cantidad</summary>
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = "0";

        /// <summary>ID del fiat wallet</summary>
        [JsonPropertyName("fiat_wallet_id")]
        public string FiatWalletId { get; set; } = string.Empty;

        /// <summary>ID de la moneda fiat</summary>
        [JsonPropertyName("fiat_id")]
        public string FiatId { get; set; } = string.Empty;

        /// <summary>Comisión</summary>
        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        /// <summary>Información temporal</summary>
        [JsonPropertyName("time")]
        public TransactionTime? Time { get; set; }
    }

    /// <summary>
    /// Información temporal de una transacción.
    /// </summary>
    public class TransactionTime
    {
        /// <summary>Fecha ISO8601</summary>
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
    /// Modelo unificado de transacción para las vistas.
    /// Combina crypto y fiat transactions.
    /// </summary>
    public class TransactionDisplayItem
    {
        /// <summary>ID de la transacción</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Tipo: crypto o fiat</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Tipo de transacción</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Estado</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Símbolo del activo</summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>Cantidad</summary>
        public decimal Amount { get; set; }

        /// <summary>Comisión</summary>
        public decimal? Fee { get; set; }

        /// <summary>Hash de blockchain (solo crypto)</summary>
        public string? TxHash { get; set; }

        /// <summary>Confirmaciones (solo crypto)</summary>
        public int? Confirmations { get; set; }

        /// <summary>Fecha</summary>
        public DateTime Date { get; set; }

        /// <summary>Color CSS según el tipo</summary>
        public string TypeColor
        {
            get
            {
                return Type.ToLower() switch
                {
                    "deposit" => "text-success",
                    "buy" => "text-success",
                    "withdrawal" => "text-danger",
                    "sell" => "text-danger",
                    "transfer" => "text-info",
                    _ => "text-secondary"
                };
            }
        }

        /// <summary>Icono según el tipo</summary>
        public string TypeIcon
        {
            get
            {
                return Type.ToLower() switch
                {
                    "deposit" => "↓",
                    "buy" => "↓",
                    "withdrawal" => "↑",
                    "sell" => "↑",
                    "transfer" => "↔",
                    _ => "•"
                };
            }
        }

        /// <summary>Badge de estado</summary>
        public string StatusBadge
        {
            get
            {
                return Status.ToLower() switch
                {
                    "finished" => "bg-success",
                    "pending" => "bg-warning",
                    "processing" => "bg-info",
                    "canceled" => "bg-danger",
                    _ => "bg-secondary"
                };
            }
        }
    }

    /// <summary>
    /// ViewModel para la página de Transacciones.
    /// </summary>
    public class TransactionsViewModel
    {
        /// <summary>Lista de transacciones</summary>
        public List<TransactionDisplayItem> Transactions { get; set; } = new();

        /// <summary>Total de transacciones</summary>
        public int TotalCount { get; set; }

        /// <summary>Cursor para siguiente página</summary>
        public string? NextCursor { get; set; }

        /// <summary>Tamaño de página</summary>
        public int PageSize { get; set; } = 25;

        /// <summary>Filtro de tipo actual</summary>
        public string? TypeFilter { get; set; }

        /// <summary>Filtro de estado actual</summary>
        public string? StatusFilter { get; set; }

        /// <summary>Filtro de categoría (crypto/fiat)</summary>
        public string CategoryFilter { get; set; } = "all";

        /// <summary>Mensaje de error</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Indica si la consulta fue exitosa</summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        /// <summary>Indica si hay API Key</summary>
        public bool HasApiKey { get; set; }

        /// <summary>Última actualización</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Hay más páginas</summary>
        public bool HasMorePages => !string.IsNullOrEmpty(NextCursor);

        /// <summary>Tipos de transacción disponibles</summary>
        public static readonly string[] AvailableTypes = 
            { "buy", "sell", "deposit", "withdrawal", "transfer", "refund", "ico" };

        /// <summary>Estados disponibles</summary>
        public static readonly string[] AvailableStatuses = 
            { "pending", "processing", "finished", "canceled" };
    }
}
