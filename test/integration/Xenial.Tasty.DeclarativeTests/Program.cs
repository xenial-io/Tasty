using System;
using System.Threading.Tasks;

using Xenial.Delicious.Plugins;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.DeclarativeTests
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var scope = new TastyScope()
                .UseConsoleReporter()
                .UseRemoteReporter()
                .UseNamedPipesTransport();

            var group = scope.Describe("I'm a group", () => { });

            group.It("with an test case", () => true);

            return await scope.Run(args);
        }
    }
}
