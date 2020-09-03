using System;
using System.Threading.Tasks;

using FakeItEasy;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

namespace Xenial.Delicious.LifecycleNativeTests
{
    internal static class Program
    {
        static Program() => TastyDefaultScope
            .UseNamedPipesTransport()
            .UseRemoteReporter();

        internal class Calculator
        {
            private readonly Action<int> printer;
            internal Calculator(Action<int> printer)
                => this.printer = printer;

            private int sum;

            internal void Add(int a, int b)
            {
                sum += a + b;
                Print();
            }

            internal void Sub(int a, int b)
            {
                sum += a - b;
                Print();
            }

            private void Print()
                => printer(sum);
        }

        internal static async Task Main(string[] args)
        {
            Describe("LifecycleNativeTests", () =>
            {
                (Calculator calc, Action<int> printer) CreateSut(Action<int> printer)
                {
                    var calc = new Calculator(printer);
                    return (calc, printer);
                }

                It("should use C#'s features to do addition", () =>
                {
                    var (calc, printer) = CreateSut(A.Fake<Action<int>>());

                    calc.Add(1, 2);

                    A.CallTo(() => printer(3)).MustHaveHappened();
                });

                It("should use C#'s features to do subtraction", () =>
                {
                    var (calc, printer) = CreateSut(A.Fake<Action<int>>());

                    calc.Sub(1, 2);

                    A.CallTo(() => printer(-1)).MustHaveHappened();
                });
            });

            await Run(args);
        }
    }
}
