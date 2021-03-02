using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TradingBot.Configuration.Configuration;
using TradingBot.Exchange.Binance;
using TradingBot.Exchange.Domain.Abstract;
using TradingBot.Utilities.Clock;

namespace BotTemplate.Application {
    class Program {
        static async Task Main(string[] args) {
            IConfiguration configuration = ConfigurationFactory.GetConfiguration();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            
            ServiceProvider services = RegisterServices(configuration);

            var exchangeClient = services.GetService<IExchangeApi>();
            var balances = await exchangeClient.GetBalances();
            foreach (var balance in balances.Balances) {
                if (balance.Symbol == "BTC" || balance.Symbol == "USDT") {
                    Log.Information("Balance for Symbol={Symbol} Balance={Balance}", 
                        balance.Symbol, 
                        balance.Free + balance.Locked);
                }
            }
            
            Console.Read();
        }

        private static ServiceProvider RegisterServices(IConfiguration configuration) {
            IServiceCollection services = new ServiceCollection();
            
            TradingBot.Exchange.Binance.ServiceRegistration.RegisterServices(services, configuration);
            TradingBot.Utilities.ServiceRegistration.RegisterServices(services, configuration);
            
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            TradingBot.Exchange.Binance.ServiceRegistration.SetExchangeDefaultOptions(serviceProvider);

            return serviceProvider;
        }
    }
}