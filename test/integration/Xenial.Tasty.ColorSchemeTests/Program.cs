using System;
using System.Threading.Tasks;

using Xenial.Delicious.Plugins;
using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace Xenial.Delicious.ColorSchemeTests
{
    internal static class Program
    {
        static Program() => TastyDefaultScope
               .UseNamedPipesTransport()
               .UseRemoteReporter();

        internal static async Task Main(string[] args)
        {
            ConsoleReporter.Scheme = new ColorScheme
            {
                ErrorIcon = "🤬",
                ErrorColor = ConsoleColor.Magenta,
                SuccessIcon = "🥰",
                SuccessColor = ConsoleColor.White
            };

            Describe("ColorSchemes", () =>
            {
                It("can be adjusted", () => true);
                It("can be whatever you want", () => false);
            });

            await Run(args);
        }
    }
}
