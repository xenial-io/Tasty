using System;
using System.Threading.Tasks;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

namespace Xenial.Delicious.NestedDescribeTests
{
    internal static class Program
    {
        static Program() => TastyDefaultScope
            .UseNamedPipesTransport()
            .UseRemoteReporter();

        internal static async Task<int> Main(string[] args)
        {
            Describe("A group", () =>
            {
                It("can contain a test", () => true);

                Describe("with nesting", () =>
                {
                    It("should be allowed", () => true);
                });

                Describe("that has multiple groups", () =>
                {
                    Describe("with really deep nesting", () =>
                    {
                        It("should be allowed", () => true);
                    });
                });
            });

            return await Run(args);
        }
    }
}
