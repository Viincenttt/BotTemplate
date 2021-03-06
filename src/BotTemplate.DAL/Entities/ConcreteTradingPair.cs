using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using TradingBot.DAL.Shared.Abstract;
using TradingBot.DAL.Shared.Entities;

namespace BotTemplate.DAL.Entities {
    [ExcludeFromCodeCoverage]
    [Table("TradingPairs")]
    public class ConcreteTradingPair : TradingPair, IHasId {
        [Column(TypeName = "decimal(36,18)")]
        public decimal LargeDropPercentage { get; set; }
        [Column(TypeName = "decimal(36,18)")]
        public decimal LargeDropSellPriceMultiplier { get; set; }
        [Column(TypeName = "decimal(36,18)")]
        public decimal InTroublePercentage { get; set; }
    }
}