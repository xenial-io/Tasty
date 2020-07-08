using System;
using System.Linq;
using System.Runtime.InteropServices;

using static SimpleExec.Command;
using static Bullseye.Targets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tasty.Build
{
    static partial class Program
    {
        static async Task Main(string[] args)
        {
            static string logOptions(string target)
                => $"/maxcpucount /nologo /verbosity:minimal /bl:./artifacts/logs/tasty.{target}.binlog";

            const string Configuration = "Release";

            Func<string> properties = () => string.Join(" ", new Dictionary<string, string>
            {
                ["Configuration"] = Configuration,
            }.Select(p => $"/P:{p.Key}=\"{p.Value}\""));

            Target("ensure-tools", () => EnsureTools());

            Target("clean", DependsOn("ensure-tools"),
                () => RunAsync("dotnet", $"rimraf . -i **/bin/**/*.* -i **/obj/**/*.* -i artifacts/**/*.* -e node_modules/**/*.* -e build/**/*.* -q")
            );

            Target("lint", DependsOn("ensure-tools"),
                () => RunAsync("dotnet", $"format --check --verbosity detailed")
            );

            Target("format", DependsOn("ensure-tools"),
                () => RunAsync("dotnet", $"format")
            );

            Target("restore", DependsOn("lint"),
                () => RunAsync("dotnet", $"restore {logOptions("restore")}")
            );

            Target("build", DependsOn("restore"),
                () => RunAsync("dotnet", $"build --no-restore -c {Configuration} {logOptions("build")} {properties()}")
            );

            Target("test", DependsOn("build"), async () =>
            {
                var (fullFramework, netcore) = FindTfms();

                var tfms = RuntimeInformation
                            .IsOSPlatform(OSPlatform.Windows)
                            ? new[] { fullFramework, netcore }
                            : new[] { netcore };

                var tests = tfms
                    .Select(tfm => RunAsync("dotnet", $"run --project test/Xenial.Tasty.Tests/Xenial.Tasty.Tests.csproj --no-build --no-restore --framework {tfm} -c {Configuration} {properties()}"))
                    .ToArray();

                await Task.WhenAll(tests);
            });

            Target("pack", DependsOn("test"),
                () => RunAsync("dotnet", $"pack src/Xenial.Tasty/Xenial.Tasty.csproj --no-restore --no-build -c {Configuration} {logOptions("pack")} {properties()}")
            );

            Target("docs",
                () => RunAsync("dotnet", "wyam docs -o ../artifacts/docs")
            );

            Target("default", DependsOn("test"));

            await RunTargetsAndExitAsync(args);
        }
    }
}
