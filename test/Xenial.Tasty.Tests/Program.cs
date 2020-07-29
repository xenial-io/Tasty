using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;

using static Xenial.Delicious.Tests.TastyScopeTests;
using static Xenial.Delicious.Tests.TestExecutorTests;
using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static class Program
    {
        static Program() => ConsoleReporter.Register();

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