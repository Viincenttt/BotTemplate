using System;
using System.Threading;
using System.Threading.Tasks;
using BotTemplate.DAL.Entities;
using BotTemplate.Domain.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TradingBot.Configuration.Configuration;
using TradingBot.Domain.Shared;
using TradingBot.Exchange.Domain.Abstract;

namespace BotTemplate.Application {
    class Program {
        static async Task Main(string[] args) {
            IConfiguration configuration = ConfigurationFactory.GetConfiguration();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            
            ServiceProvider services = RegisterServices(configuration);
           
            await StartTradingBot(services);
        }

        private static async Task StartTradingBot(IServiceProvider serviceProvider) {
            var cancellationTokenSource = new CancellationTokenSource();
            var tradeEngine = serviceProvider.GetService<TradeEngine<TradingPairContext, ConcreteTradingPair, ConcreteTrade>>();

            await Task.WhenAll(
                tradeEngine.Start(cancellationTokenSource.Token));
        }

        private static ServiceProvider RegisterServices(IConfiguration configuration) {
            IServiceCollection services = new ServiceCollection();
            
            TradingBot.Exchange.Binance.ServiceRegistration.RegisterServices(services, configuration);
            TradingBot.Utilities.ServiceRegistration.RegisterServices(services, configuration);
            
            DAL.ServiceRegistration.RegisterServices(services, configuration);
            Domain.ServiceRegistration.RegisterServices(services, configuration);
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            TradingBot.Exchange.Binance.ServiceRegistration.SetExchangeDefaultOptions(serviceProvider);

            return serviceProvider;
        }
    }
}