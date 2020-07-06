using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace AsyncTastyTests
{
    class Program
    {
        static async Task Main(string[] args)
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
