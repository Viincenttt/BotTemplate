using System.Diagnostics.CodeAnalysis;
using BotTemplate.DAL.Entities;
using BotTemplate.Domain.Dto;
using BotTemplate.Domain.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingBot.Domain.Shared;
using TradingBot.Domain.Shared.ActiveTradingPairServices;
using TradingBot.Domain.Shared.ActiveTradingPairServices.ActiveTradingPairChecks;
using TradingBot.Domain.Shared.Configuration;
using TradingBot.Domain.Shared.Support;
using TradingBot.Domain.Shared.TradeContextServices;
using TradingBot.Domain.Shared.TradeContextServices.BuyOrderChecks;
using TradingBot.Extensions;

namespace BotTemplate.Domain {
    [ExcludeFromCodeCoverage]
    public static class ServiceRegistration {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration) {
            services.AddTransient<ITradeContextFactory<TradingPairContext, ConcreteTradingPair, ConcreteTrade>, TradeContextFactory>();
            services.AddTransient<TradeEngine<TradingPairContext, ConcreteTradingPair, ConcreteTrade>, TradeEngine<TradingPairContext, ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<IEstimatedInvestedPortfolioCalculator, EstimatedInvestedPortfolioCalculator<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<BnbRefiller<ConcreteTradingPair, ConcreteTrade>, BnbRefiller<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<ITradeCrudService<ConcreteTradingPair, ConcreteTrade>, TradeCrudService<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<IQuoteCurrencyService, QuoteCurrencyService<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<IEstimatedQuoteCurrencyBalanceCalculator, EstimatedQuoteCurrencyBalanceCalculator<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<BalanceStorageService<ConcreteTradingPair, ConcreteTrade>, BalanceStorageService<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<TakeProfitService<ConcreteTradingPair, ConcreteTrade>, TakeProfitService<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<AmountToBuyOrSellUpdater<ConcreteTradingPair, ConcreteTrade>, AmountToBuyOrSellUpdater<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<ActiveTradingPairChecker<ConcreteTradingPair, ConcreteTrade>, ActiveTradingPairChecker<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<IActiveTradingPairContextFactory<ConcreteTradingPair, ConcreteTrade>, ActiveTradingPairContextFactory<ConcreteTradingPair, ConcreteTrade>>();
            services.AddTransient<ITradingPairFactory<ConcreteTradingPair>, TradingPairFactory>();
            
            services.ConfigureAndValidate<TradeEngineOptions>(configuration.GetSection("TradingOptions:TradeEngineOptions"));
            services.ConfigureAndValidate<ActiveTradingPairCheckerOptions>(configuration.GetSection("TradingOptions:ActiveTradingPairCheckerOptions"));
            services.ConfigureAndValidate<BnbRefillOptions>(configuration.GetSection("TradingOptions:BnbRefillerOptions"));
            services.ConfigureAndValidate<AmountToBuyOrSellUpdaterOptions>(configuration.GetSection("TradingOptions:AmountToBuyOrSellUpdaterOptions"));
            services.ConfigureAndValidate<PumpAndDumpOptions>(configuration.GetSection("TradingOptions:BuyOrderChecks:PumpAndDump"));
            services.ConfigureAndValidate<MaximumPriceDifferenceCapperOptions>(configuration.GetSection("TradingOptions:MaximumPriceDifferenceCapperOptions"));
            services.ConfigureAndValidate<BalanceStorageOptions>(configuration.GetSection("TradingOptions:BalanceStorageOptions"));      
            services.ConfigureAndValidate<TakeProfitOptions>(configuration.GetSection("TradingOptions:TakeProfitOptions"));
            services.ConfigureAndValidate<QuoteCurrencyServiceOptions>(configuration.GetSection("TradingOptions:QuoteCurrencyServiceOptions"));
            services.ConfigureAndValidate<TradeCancellationOptions>(configuration.GetSection("TradingOptions:TradeCancellationOptions"));
            services.ConfigureAndValidate<BuyOrderCheckOptions>(configuration.GetSection("TradingOptions:BuyOrderCheckOptions"));
            services.ConfigureAndValidate<CandleStickIntervalOptions>(configuration.GetSection("TradingOptions:CandleStickIntervalOptions"));
            
            services.Add(new ServiceDescriptor(typeof(ITradeContextService<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), typeof(TradeStatusUpdater<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(ITradeContextService<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), typeof(MaximumPriceDifferenceCapper<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), ServiceLifetime.Transient));
            
            services.Add(new ServiceDescriptor(typeof(IBuyOrderCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), typeof(PumpAndDumpCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IBuyOrderCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), typeof(MaximumNumberOfOpenTradesForTradingPairCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IBuyOrderCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), typeof(RecentBuyOrderTradingPairCheck<TradingPairContext, ConcreteTradingPair, ConcreteTrade>), ServiceLifetime.Transient));
            
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(IsQuoteCurrencyEnabledForTrading<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(IsSymbolActiveOnExchange<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(HasNotReachedMaximumNumberOfActiveTradingPairs<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(IsPriceHighEnough<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(IsNotDisabled<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(IsVolumeHighEnough<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(MaximumDailyPriceFluctuationCheck<ConcreteTradingPair>), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IActiveTradingPairCheck<ConcreteTradingPair>), typeof(LastTimestampWithLowVolumeCheck<ConcreteTradingPair>), ServiceLifetime.Transient));
        }
    }
}