using BotTemplate.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using TradingBot.DAL.Shared;
using TradingBot.DAL.Shared.Constants;

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
            modelBuilder.Entity<ConcreteTradingPair>().HasData(this.CreateBtcFiatPair(1, KnownCoins.BTC, KnownCoins.USDT));
            modelBuilder.Entity<ConcreteTradingPair>().HasData(this.CreateBtcFiatPair(1200, KnownCoins.BTC, KnownCoins.EUR));           
            modelBuilder.Entity<ConcreteTradingPair>().HasData(this.CreateBtcFiatPair(1400, KnownCoins.EUR, KnownCoins.USDT));
            modelBuilder.Entity<ConcreteTradingPair>().HasData(this.CreateBtcFiatPair(1600, KnownCoins.BTC, KnownCoins.BUSD));
        }
        
        private ConcreteTradingPair CreateBtcFiatPair(int id, string baseCurrency, string quoteCurrency) {
            var btcUsdtTradingPair = this.CreateTradingPair(
                id,
                baseCurrency,
                quoteCurrency,
                6, 
                2);

            btcUsdtTradingPair.IsBuyingEnabled = false;
            return btcUsdtTradingPair;
        }
        
        private ConcreteTradingPair CreateTradingPair(
            int id,
            string baseCurrency,
            string quoteCurrency,
            int amountPrecision, 
            int pricePrecision,
            decimal amountToBuyOrSell = 0m) {
            return new ConcreteTradingPair() {
                Id = id,
                AmountPrecision = amountPrecision,
                PricePrecision = pricePrecision,
                Symbol = baseCurrency + quoteCurrency,
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency,
                AmountToSellOrBuyInQuoteCurrency = amountToBuyOrSell,
                IsBuyingEnabled = true,
                IsActive = true
            };
        }
    }
}