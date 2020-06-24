using System;
using System.Linq;
using System.Threading.Tasks;

using static Xenial.Tasty;
using static Xenial.Delicious.Tests.TastyScopeTests;
using static Xenial.Delicious.Tests.TestExecutorTests;

namespace Xenial.Delicious.Tests
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Describe(nameof(Tasty), () =>
            {
                DependencyTree();
                DefaultRuntimeCases();
                OverloadRuntimeCases();
                ForcingTestCases();
                IntegrationTests();
            });

            return await Run(args);
        }
    }
}