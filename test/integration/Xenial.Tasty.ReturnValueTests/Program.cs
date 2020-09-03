﻿using System;
using System.Threading.Tasks;

using Shouldly;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

namespace Xenial.Delicious.ReturnValueTests
{
    internal static class Program
    {
        static Program() => TastyDefaultScope
            .UseNamedPipesTransport()
            .UseRemoteReporter();

        internal static async Task Main(string[] args)
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

            await Run(args);
        }
    }
}
