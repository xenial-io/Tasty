using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace DataDrivenTests
{
    class Program
    {
        static Program() => ConsoleReporter.Register();

        static void Main(string[] args)
        {
            var cases = new[]
            {
                (1, 2, 3),
                (99, 1, 100),
                (-1, 1, 0)
            };

            foreach (var (a, b, expected) in cases)
            {
                It($"{a} + {b} = {expected}", () =>
                {
                    var calculation = a + b;
                    return calculation == expected;
                });
            }

            Run(args);
        }
    }
}
