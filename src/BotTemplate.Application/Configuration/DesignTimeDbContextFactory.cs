using System.Diagnostics.CodeAnalysis;
using BotTemplate.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TradingBot.Configuration.Configuration;

namespace BotTemplate.Application.Configuration {
    [ExcludeFromCodeCoverage]
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext> {
        public ApplicationDbContext CreateDbContext(string[] args) {
            var configuration = ConfigurationFactory.GetConfiguration();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}