using System;
using System.Threading.Tasks;

using FakeItEasy;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

TastyDefaultScope
    .UseNamedPipesTransport()
    .UseRemoteReporter();

Describe("LifecycleTests", () =>
{
    Describe("with expected side effects", () =>
    {
        Calculator? calc = null;
        Action<int>? printer = null;

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
            return Task.CompletedTask; //TODO: API is not ready yet, so we have to deal with tasks even if it's sync
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

await Run(args);

internal class Calculator
{
    private readonly Action<int> printer;
    internal Calculator(Action<int> printer)
        => this.printer = printer;

    internal int Sum;

    internal void Add(int a, int b)
    {
        Sum += a + b;
        Print();
    }

    internal void Sub(int a, int b)
    {
        var r = a - b;
        Sum += r;
        Print();
    }

    private void Print()
        => printer(Sum);

    internal void Reset()
        => Sum = 0;
}
