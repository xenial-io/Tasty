using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace Xenial.Delicious.ForcedTests
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Describe("ForcedTests", () =>
            {
                It("Should not run #1", () => false);
                FIt("Should run #1", () => true);
                FIt("Should run #2", () => true);
                It("Should not run #2", () => false);
            });

            return await Run(args);
        }
    }
}
