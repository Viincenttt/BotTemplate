using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using BotTemplate.DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TradingBot.Configuration.Configuration;
using TradingBot.Simulation.Shared.HistoricDataLoader;

namespace BotTemplate.Simulation.Application {
    [ExcludeFromCodeCoverage]
    public class Bootstrapper {
        private const string LogPath = @"Logs\\tradebot-log.txt";
        private readonly IConfiguration _configuration = ConfigurationFactory.GetConfiguration();

        public async Task Bootstrap() {
            this.DeleteOldLogFiles();
            this.InitializeSerilog();
            await this.DownloadHistoricCandleSticks();
            await this.RunSimulation();
        }

        private async Task RunSimulation() {
            ServiceProvider serviceProvider = ServiceRegistration.RegisterSimulationServices(this._configuration);
            SimulationRunner simulationRunner = serviceProvider.GetService<SimulationRunner>();
            await simulationRunner.Execute();
        }

        private async Task DownloadHistoricCandleSticks() {
            ServiceProvider serviceProvider = ServiceRegistration.RegisterHistoricCandleStickDownloadServices(this._configuration);
            var historicCandleStickDownLoadService = serviceProvider.GetService<HistoricCandleStickDownloadService<ConcreteTradingPair, ConcreteTrade>>();
            await historicCandleStickDownLoadService.Execute();
        }

        private void InitializeSerilog() {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(LogPath)
                .CreateLogger();
        }

        private void DeleteOldLogFiles() {
            if (File.Exists(LogPath)) {
                File.Delete(LogPath);
            }
        }
    }
}