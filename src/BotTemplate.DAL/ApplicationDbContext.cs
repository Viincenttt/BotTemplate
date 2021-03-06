using BotTemplate.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using TradingBot.DAL.Shared;

namespace BotTemplate.DAL {
    public interface IApplicationDbContext : ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> {
    }
    
    public class ApplicationDbContext : TradingBotDbContext<ConcreteTradingPair, ConcreteTrade>, IApplicationDbContext {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            
            this.SeedTradingPairs(modelBuilder);
        }

        protected virtual void SeedTradingPairs(ModelBuilder modelBuilder) {
            // TODO
        }
    }
}