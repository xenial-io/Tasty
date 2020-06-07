using System;
using System.Linq;
using System.Runtime.InteropServices;

using static SimpleExec.Command;
using static Bullseye.Targets;
using System.Threading.Tasks;
using Tasty.Build.Helpers;

namespace Tasty.Build
{
    class Program
    {
        static async Task Main(string[] args)
        {
            XenialVersionInfo version = null;

            Target("version", () =>
            {
                version = new XenialVersionInfo();
                version.PrintVersion();
            });

            static string logOptions(string target)
                => $"/maxcpucount /nologo /verbosity:minimal /bl:./artifacts/logs/tasty.{target}.binlog";

            Target("restore", DependsOn("version"),
                () => RunAsync("dotnet", $"restore {logOptions("restore")}")
            );

            Target("build", DependsOn("restore"),
                () => RunAsync("dotnet", $"build --no-restore {logOptions("build")}")
            );

            Target("test", DependsOn("build"), async () =>
            {
                var tfms = RuntimeInformation
                            .IsOSPlatform(OSPlatform.Windows)
                            ? new[] { "net462", "netcoreapp3.1" }
                            : new[] { "netcoreapp3.1" };

                var tests = tfms
                    .Select(tfm => RunAsync("dotnet", $"run --project test/Xenial.Tasty.Tests/Xenial.Tasty.Tests.csproj --framework {tfm}"))
                    .ToArray();

                await Task.WhenAll(tests);
            });

            Target("pack", DependsOn("test"),
                () => RunAsync("dotnet", $"pack src/Xenial.Tasty/Xenial.Tasty.csproj --no-restore --no-build {logOptions("pack")}")
            );

            Target("default", DependsOn("test"));

            await RunTargetsAndExitAsync(args);
        }
    }
}
