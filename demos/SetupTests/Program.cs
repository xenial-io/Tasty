using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace SetupTests
{
    class Calculator
    {
        public int Result { get; private set; }

        public int Add(int a, int b) => Result += a + b;

        public int Reset() => Result = 0;
    }

    class Program
    {
        static Program() => ConsoleReporter.Register();

        static void Main(string[] args)
        {
            var calculator = new Calculator();

            BeforeEach(() => calculator.Add(1, 1));
            AfterEach(() => calculator.Reset());

            It("2 + (2 + 4) = 8", () =>
            {
                calculator.Add(2, 4);
                return calculator.Result == 8;
            });

            It("2 + (10 + 10) = 22", () =>
            {
                calculator.Add(10, 10);
                return calculator.Result == 22;
            });

            Run(args);
        }
    }
}
