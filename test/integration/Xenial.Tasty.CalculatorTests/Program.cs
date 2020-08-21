using System;

using Shouldly;

using static Xenial.Tasty;

namespace Xenial.Delicious.CalculatorTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "By design")]
    public class Calculator
    {
        public int Add(int a, int b) => a + b;
        public int Sub(int a, int b) => a - b;
        public int Div(int a, int b) => a / b;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var sut = new Calculator();

            It("should add", () =>
            {
                sut.Add(1, 2).ShouldBe(3);
            });

            It("should subtract", () =>
            {
                sut.Sub(1, 2).ShouldBe(-1);
            });

            It("should not divide by 0", () =>
            {
                sut.Div(1, 0).ShouldBe(-1);
            });

            Run(args);
        }
    }
}
