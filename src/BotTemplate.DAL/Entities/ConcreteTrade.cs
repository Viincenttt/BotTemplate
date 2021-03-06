using System.ComponentModel.DataAnnotations.Schema;
using TradingBot.DAL.Shared.Entities;

namespace BotTemplate.DAL.Entities {
    [Table("Trades")]
    public class ConcreteTrade : Trade<ConcreteTradingPair> {
    }
}