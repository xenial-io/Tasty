using System;
using System.Linq;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Describe(nameof(Tasty), () =>
            {
                TastyScopeTests.DependencyTree();
                TestExecutorTests.DefaultRuntimeCases();
                TestExecutorTests.OverloadRuntimeCases();
                TestExecutorTests.ForcingTestCases();
                TestExecutorTests.IntegrationTests();
            });

            return await Run(args);
        }
    }
}