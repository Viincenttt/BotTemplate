using System;
using BotTemplate.DAL.Entities;
using TradingBot.Domain.Shared.ActiveTradingPairServices;
using TradingBot.Domain.Shared.Support;

namespace BotTemplate.Domain.Support {
    public class TradingPairFactory : ITradingPairFactory<ConcreteTradingPair> {
        public ConcreteTradingPair Create(ActiveTradingPairContext<ConcreteTradingPair> tradingPairExchangeContext, ConcreteTradingPair template, DateTime? disabledUntil) {
            return new ConcreteTradingPair() {
                Symbol = tradingPairExchangeContext.Symbol,
                BaseCurrency = tradingPairExchangeContext.BaseAsset,
                QuoteCurrency = tradingPairExchangeContext.QuoteAsset,
                AmountToSellOrBuyInQuoteCurrency = template.AmountToSellOrBuyInQuoteCurrency,
                IsBuyingEnabled = false,
                IsActive = false,
                AmountPrecision = tradingPairExchangeContext.AmountPrecision,
                PricePrecision = tradingPairExchangeContext.PricePrecision,
                DailyPriceChangePercentage = tradingPairExchangeContext.PriceChangePercent,
                DailyVolume = tradingPairExchangeContext.DailyQuoteVolume,
                DisabledUntil = disabledUntil,
                LatestPrice = tradingPairExchangeContext.LastPrice
            };
        }
    }
}