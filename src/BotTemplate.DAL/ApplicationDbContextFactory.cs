using BotTemplate.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TradingBot.Configuration.Configuration;
using TradingBot.DAL.Shared;

namespace BotTemplate.DAL {
    public interface IApplicationDbContextFactory {
        IApplicationDbContext Create();
    }
    
    public class ApplicationDbContextFactory : IApplicationDbContextFactory, ITradingBotBaseDbContextFactory<ConcreteTradingPair, ConcreteTrade> {
        private readonly IConfiguration _configuration;

        public ApplicationDbContextFactory() {
            this._configuration = ConfigurationFactory.GetConfiguration();
        }

        public IApplicationDbContext Create() {
            return new ApplicationDbContext(this.GetOptions());
        }
        
        ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> ITradingBotBaseDbContextFactory<ConcreteTradingPair, ConcreteTrade>.Create() {
            return Create();
        }

        private DbContextOptions GetOptions() {
            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultConnection"));
            return dbContextOptionsBuilder.Options;
        }
    }
}