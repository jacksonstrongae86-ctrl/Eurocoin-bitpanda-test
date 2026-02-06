/*
 * ============================================================================
 * STAKING CONTROLLER
 * ============================================================================
 * 
 * Controlador para la página informativa de staking.
 * 
 * IMPORTANTE: Bitpanda no expone endpoints de staking en su API pública.
 * Esta página proporciona información educativa sobre:
 * - Qué activos soportan staking
 * - APY estimados (datos aproximados, pueden variar)
 * - Holdings del usuario elegibles para staking
 * 
 * Los datos de APY son aproximados y se actualizan manualmente.
 * Para tasas exactas, consultar bitpanda.com
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using BitpandaExplorer.Services;
using BitpandaExplorer.Models.Staking;

namespace BitpandaExplorer.Controllers
{
    /// <summary>
    /// Controlador para información de staking.
    /// </summary>
    public class StakingController : Controller
    {
        private readonly IBitpandaService _bitpandaService;
        private readonly ILogger<StakingController> _logger;

        /// <summary>
        /// Datos estáticos de activos que soportan staking.
        /// APY son estimados y pueden variar. Actualizado Feb 2026.
        /// </summary>
        private static readonly List<StakingAsset> StakingAssetsData = new()
        {
            new StakingAsset
            {
                Symbol = "ETH",
                Name = "Ethereum",
                EstimatedAPY = 3.5m,
                MinAPY = 3.0m,
                MaxAPY = 4.0m,
                LockPeriodDays = 0,
                MinimumStake = 0.01m,
                RewardFrequency = "Semanal",
                StakingType = "Proof of Stake",
                Description = "Staking nativo de Ethereum 2.0"
            },
            new StakingAsset
            {
                Symbol = "SOL",
                Name = "Solana",
                EstimatedAPY = 6.0m,
                MinAPY = 5.5m,
                MaxAPY = 7.0m,
                LockPeriodDays = 0,
                MinimumStake = 0.1m,
                RewardFrequency = "Cada 2-3 días",
                StakingType = "Proof of Stake",
                Description = "Alta velocidad, bajas comisiones"
            },
            new StakingAsset
            {
                Symbol = "ADA",
                Name = "Cardano",
                EstimatedAPY = 4.5m,
                MinAPY = 4.0m,
                MaxAPY = 5.0m,
                LockPeriodDays = 0,
                MinimumStake = 1m,
                RewardFrequency = "Cada 5 días (epoch)",
                StakingType = "Proof of Stake",
                Description = "Sin período de bloqueo"
            },
            new StakingAsset
            {
                Symbol = "DOT",
                Name = "Polkadot",
                EstimatedAPY = 12.0m,
                MinAPY = 10.0m,
                MaxAPY = 14.0m,
                LockPeriodDays = 28,
                MinimumStake = 1m,
                RewardFrequency = "Diario",
                StakingType = "Nominated PoS",
                Description = "Alto APY, período de desbloqueo de 28 días"
            },
            new StakingAsset
            {
                Symbol = "ATOM",
                Name = "Cosmos",
                EstimatedAPY = 18.0m,
                MinAPY = 15.0m,
                MaxAPY = 20.0m,
                LockPeriodDays = 21,
                MinimumStake = 0.1m,
                RewardFrequency = "Instantáneo",
                StakingType = "Proof of Stake",
                Description = "Uno de los APY más altos"
            },
            new StakingAsset
            {
                Symbol = "AVAX",
                Name = "Avalanche",
                EstimatedAPY = 8.0m,
                MinAPY = 7.0m,
                MaxAPY = 9.0m,
                LockPeriodDays = 14,
                MinimumStake = 0.1m,
                RewardFrequency = "Diario",
                StakingType = "Proof of Stake",
                Description = "Red rápida y escalable"
            },
            new StakingAsset
            {
                Symbol = "MATIC",
                Name = "Polygon",
                EstimatedAPY = 5.0m,
                MinAPY = 4.0m,
                MaxAPY = 6.0m,
                LockPeriodDays = 0,
                MinimumStake = 1m,
                RewardFrequency = "Semanal",
                StakingType = "Proof of Stake",
                Description = "Layer 2 de Ethereum"
            },
            new StakingAsset
            {
                Symbol = "NEAR",
                Name = "NEAR Protocol",
                EstimatedAPY = 9.5m,
                MinAPY = 8.0m,
                MaxAPY = 11.0m,
                LockPeriodDays = 2,
                MinimumStake = 0.1m,
                RewardFrequency = "Cada 12 horas",
                StakingType = "Proof of Stake",
                Description = "Sharding para escalabilidad"
            },
            new StakingAsset
            {
                Symbol = "XTZ",
                Name = "Tezos",
                EstimatedAPY = 5.5m,
                MinAPY = 5.0m,
                MaxAPY = 6.0m,
                LockPeriodDays = 0,
                MinimumStake = 0.1m,
                RewardFrequency = "Cada 3 días",
                StakingType = "Liquid PoS",
                Description = "Blockchain auto-enmendable"
            },
            new StakingAsset
            {
                Symbol = "ALGO",
                Name = "Algorand",
                EstimatedAPY = 5.0m,
                MinAPY = 4.0m,
                MaxAPY = 6.0m,
                LockPeriodDays = 0,
                MinimumStake = 0.1m,
                RewardFrequency = "Instantáneo",
                StakingType = "Pure PoS",
                Description = "Staking automático"
            },
            new StakingAsset
            {
                Symbol = "TRX",
                Name = "TRON",
                EstimatedAPY = 4.0m,
                MinAPY = 3.5m,
                MaxAPY = 5.0m,
                LockPeriodDays = 3,
                MinimumStake = 1m,
                RewardFrequency = "Diario",
                StakingType = "Delegated PoS",
                Description = "Red de entretenimiento"
            },
            new StakingAsset
            {
                Symbol = "BEST",
                Name = "Bitpanda Ecosystem Token",
                EstimatedAPY = 8.0m,
                MinAPY = 5.0m,
                MaxAPY = 12.0m,
                LockPeriodDays = 0,
                MinimumStake = 1m,
                RewardFrequency = "Semanal",
                StakingType = "Rewards Program",
                Description = "Token nativo de Bitpanda con beneficios VIP"
            }
        };

        public StakingController(
            IBitpandaService bitpandaService,
            ILogger<StakingController> logger)
        {
            _bitpandaService = bitpandaService;
            _logger = logger;
        }

        /// <summary>
        /// Página principal de staking.
        /// URL: /Staking
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var viewModel = new StakingViewModel
            {
                HasApiKey = _bitpandaService.HasApiKey,
                StakingAssets = StakingAssetsData
            };

            if (_bitpandaService.HasApiKey)
            {
                try
                {
                    // Obtener wallets del usuario
                    var wallets = await _bitpandaService.GetWalletsAsync();
                    
                    if (wallets.IsSuccess)
                    {
                        // Buscar holdings que coincidan con activos de staking
                        var stakingSymbols = StakingAssetsData
                            .Select(s => s.Symbol.ToUpper())
                            .ToHashSet();

                        foreach (var wallet in wallets.CryptoWallets)
                        {
                            if (wallet.Balance > 0 && 
                                stakingSymbols.Contains(wallet.Symbol.ToUpper()))
                            {
                                var stakingInfo = StakingAssetsData
                                    .First(s => s.Symbol.Equals(wallet.Symbol, 
                                        StringComparison.OrdinalIgnoreCase));

                                var annualReward = wallet.Balance * (stakingInfo.EstimatedAPY / 100);
                                var annualRewardEUR = (wallet.ValueEUR ?? 0) * (stakingInfo.EstimatedAPY / 100);

                                viewModel.UserHoldings.Add(new UserStakingHolding
                                {
                                    Symbol = wallet.Symbol,
                                    Name = stakingInfo.Name,
                                    Balance = wallet.Balance,
                                    ValueEUR = wallet.ValueEUR ?? 0,
                                    EstimatedAPY = stakingInfo.EstimatedAPY,
                                    EstimatedAnnualReward = annualReward,
                                    EstimatedAnnualRewardEUR = annualRewardEUR
                                });
                            }
                        }

                        // Calcular totales
                        viewModel.TotalEligibleValueEUR = viewModel.UserHoldings
                            .Sum(h => h.ValueEUR);
                        viewModel.EstimatedAnnualRewardsEUR = viewModel.UserHoldings
                            .Sum(h => h.EstimatedAnnualRewardEUR);

                        // Ordenar por valor
                        viewModel.UserHoldings = viewModel.UserHoldings
                            .OrderByDescending(h => h.ValueEUR)
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user holdings for staking");
                    viewModel.ErrorMessage = "Error al obtener holdings";
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// API JSON con datos de staking.
        /// URL: /Staking/Api
        /// </summary>
        public IActionResult Api()
        {
            return Json(new
            {
                assets = StakingAssetsData,
                lastUpdated = DateTime.UtcNow,
                disclaimer = "APY son estimados y pueden variar. Consultar bitpanda.com para tasas actuales."
            });
        }
    }
}
