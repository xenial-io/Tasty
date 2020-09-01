using System.Threading.Tasks;

using Xenial.Delicious.Plugins;

using static Xenial.Delicious.Tests.Plugins.InvalidPluginExceptionTests;
using static Xenial.Delicious.Tests.TastyCommanderTests;
using static Xenial.Delicious.Tests.TastyScopeTests;
using static Xenial.Delicious.Tests.TestExecutorTests;
using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static class Program
    {
        static Program() => TastyDefaultScope
            .UseConsoleReporter()
            .UseRemoteReporter()
            .UseNamedPipesTransport()
        ;

        public static async Task<int> Main(string[] args)
        {
            Describe(nameof(Tasty), () =>
            {
                Commander();
                DependencyTree();
                DefaultRuntimeCases();
                OverloadRuntimeCases();
                ForcingTestCases();
                InvalidPluginException();
                IntegrationTests();
            });

            return await Run(args);
        }
    }
}
