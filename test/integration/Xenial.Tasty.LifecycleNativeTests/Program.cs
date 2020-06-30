using System;
using FakeItEasy;
using static Xenial.Tasty;

namespace Xenial.Delicious.LifecycleNativeTests
{
    class Program
    {
        class Calculator
        {
            private Action<int> Printer;
            internal Calculator(Action<int> printer)
                => Printer = printer;

            private int Sum;

            internal void Add(int a, int b)
            {
                Sum += a + b;
                Print();
            }

            internal void Sub(int a, int b)
            {
                Sum += a - b;
                Print();
            }

            private void Print()
                => Printer(Sum);
        }

        static void Main(string[] args)
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

            Run();
        }
    }
}
