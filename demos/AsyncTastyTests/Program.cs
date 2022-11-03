using System;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Plugins;
using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace AsyncTastyTests
{
    internal class Program
    {
        static Program() => ConsoleReporter.Register();

        private static async Task Main(string[] args)
        {
            It("I'm async and happy about it", async () =>
            {
                await Task.Delay(100); // Do async computation
                return true; //We could omit that, but that's for the next lesson
            });

            await Run(args);
        }
    }
}
