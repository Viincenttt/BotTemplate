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
using TradingBot.Models.Enums;

namespace BotTemplate.Domain.TradeContextServices {
    public class SellAlgorithm : ITradeContextService<TradingPairContext, ConcreteTradingPair, ConcreteTrade> {
        private readonly ITradeCrudService<ConcreteTradingPair, ConcreteTrade> _tradeCrudService;

        public SellAlgorithm(ITradeCrudService<ConcreteTradingPair, ConcreteTrade> tradeCrudService) {
            _tradeCrudService = tradeCrudService;
        }
        
        public async Task Execute(
            TradeContext<TradingPairContext, ConcreteTradingPair> tradeContext, 
            ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext) {
            
            foreach (var filledBuyTrade in this.GetFilledBuyOrders(dbContext)) {
                await this.CreateCorrespondingSellTrade(dbContext, filledBuyTrade);
            }
        }
        
        private async Task CreateCorrespondingSellTrade(ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext, ConcreteTrade buyTrade) {
            decimal amount = buyTrade.AmountFilled;
            decimal sellPriceTarget = 1.01m;
            decimal price = buyTrade.Price * sellPriceTarget;

            Log.Information("Buy order was fulfilled, placing sell order: " +
                            "Symbol={symbol} " +
                            "BuyOrderId={buyOrderId} " +
                            "Amount={amount} " +
                            "BuyPrice={buyPrice} " +
                            "Price={price} " +
                            "SellPriceTarget={sellPriceMultiplier}",
                buyTrade.TradingPair.Symbol,
                buyTrade.Id,
                buyTrade.Amount,
                buyTrade.Price,
                price,
                sellPriceTarget);

            await this._tradeCrudService.PlaceSellOrder(dbContext, buyTrade, amount, price);
        }
        
        private IEnumerable<ConcreteTrade> GetFilledBuyOrders(ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext) {
            return dbContext.Trades.Where(x =>
                    x.Side == TradeSide.Buy &&
                    x.Status == TradeStatus.Filled &&
                    x.LatestSellTradeId == null)
                .ToList();
        }
    }
}