using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotTemplate.DAL.Entities;
using BotTemplate.Domain.Dto;
using Serilog;
using TradingBot.DAL.Shared;
using TradingBot.Domain.Shared.Dto;
using TradingBot.Domain.Shared.Support;
using TradingBot.Domain.Shared.TradeContextServices;
using TradingBot.Domain.Shared.TradeContextServices.BuyOrderChecks;
using TradingBot.Exchange.Binance.Exceptions;

namespace BotTemplate.Domain.TradeContextServices {
    public class BuyAlgorithm : ITradeContextService<TradingPairContext, ConcreteTradingPair, ConcreteTrade> {
        private readonly ITradeCrudService<ConcreteTradingPair, ConcreteTrade> _tradeCrudService;
        private readonly IEnumerable<IBuyOrderCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>> _buyOrderChecks;

        public BuyAlgorithm(
            ITradeCrudService<ConcreteTradingPair, ConcreteTrade> tradeCrudService, 
            IEnumerable<IBuyOrderCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>> buyOrderChecks) {
            
            _tradeCrudService = tradeCrudService;
            _buyOrderChecks = buyOrderChecks;
        }

        public async Task Execute(TradeContext<TradingPairContext, ConcreteTradingPair> tradeContext, ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext) {
            var tradingPairContexts = tradeContext.TradingPairContexts
                .Select(x => x.Value)
                .Where(x => x.TradingPair.IsBuyingEnabled);
            
            Log.Debug("Number of tradingpairs with buying enabled Count={count}", tradingPairContexts.Count());
            HashSet<string> placedBuyOrdersForQuoteCurrencies = new HashSet<string>();
            foreach (TradingPairContext tradingPairContext in tradingPairContexts) {
                if (tradingPairContext.CurrentCandleStick == null) {
                    return;
                }
                
                string quoteCurrency = tradingPairContext.TradingPair.QuoteCurrency;
                if (this.ShouldPlaceBuyOrder(dbContext, tradingPairContext) && !placedBuyOrdersForQuoteCurrencies.Contains(quoteCurrency)) {
                    await this.PlaceBuyOrder(dbContext, tradingPairContext);
                    placedBuyOrdersForQuoteCurrencies.Add(quoteCurrency);
                }
            }
        }
        
        private async Task PlaceBuyOrder(ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext, TradingPairContext tradingPairContext) {
            using (var transaction = dbContext.BeginTransaction()) {
                decimal amountToBuyInQuoteCurrency = tradingPairContext.TradingPair.AmountToSellOrBuyInQuoteCurrency;
                decimal buyPrice = tradingPairContext.CurrentCandleStick.Close;
                decimal amountToBuyInBaseCurrency = amountToBuyInQuoteCurrency / buyPrice;
                
                try { 
                    await this._tradeCrudService.PlaceBuyOrder(dbContext, tradingPairContext.TradingPair, amountToBuyInBaseCurrency, buyPrice);                    
                }
                catch (BinanceInsufficientBalanceException) {
                    Log.Warning("Unable to place buy order for Symbol={symbol} AmountToBuy={amountToBuy}, because we do not have enough funds",
                        tradingPairContext.TradingPair.Symbol,
                        amountToBuyInQuoteCurrency);
                }
                finally {
                    await transaction.CommitAsync();
                }
            }
        }
        
        private bool ShouldPlaceBuyOrder(ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext, TradingPairContext tradingPairContext) {
            foreach (var buyOrderCheck in this._buyOrderChecks) {
                if (!buyOrderCheck.ShouldPlaceBuyOrder(dbContext, tradingPairContext)) {
                    return false;
                }
            }
            
            Log.Information("All order checks have passed, we should place a buy order for Symbol={Symbol}",
                tradingPairContext.TradingPair.Symbol);

            return true;
        }
    }
}