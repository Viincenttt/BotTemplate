using System.Diagnostics.CodeAnalysis;
using BotTemplate.DAL;
using BotTemplate.DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingBot.Exchange.Domain.Abstract;
using TradingBot.Extensions;
using TradingBot.Simulation.Shared.FakeImplementations;
using TradingBot.Simulation.Shared.HistoricDataLoader;
using TradingBot.Simulation.Shared.HistoricDataLoader.CandleStick;
using TradingBot.Simulation.Shared.HistoricDataLoader.Configuration;

namespace BotTemplate.Simulation.Application {
    [ExcludeFromCodeCoverage]
    public static class ServiceRegistration {
        public static ServiceProvider RegisterSimulationServices(IConfiguration configuration) {
            IServiceCollection services = new ServiceCollection();

            DAL.ServiceRegistration.RegisterServices(services, configuration);
            Domain.ServiceRegistration.RegisterServices(services, configuration);
            TradingBot.Utilities.ServiceRegistration.RegisterServices(services, configuration);
            TradingBot.Simulation.Shared.ServiceRegistration.RegisterSimulationServices(services, configuration);

            services.AddSingleton<IExchangeApi, FakeExchangeApi<ConcreteTradingPair, ConcreteTrade>>();
            services.AddSingleton<IApplicationDbContextFactory, ApplicationDbContextFactory>();
            services.AddSingleton<IExchangeCandleStickApi, FakeExchangeApi<ConcreteTradingPair, ConcreteTrade>>();
            services.AddSingleton<IHistoricCandleStickRetriever, HistoricCandleStickRetriever>();
           
            services.ConfigureAndValidate<HistoricDataOptions>(configuration.GetSection("HistoricDataSettings"));
            services.AddSingleton<SimulationRunner, SimulationRunner>();

            return services.BuildServiceProvider();
        }

        public static ServiceProvider RegisterHistoricCandleStickDownloadServices(IConfiguration configuration) {
            IServiceCollection services = new ServiceCollection();

            DAL.ServiceRegistration.RegisterServices(services, configuration);
            Domain.ServiceRegistration.RegisterServices(services, configuration);
            TradingBot.Exchange.Binance.ServiceRegistration.RegisterServices(services, configuration);
            TradingBot.Utilities.ServiceRegistration.RegisterServices(services, configuration);

            services.AddSingleton<IApplicationDbContextFactory, ApplicationDbContextFactory>();
            services.AddTransient<IFileWrapper, FileWrapper>();
            services.AddTransient<IExchangeCandleStickRetriever<ConcreteTradingPair>, ExchangeCandleStickRetriever<ConcreteTradingPair>>();
            services.AddTransient<IHistoricCandleStickUpdater<ConcreteTradingPair>, HistoricCandleStickUpdater<ConcreteTradingPair>>();
            services.AddTransient<HistoricCandleStickDownloadService<ConcreteTradingPair, ConcreteTrade>, HistoricCandleStickDownloadService<ConcreteTradingPair, ConcreteTrade>>();

            services.ConfigureAndValidate<HistoricDataOptions>(configuration.GetSection("HistoricDataSettings"));

            return services.BuildServiceProvider();
        }
    }
}