using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace MyFirstTastyTests
{
    class Program
    {
        static Program() => ConsoleReporter.Register();

        static void Main(string[] args)
        {
            It("1 + 2 should be 3", () =>
            {
                var calculation = 1 + 2;
                var isThree = calculation == 3;
                return (isThree, $"1 + 2 should be 3 but actually was {calculation}");
            });

            Run(args); //Tell Tasty to execute the test cases
        }
    }
}
