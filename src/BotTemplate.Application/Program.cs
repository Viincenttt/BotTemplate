using System;
using TradingBot.Utilities.Collections;

namespace BotTemplate.Application {
    class Program {
        static void Main(string[] args) {
            var test = new CacheWithExpiry<string>(TimeSpan.FromMinutes(1));
            test.AddOrUpdate("key", "value");
            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}