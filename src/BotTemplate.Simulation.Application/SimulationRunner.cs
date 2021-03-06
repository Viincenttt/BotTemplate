using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BotTemplate.DAL;
using BotTemplate.DAL.Entities;
using BotTemplate.Domain.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using TradingBot.DAL.Shared.Constants;
using TradingBot.DAL.Shared.Extensions;
using TradingBot.Domain.Shared;
using TradingBot.Domain.Shared.ActiveTradingPairServices;
using TradingBot.Domain.Shared.Configuration;
using TradingBot.Domain.Shared.Support;
using TradingBot.Exchange.Domain.Abstract;
using TradingBot.Exchange.Domain.Models.Response;
using TradingBot.Models.Enums;
using TradingBot.Simulation.Shared.FakeImplementations;
using TradingBot.Simulation.Shared.HistoricDataLoader.CandleStick;
using TradingBot.Utilities.Clock;

namespace BotTemplate.Simulation.Application {
    public class SimulationRunner {
        private static readonly IDictionary<string, DateTime> _servicesToRunAtInterval = new Dictionary<string, DateTime>();

        private readonly CandleStickIntervalOptions _candleStickIntervalOptions;
        private readonly ActiveTradingPairCheckerOptions _activeTradingPairCheckerOptions;
        private readonly AmountToBuyOrSellUpdaterOptions _amountToBuyOrSellUpdaterOptions;
        private readonly BalanceStorageOptions _balanceStorageOptions;
        private readonly BalanceStorageService<ConcreteTradingPair, ConcreteTrade> _balanceStorageService;
        private readonly TakeProfitOptions _takeProfitOptions;
        private readonly TakeProfitService<ConcreteTradingPair, ConcreteTrade> _takeProfitService;
        private readonly TradeEngine<TradingPairContext, ConcreteTradingPair, ConcreteTrade> _tradeEngine;
        private readonly ActiveTradingPairChecker<ConcreteTradingPair, ConcreteTrade> _activeTradingPairChecker;
        private readonly AmountToBuyOrSellUpdater<ConcreteTradingPair, ConcreteTrade> _amountToBuyOrSellUpdater;
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;
        private readonly IHistoricCandleStickRetriever _historicCandleStickRetriever;
        private readonly FakeDateTime _dateTimeWrapper;
        private readonly IExchangeApi _exchangeApi;
        private readonly IQuoteCurrencyService _quoteCurrencyService;

        public SimulationRunner(
            IOptions<CandleStickIntervalOptions> candleStickIntervalOptions,
            IOptions<ActiveTradingPairCheckerOptions> activeTradingPairCheckerOptions,
            IOptions<AmountToBuyOrSellUpdaterOptions> amountToBuyOrSellUpdaterOptions,
            IOptions<BalanceStorageOptions> balanceStorageOptions,
            IOptions<TakeProfitOptions> takeProfitOptions,
            TradeEngine<TradingPairContext,ConcreteTradingPair, ConcreteTrade> tradeEngine,
            ActiveTradingPairChecker<ConcreteTradingPair, ConcreteTrade> activeTradingPairChecker,
            AmountToBuyOrSellUpdater<ConcreteTradingPair, ConcreteTrade> amountToBuyOrSellUpdater,
            BalanceStorageService<ConcreteTradingPair, ConcreteTrade> balanceStorageService,
            TakeProfitService<ConcreteTradingPair, ConcreteTrade> takeProfitService,
            IApplicationDbContextFactory applicationDbContextFactory,
            IHistoricCandleStickRetriever historicCandleStickRetriever,
            IDateTimeWrapper dateTimeWrapper,
            IExchangeApi exchangeApi,
            IQuoteCurrencyService quoteCurrencyService) {
            this._candleStickIntervalOptions = candleStickIntervalOptions.Value;
            this._activeTradingPairCheckerOptions = activeTradingPairCheckerOptions.Value;
            this._amountToBuyOrSellUpdaterOptions = amountToBuyOrSellUpdaterOptions.Value;
            this._balanceStorageOptions = balanceStorageOptions.Value;
            this._balanceStorageService = balanceStorageService;
            this._takeProfitService = takeProfitService;
            this._takeProfitOptions = takeProfitOptions.Value;
            this._tradeEngine = tradeEngine;
            this._activeTradingPairChecker = activeTradingPairChecker;
            this._amountToBuyOrSellUpdater = amountToBuyOrSellUpdater;
            this._historicCandleStickRetriever = historicCandleStickRetriever;
            this._applicationDbContextFactory = applicationDbContextFactory;
            this._dateTimeWrapper = (FakeDateTime)dateTimeWrapper;
            this._exchangeApi = exchangeApi;
            this._quoteCurrencyService = quoteCurrencyService;
        }

        public async Task Execute() {
            Log.Information("Starting simulation run");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (IApplicationDbContext dbContext = this._applicationDbContextFactory.Create()) {
                await this.ResetDatabase(dbContext);

                IEnumerable<DateTime> timestampsToSimulate = this.GetTimestampsToSimulate(dbContext);
                this._dateTimeWrapper.FakeTime = timestampsToSimulate.First();

                foreach (DateTime timestamp in timestampsToSimulate) {
                    this._dateTimeWrapper.FakeTime = timestamp;

                    await ExecuteServiceAtInterval("ActiveTradingPairChecker", this._activeTradingPairChecker.Execute, this._activeTradingPairCheckerOptions.Enabled, this._activeTradingPairCheckerOptions.Interval, this._dateTimeWrapper.FakeTime);
                    await ExecuteServiceAtInterval("TradeEngine", this._tradeEngine.Execute, true, TimeSpan.FromMinutes(1), this._dateTimeWrapper.FakeTime);
                    await ExecuteServiceAtInterval("AmountToBuyOrSellUpdater", this._amountToBuyOrSellUpdater.Execute, this._amountToBuyOrSellUpdaterOptions.Enabled, this._amountToBuyOrSellUpdaterOptions.Interval, this._dateTimeWrapper.FakeTime);
                    await ExecuteServiceAtInterval("BalanceStorageService", this._balanceStorageService.Execute, this._balanceStorageOptions.Enabled, this._balanceStorageOptions.Interval, this._dateTimeWrapper.FakeTime);
                    await ExecuteServiceAtInterval("TakeProfitService", this._takeProfitService.Execute, this._takeProfitOptions.Enabled, this._takeProfitOptions.Interval, this._dateTimeWrapper.FakeTime);
                }

                await this.OutputSimulationResults(dbContext);
            }            

            stopwatch.Stop();
            Log.Information("Completed simulation in {totalSeconds} seconds", stopwatch.Elapsed.TotalSeconds);
        }

        private async Task OutputSimulationResults(IApplicationDbContext dbContext) {
            IDictionary<string, decimal> latestPricesOfTradingPairs = (await this._exchangeApi.GetDailyStatistics()).ToDictionary(x => x.Symbol, x => x.LastPrice);
            var openTrades = dbContext.Trades.GetOpenTrades<ConcreteTradingPair, ConcreteTrade>().ToList();
            List<BalanceResponse> balances = (await this._exchangeApi.GetBalances()).Balances.ToList();
            foreach (string quoteCurrency in this._quoteCurrencyService.GetQuoteCurrenciesToTradeOn()) {
                decimal openTradesBalance = openTrades.Where(x => x.TradingPair.Symbol.EndsWith(quoteCurrency)).Sum(x => x.Amount * latestPricesOfTradingPairs[x.TradingPair.Symbol]);
                decimal freeBalance = balances.First(x => x.Symbol == quoteCurrency).Free;
                decimal totalBalance = openTradesBalance + freeBalance;
                decimal initialBalance = FakeExchangeApi<ConcreteTradingPair, ConcreteTrade>._initialBalances[quoteCurrency];
                decimal profitPercentage = (totalBalance / initialBalance * 100) - 100;
                decimal usdtValue = this._quoteCurrencyService.ConvertAmount(totalBalance, quoteCurrency, KnownCoins.USD);

                Log.Information("Statistics of {quoteCurrency} - Free={free} Open={open} - Total={total} - UsdtValue={usdtValue} Profit={profit}%",
                    quoteCurrency,
                    Math.Round(freeBalance, 8),
                    Math.Round(openTradesBalance, 8),
                    Math.Round(totalBalance, 8),
                    Math.Round(usdtValue, 8),
                    Math.Round(profitPercentage, 4));
            }

            int maximumNumberOfOpenTrades = ((FakeExchangeApi<ConcreteTradingPair, ConcreteTrade>)this._exchangeApi).MaximumNumberOfOpenTrades;
            int numberOfCompletedTrades = dbContext.Trades.Count(x => x.Side == TradeSide.Buy && x.Status == TradeStatus.Filled);
            Log.Information("Maximum number of open trades: {maximumNumberOfOpenTrades}", maximumNumberOfOpenTrades);
            Log.Information("Total number of completed trades: {numberOfCompletedTrades}", numberOfCompletedTrades);
        }

        private IEnumerable<DateTime> GetTimestampsToSimulate(IApplicationDbContext dbContext) {
            return this._historicCandleStickRetriever
                .GetAll(dbContext.TradingPairs.First().Symbol)
                .OrderBy(x => x.OpenTime)
                .Skip(this._candleStickIntervalOptions.NumberOfCandleSticksToRetrieve)
                .Select(x => x.OpenTime)
                .ToList();
        }

        private async Task ExecuteServiceAtInterval(string serviceName, Func<Task> function, bool isEnabled, TimeSpan interval, DateTime currentDateTime) {
            if (!isEnabled) {
                return;
            }

            if (!_servicesToRunAtInterval.ContainsKey(serviceName) || _servicesToRunAtInterval[serviceName] + interval <= currentDateTime) {
                await function();

                _servicesToRunAtInterval[serviceName] = currentDateTime;
            }
        }

        private async Task ResetDatabase(IApplicationDbContext dbContext) {
            ApplicationDbContext db = (ApplicationDbContext)dbContext;
            db.Database.ExecuteSqlRaw("TRUNCATE TABLE [Trades]");
            db.Database.ExecuteSqlRaw("TRUNCATE TABLE [Balances]");
            db.Database.ExecuteSqlRaw("TRUNCATE TABLE [TakeProfitOrders]");

            foreach (var tradingPair in dbContext.TradingPairs) {
                tradingPair.DisabledUntil = null;
                tradingPair.LastTimestampWithLowVolume = null;
            }
            await dbContext.SaveChangesAsync();
        }
    }
}