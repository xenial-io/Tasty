using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Plugins;

using static Xenial.Tasty;

namespace Xenial.Delicious.DataDrivenTests
{
    internal static class Program
    {
        static Program() => TastyDefaultScope
            .UseNamedPipesTransport()
            .UseRemoteReporter();

        internal static async Task<int> Main(string[] args)
        {
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
        }
    }
}
