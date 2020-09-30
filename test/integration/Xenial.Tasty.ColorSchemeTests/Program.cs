using System;

using Xenial.Delicious.Plugins;
using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

TastyDefaultScope
    .UseNamedPipesTransport()
    .UseRemoteReporter();

ConsoleReporter.Scheme = new ColorScheme
{
    ErrorIcon = "🤬",
    ErrorColor = ConsoleColor.Magenta,
    SuccessIcon = "🥰",
    SuccessColor = ConsoleColor.White
};

Describe("ColorSchemes", () =>
{
    It("can be adjusted", () => true);
    It("can be whatever you want", () => true);
});

return await Run(args);
