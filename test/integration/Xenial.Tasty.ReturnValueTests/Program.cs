using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace Xenial.Delicious.ReturnValueTests
{
    class Program
    {
        static void Main(string[] args)
        {
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
                    Sut();
                });

                It("can be booleans", () => true);

                It("can be tuples to provide context", () =>
                {
                    return (false, "This is the reason for the fail");
                });

                It("can be async", async () =>
                {
                    await Task.CompletedTask;
                    return true;
                });
            });

            Run(args);
        }
    }
}
