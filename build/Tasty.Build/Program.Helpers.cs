using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using static SimpleExec.Command;

namespace Tasty.Build
{
    internal static partial class Program
    {
        private static async Task EnsureTools()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
            {
                try
                {
                    await RunAsync("dotnet", "format --version");
                }
                catch (SimpleExec.ExitCodeException)
                {
                    //Can't find dotnet format, assuming tools are not installed
                    await RunAsync("dotnet", "tool restore");
                }
            }
            else
            {
                await RunAsync("dotnet", "tool restore");
            }
        }

        private static string Tabify(string s)
            => string.Join(
                Environment.NewLine,
                s.Split("\n").Select(s => $"\t{s}")
            );
    }
}
