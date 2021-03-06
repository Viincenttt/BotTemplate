using System.Diagnostics.CodeAnalysis;
using BotTemplate.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingBot.DAL.Shared;

namespace BotTemplate.DAL {
    [ExcludeFromCodeCoverage]
    public static class ServiceRegistration {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration) {
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            services.AddTransient<IApplicationDbContextFactory, ApplicationDbContextFactory>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();            
            
            services.AddTransient<ITradingBotBaseDbContextFactory<ConcreteTradingPair, ConcreteTrade>, ApplicationDbContextFactory>();
            services.AddTransient<ITradingBotBaseDbContext<ConcreteTradingPair, ConcreteTrade>, ApplicationDbContext>();
        }
    }
}