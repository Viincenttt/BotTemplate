using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotTemplate.DAL.Entities;
using BotTemplate.Domain.Dto;
using Microsoft.Extensions.Options;
using TradingBot.DAL.Shared;
using TradingBot.DAL.Shared.Entities;
using TradingBot.Domain.Shared;
using TradingBot.Domain.Shared.Configuration;
using TradingBot.Domain.Shared.Dto;
using TradingBot.Exchange.Domain.Abstract;
using TradingBot.Exchange.Domain.Models.Request;
using TradingBot.Exchange.Domain.Models.Response;

namespace BotTemplate.Domain {
    public class TradeContextFactory : ITradeContextFactory<TradingPairContext, ConcreteTradingPair, ConcreteTrade> {
        private readonly IExchangeApi _exchangeApi;
        private readonly CandleStickIntervalOptions _candleStickIntervalOptions;

        public TradeContextFactory(IExchangeApi exchangeApi,IOptions<CandleStickIntervalOptions> candleStickIntervalOptions) {
            _exchangeApi = exchangeApi;
            _candleStickIntervalOptions = candleStickIntervalOptions.Value;
        }

        
        public async Task<TradeContext<TradingPairContext, ConcreteTradingPair>> Create(ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext) {
            var tradeContext = new TradeContext<TradingPairContext, ConcreteTradingPair>();
            var tradingPairContexts = await this.GetAllTradingPairContexts(tradeContext, dbContext);
            tradeContext.TradingPairContexts = tradingPairContexts;
            return tradeContext;
        }

        private async Task<IDictionary<string, TradingPairContext>> GetAllTradingPairContexts(TradeContext<TradingPairContext, ConcreteTradingPair> tradeContext, ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext) {
            var activeTradingPairs = this.GetActiveTradingPairs(dbContext);
            var createTradingPairContextTasks = Task.WhenAll(activeTradingPairs.Select(x => this.CreateTradingPairContext(x, tradeContext.RunId)));

            var result = await createTradingPairContextTasks;
            return result.ToDictionary(x => x.TradingPair.Symbol, x => x);
        }
         
        private async Task<TradingPairContext> CreateTradingPairContext(ConcreteTradingPair tradingPair, Guid runId) {
            IEnumerable<CandleStickResponse> candleSticks = await this.GetCandleSticksForTradingPair(tradingPair);

            return new TradingPairContext() {
                RunId = runId,
                TradingPair = tradingPair,
                AllCandleSticks = candleSticks,
                LatestCandleSticks = candleSticks.TakeLast(this._candleStickIntervalOptions.NumberOfCandleSticksToAnalyze)
            };
        }

        private IEnumerable<ConcreteTradingPair> GetActiveTradingPairs(ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade> dbContext) {
            return dbContext.TradingPairs.Where(x => x.IsActive).ToList();
        }

        private async Task<IEnumerable<CandleStickResponse>> GetCandleSticksForTradingPair(TradingPair tradingPair) {
            CandleStickRequest candleStickRequest = new CandleStickRequest(tradingPair.Symbol, this._candleStickIntervalOptions.Interval, this._candleStickIntervalOptions.NumberOfCandleSticksToRetrieve);
            return await this._exchangeApi.GetCandleSticks(candleStickRequest);
        }
    }
}