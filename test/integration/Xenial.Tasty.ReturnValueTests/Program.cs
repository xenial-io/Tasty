using System;
using System.Threading.Tasks;

using Shouldly;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

TastyDefaultScope
    .UseNamedPipesTransport()
    .UseRemoteReporter();

Describe("Return values", () =>
{
    It("can be void", () =>
    {
        var add = 1 + 1;
        Console.WriteLine($"1 + 2 = {add}");
    });

    It("with throwing an exception", () =>
    {
        void Sut() => throw new Exception("Foo");
        Should.Throw<Exception>(Sut);
    });

    It("can be booleans", () => true);

    It("can be tuples to provide context", () =>
    {
        return (true, "This is the reason for the fail");
    });

    It("can be async", async () =>
    {
        await Task.CompletedTask;
        return true;
    });
});

return await Run(args);
