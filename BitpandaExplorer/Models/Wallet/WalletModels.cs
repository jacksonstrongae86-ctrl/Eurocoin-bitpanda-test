/*
 * ============================================================================
 * WALLET MODELS - Bitpanda Wallet Data
 * ============================================================================
 * 
 * Este archivo contiene los modelos para los endpoints de wallets:
 * 
 * 1. GET /wallets         - Wallets de criptomonedas
 * 2. GET /fiatwallets     - Wallets de moneda fiat (EUR, USD, etc.)
 * 3. GET /asset-wallets   - Todos los wallets agrupados
 * 
 * Autenticación: REQUERIDA via header X-Api-Key
 * 
 * Estructura de respuesta típica de Bitpanda API:
 * {
 *   "data": [
 *     {
 *       "type": "wallet",
 *       "id": "uuid-here",
 *       "attributes": { ... }
 *     }
 *   ]
 * }
 * ============================================================================
 */

using System.Text.Json.Serialization;

namespace BitpandaExplorer.Models.Wallet
{
    // ========================================================================
    // CRYPTO WALLETS - Endpoint: GET /wallets
    // ========================================================================

    /// <summary>
    /// Respuesta del endpoint /wallets.
    /// Contiene una lista de wallets de criptomonedas.
    /// </summary>
    public class CryptoWalletsResponse
    {
        /// <summary>Lista de wallets crypto</summary>
        [JsonPropertyName("data")]
        public List<CryptoWalletData>? Data { get; set; }
    }

    /// <summary>
    /// Estructura de un wallet individual en la respuesta.
    /// Sigue el patrón JSON:API con type, id, y attributes.
    /// </summary>
    public class CryptoWalletData
    {
        /// <summary>Tipo de recurso (siempre "wallet")</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>ID único del wallet (UUID)</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Atributos detallados del wallet</summary>
        [JsonPropertyName("attributes")]
        public CryptoWalletAttributes? Attributes { get; set; }
    }

    /// <summary>
    /// Atributos de un wallet de criptomoneda.
    /// </summary>
    public class CryptoWalletAttributes
    {
        /// <summary>ID interno de la criptomoneda</summary>
        [JsonPropertyName("cryptocoin_id")]
        public string CryptocoinId { get; set; } = string.Empty;

        /// <summary>Símbolo de la criptomoneda (BTC, ETH, etc.)</summary>
        [JsonPropertyName("cryptocoin_symbol")]
        public string CryptocoinSymbol { get; set; } = string.Empty;

        /// <summary>Balance actual del wallet</summary>
        [JsonPropertyName("balance")]
        public string Balance { get; set; } = "0";

        /// <summary>Indica si es el wallet por defecto para esta crypto</summary>
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        /// <summary>Nombre personalizado del wallet</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Número de transacciones pendientes</summary>
        [JsonPropertyName("pending_transactions_count")]
        public int PendingTransactionsCount { get; set; }

        /// <summary>Indica si el wallet ha sido eliminado</summary>
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }
    }

    // ========================================================================
    // FIAT WALLETS - Endpoint: GET /fiatwallets
    // ========================================================================

    /// <summary>
    /// Respuesta del endpoint /fiatwallets.
    /// Contiene una lista de wallets de moneda fiat.
    /// </summary>
    public class FiatWalletsResponse
    {
        /// <summary>Lista de wallets fiat</summary>
        [JsonPropertyName("data")]
        public List<FiatWalletData>? Data { get; set; }
    }

    /// <summary>
    /// Estructura de un wallet fiat individual.
    /// </summary>
    public class FiatWalletData
    {
        /// <summary>Tipo de recurso (siempre "fiat_wallet")</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>ID único del wallet (UUID)</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Atributos detallados del wallet</summary>
        [JsonPropertyName("attributes")]
        public FiatWalletAttributes? Attributes { get; set; }
    }

    /// <summary>
    /// Atributos de un wallet fiat.
    /// </summary>
    public class FiatWalletAttributes
    {
        /// <summary>ID interno de la moneda fiat</summary>
        [JsonPropertyName("fiat_id")]
        public string FiatId { get; set; } = string.Empty;

        /// <summary>Símbolo de la moneda fiat (EUR, USD, etc.)</summary>
        [JsonPropertyName("fiat_symbol")]
        public string FiatSymbol { get; set; } = string.Empty;

        /// <summary>Balance actual del wallet</summary>
        [JsonPropertyName("balance")]
        public string Balance { get; set; } = "0";

        /// <summary>Nombre del wallet</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Número de transacciones pendientes</summary>
        [JsonPropertyName("pending_transactions_count")]
        public int PendingTransactionsCount { get; set; }
    }

    // ========================================================================
    // VIEW MODELS - Para presentación en las vistas
    // ========================================================================

    /// <summary>
    /// Modelo simplificado de wallet para las vistas.
    /// Unifica crypto y fiat wallets en una estructura común.
    /// </summary>
    public class WalletDisplayItem
    {
        /// <summary>ID del wallet</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Tipo: "crypto" o "fiat"</summary>
        public string WalletType { get; set; } = string.Empty;

        /// <summary>Símbolo del activo (BTC, EUR, etc.)</summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>Nombre del wallet</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Balance como decimal</summary>
        public decimal Balance { get; set; }

        /// <summary>Balance formateado como string</summary>
        public string BalanceFormatted => Balance.ToString("N8");

        /// <summary>Valor en EUR (calculado con ticker)</summary>
        public decimal? ValueEUR { get; set; }

        /// <summary>Transacciones pendientes</summary>
        public int PendingTransactions { get; set; }

        /// <summary>Es el wallet por defecto</summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// ViewModel para la página de Wallets.
    /// Contiene ambos tipos de wallets y totales.
    /// </summary>
    public class WalletsViewModel
    {
        /// <summary>Lista de wallets crypto</summary>
        public List<WalletDisplayItem> CryptoWallets { get; set; } = new();

        /// <summary>Lista de wallets fiat</summary>
        public List<WalletDisplayItem> FiatWallets { get; set; } = new();

        /// <summary>Valor total de la cartera en EUR</summary>
        public decimal TotalValueEUR { get; set; }

        /// <summary>Fecha de última actualización</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Mensaje de error</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Indica si la consulta fue exitosa</summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        /// <summary>Indica si hay API Key configurada</summary>
        public bool HasApiKey { get; set; }
    }
}
