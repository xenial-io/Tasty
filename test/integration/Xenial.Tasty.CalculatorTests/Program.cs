using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Shouldly;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

namespace Xenial.Delicious.CalculatorTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "By design")]
    public class Calculator
    {
        public int Add(int a, int b) => a + b;
        public int Sub(int a, int b) => a - b;
        public int Div(int a, int b)
        {
            if (b == 0)
            {
                return -1;
            }
            return a / b;
        }
    }

    internal static class Program
    {
        static Program() => TastyDefaultScope
            .UseNamedPipesTransport()
            .UseRemoteReporter();

        internal static async Task Main(string[] args)
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

            await Run(args);
        }
    }
}
