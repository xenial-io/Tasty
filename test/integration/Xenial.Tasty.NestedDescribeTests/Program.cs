using System;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

TastyDefaultScope
    .UseNamedPipesTransport()
    .UseRemoteReporter();

Describe("A group", () =>
{
    It("can contain a test", () => true);

    Describe("with nesting", () =>
    {
        It("should be allowed", () => true);
    });

    Describe("that has multiple groups", () =>
    {
        Describe("with really deep nesting", () =>
        {
            It("should be allowed", () => true);
        });
    });
});

return await Run(args);

