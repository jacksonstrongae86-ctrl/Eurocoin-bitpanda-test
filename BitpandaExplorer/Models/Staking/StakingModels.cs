/*
 * ============================================================================
 * MODELOS DE STAKING
 * ============================================================================
 * 
 * Modelos para la página informativa de staking.
 * 
 * NOTA: Bitpanda no expone endpoints de staking en su API pública.
 * Esta página muestra información estática sobre activos que soportan
 * staking y los holdings del usuario que podrían ser elegibles.
 * ============================================================================
 */

namespace BitpandaExplorer.Models.Staking
{
    /// <summary>
    /// ViewModel para la página de staking.
    /// </summary>
    public class StakingViewModel
    {
        /// <summary>Indica si hay API key configurada</summary>
        public bool HasApiKey { get; set; }

        /// <summary>Lista de activos que soportan staking</summary>
        public List<StakingAsset> StakingAssets { get; set; } = new();

        /// <summary>Holdings del usuario elegibles para staking</summary>
        public List<UserStakingHolding> UserHoldings { get; set; } = new();

        /// <summary>Valor total elegible para staking en EUR</summary>
        public decimal TotalEligibleValueEUR { get; set; }

        /// <summary>Recompensas anuales estimadas en EUR</summary>
        public decimal EstimatedAnnualRewardsEUR { get; set; }

        /// <summary>Última actualización</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Mensaje de error si hubo problemas</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Indica si la operación fue exitosa</summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    }

    /// <summary>
    /// Información de un activo que soporta staking.
    /// </summary>
    public class StakingAsset
    {
        /// <summary>Símbolo del activo (ETH, SOL, etc.)</summary>
        public string Symbol { get; set; } = "";

        /// <summary>Nombre completo</summary>
        public string Name { get; set; } = "";

        /// <summary>APY estimado (porcentaje anual)</summary>
        public decimal EstimatedAPY { get; set; }

        /// <summary>APY mínimo del rango</summary>
        public decimal MinAPY { get; set; }

        /// <summary>APY máximo del rango</summary>
        public decimal MaxAPY { get; set; }

        /// <summary>Período de bloqueo (días, 0 = flexible)</summary>
        public int LockPeriodDays { get; set; }

        /// <summary>Mínimo para hacer staking</summary>
        public decimal MinimumStake { get; set; }

        /// <summary>Frecuencia de pago de recompensas</summary>
        public string RewardFrequency { get; set; } = "Semanal";

        /// <summary>Tipo de staking</summary>
        public string StakingType { get; set; } = "Proof of Stake";

        /// <summary>Descripción adicional</summary>
        public string? Description { get; set; }

        /// <summary>Si está disponible en Bitpanda</summary>
        public bool AvailableOnBitpanda { get; set; } = true;

        /// <summary>Rango de APY formateado</summary>
        public string APYRange => MinAPY == MaxAPY 
            ? $"{EstimatedAPY:F1}%" 
            : $"{MinAPY:F1}% - {MaxAPY:F1}%";
    }

    /// <summary>
    /// Holding del usuario elegible para staking.
    /// </summary>
    public class UserStakingHolding
    {
        /// <summary>Símbolo del activo</summary>
        public string Symbol { get; set; } = "";

        /// <summary>Nombre del activo</summary>
        public string Name { get; set; } = "";

        /// <summary>Balance actual</summary>
        public decimal Balance { get; set; }

        /// <summary>Valor en EUR</summary>
        public decimal ValueEUR { get; set; }

        /// <summary>APY estimado</summary>
        public decimal EstimatedAPY { get; set; }

        /// <summary>Recompensa anual estimada en cripto</summary>
        public decimal EstimatedAnnualReward { get; set; }

        /// <summary>Recompensa anual estimada en EUR</summary>
        public decimal EstimatedAnnualRewardEUR { get; set; }

        /// <summary>Balance formateado</summary>
        public string BalanceFormatted => Balance < 1 
            ? Balance.ToString("F8") 
            : Balance.ToString("F4");
    }
}
