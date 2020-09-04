using System;
using System.IO;
using System.Linq;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

TastyDefaultScope
    .UseNamedPipesTransport()
    .UseRemoteReporter();

Describe("Data driven tests", async () =>
{
    var numbers = Enumerable.Range(0, 3);

    foreach (var number in numbers)
    {
        It($"can be as simple as a foreach #{number}", () => true);
    }

    _ = numbers
        .Select((n) => It($"can be a linq expression #{n}", () => true))
        .ToList();

    using (var reader = File.OpenText("data.txt"))
    {
        var fileText = await reader.ReadToEndAsync();
        var cases = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var @case in cases)
        {
            It($"can be anything, your imagination is the limit #{@case}", () => true);
        }
    }
});

return await Run(args);
