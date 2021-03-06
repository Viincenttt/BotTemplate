using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace BotTemplate.Simulation.Application {
    [ExcludeFromCodeCoverage]
    class Program { 
        static async Task Main(string[] args) {
            Bootstrapper bootstrapper = new Bootstrapper();
            await bootstrapper.Bootstrap();
            Console.Read();
        }
    }
}