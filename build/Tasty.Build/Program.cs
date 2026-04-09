using System;
using System.Linq;
using System.Runtime.InteropServices;

using static SimpleExec.Command;
using static Bullseye.Targets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Tasty.Build
{
    internal static partial class Program
    {
        internal static async Task Main(string[] args)
        {
            static string logOptions(string target)
                => $"/maxcpucount /nologo /verbosity:minimal /bl:./artifacts/logs/tasty.{target}.binlog";

            const string Configuration = "Release";

            Func<string> properties = () => string.Join(" ", new Dictionary<string, string>
            {
                ["Configuration"] = Configuration,
            }.Select(p => $"/P:{p.Key}=\"{p.Value}\""));

            Target("ensure-tools", () => EnsureTools());

            Target("clean", dependsOn: ["ensure-tools"],
                () => RunAsync("dotnet", $"rimraf . -i **/bin/**/*.* -i **/obj/**/*.* -i artifacts/**/*.* -e node_modules/**/*.* -e build/**/*.* -q")
            );

            Target("lint", dependsOn: ["ensure-tools"],
                () => RunAsync("dotnet", $"format --exclude ext --check --verbosity diagnostic")
            );

            Target("format", dependsOn: ["ensure-tools"],
                () => RunAsync("dotnet", $"format --exclude ext")
            );

            Target("restore", dependsOn: ["lint"],
                () => RunAsync("dotnet", $"restore {logOptions("restore")}")
            );

            Target("build", dependsOn: ["restore"],
                () => RunAsync("dotnet", $"build --no-restore -c {Configuration} {logOptions("build")} {properties()}")
            );

            Target("test", dependsOn: ["build"], async () =>
            {
                var netcore = "net10.0";

                string[] tfms = RuntimeInformation
                            .IsOSPlatform(OSPlatform.Windows)
                            ? [netcore]
                            : [netcore];

                var tests = tfms
                    .Select(tfm => RunAsync("dotnet", $"run --project test/Xenial.Tasty.Tests/Xenial.Tasty.Tests.csproj --no-build --no-restore --framework {tfm} -c {Configuration} {properties()}"))
                    .ToArray();

                await Task.WhenAll(tests);
            });

            Target("lic", dependsOn: ["test"],
                async () =>
                {
                    var files = Directory.EnumerateFiles(@"src", "*.csproj", SearchOption.AllDirectories).Select(file => new
                    {
                        ProjectName = $"src/{Path.GetFileNameWithoutExtension(file)}/{Path.GetFileName(file)}",
                        ThirdPartyName = $"src/{Path.GetFileNameWithoutExtension(file)}/THIRD-PARTY-NOTICES.TXT"
                    });

                    var tasks = files.Select(proj => RunAsync("dotnet", $"thirdlicense --project {proj.ProjectName} --output {proj.ThirdPartyName}"));

                    await Task.WhenAll(tasks);
                }
            );

            Target("pack", dependsOn: ["lic"],
                () => RunAsync("dotnet", $"pack Xenial.Tasty.sln --no-restore --no-build -c {Configuration} {logOptions("pack.nuget")} {properties()}")
            );

            Target("docs",
                () => RunAsync("dotnet", "wyam docs -o ../artifacts/docs")
            );

            Target("docs.serve", dependsOn: ["ensure-tools"],
                () => RunAsync("dotnet", "wyam docs -o ../artifacts/docs -w -p")
            );

            Target("deploy.nuget", dependsOn: ["ensure-tools"], async () =>
            {
                var files = Directory.EnumerateFiles("artifacts/nuget", "*.nupkg");

                foreach (var file in files)
                {
                    await RunAsync("dotnet", $"nuget push {file} --skip-duplicate -s https://api.nuget.org/v3/index.json -k {Environment.GetEnvironmentVariable("NUGET_AUTH_TOKEN")}",
                        noEcho: true
                    );
                }
            });

            Target("release", async () =>
            {
                await Release();
            });

            Target("default", dependsOn: ["test"]);

            await RunTargetsAndExitAsync(args);
        }
    }
}
