using System;

using static Xenial.Tasty;
using Shouldly;

namespace Xenial.Delicious.CalculatorTests
{
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
            It("should add", () =>
            {
                var sut = new Calculator();
                sut.Add(1, 2).ShouldBe(3);
            });

            It("should subtract", () =>
            {
                var sut = new Calculator();
                sut.Sub(1, 2).ShouldBe(-1);
            });

            It("should not divide by 0", () =>
            {
                var sut = new Calculator();
                sut.Div(1, 0).ShouldBe(-1);
            });

            Run();
        }
    }
}
