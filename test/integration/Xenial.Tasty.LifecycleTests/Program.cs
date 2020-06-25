using System;
using System.Threading.Tasks;

using FakeItEasy;

using static Xenial.Tasty;

namespace Xenial.Delicious.LifecycleTests
{
    class Program
    {
        class Calculator
        {
            private Action<int> Printer;
            internal Calculator(Action<int> printer)
                => Printer = printer;

            internal int Sum;

            internal void Add(int a, int b)
            {
                Sum += a + b;
                Print();
            }

            internal void Sub(int a, int b)
            {
                var r =  a - b;
                Sum += r;
                Print();
            }

            private void Print()
                => Printer(Sum);

            internal void Reset() 
                => Sum = 0;
        }

        static void Main(string[] args)
        {
            Describe("LifecycleTests", () =>
            {
                Describe("with expected side effects", () =>
                {
                    Calculator? calc = null;
                    Action<int>?  printer = null;

                    BeforeEach(() =>
                    {
                        printer = A.Fake<Action<int>>();
                        calc = new Calculator(printer);
                        return Task.CompletedTask; // API is not ready yet, so we have to deal with tasks even if it's sync
                    });

                    It("should use Tasty's features to do addition", () =>
                    {
                        calc!.Add(1, 2);

                        A.CallTo(() => printer!(3)).MustHaveHappened();
                    });

                    It("should use Tasty's features to do subtraction", () =>
                    {
                        calc!.Sub(1, 2);

                        A.CallTo(() => printer!(-1)).MustHaveHappened();
                    });
                });

                Describe("with side effects", () =>
                {
                    var printer = A.Fake<Action<int>>();
                    var calc = new Calculator(printer);

                    AfterEach(() =>
                    {
                        calc.Reset();
                        return Task.CompletedTask; // API is not ready yet, so we have to deal with tasks even if it's sync
                    });

                    It("should do addition", () =>
                    {
                        calc.Add(1, 1);

                        A.CallTo(() => printer(2)).MustHaveHappened();
                    });

                    It("should do subtraction", () =>
                    {
                        calc.Sub(2, 2);

                        A.CallTo(() => printer(0)).MustHaveHappened();
                    });
                });
            });

            Run();
        }
    }
}
