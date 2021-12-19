using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace FocusedTests
{
    internal class Program
    {
        static Program() => ConsoleReporter.Register();

        private static void Main(string[] args)
        {
            Describe(nameof(FocusedTests), () =>
            {
                It("Should not run #1", () => false);
                FIt("Should run #1", () => true);
                FIt("Should run #2", () => true);
                It("Should not run #2", () => false);

                FDescribe("all those tests and groups are in focus mode", () =>
                {
                    It("Focused #1", () => true);

                    Describe("even if nested", () =>
                    {
                        It("Focused #2", () => true);
                        It("Focused #3", () => true);
                    });
                });
            });

            Run(args);
        }
    }
}
