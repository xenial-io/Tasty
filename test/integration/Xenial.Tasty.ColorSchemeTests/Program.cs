using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace Xenial.Delicious.ColorSchemeTests
{
    class Program
    {
        static void Main(string[] args)
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

            Run(args);
        }
    }
}
