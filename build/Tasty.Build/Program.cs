using System;

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
            //XenialVersionInfo version = null;

            Target("version", () =>
            {
                throw new Exception(Environment.GetEnvironmentVariable("OS"));
                //version = new XenialVersionInfo();
                //version.PrintVersion();
            });

            static string logOptions(string target)
                => $"/maxcpucount /nologo /verbosity:minimal /bl:./artifacts/logs/tasty.{target}.binlog";

            Target("restore", DependsOn("version"),
                () => RunAsync("dotnet", $"restore {logOptions("restore")}")
            );

            Target("build", DependsOn("restore"), 
                () => RunAsync("dotnet", $"build --no-restore {logOptions("build")}")
            );

            Target("test", DependsOn("build"), () =>
            {

            });

            Target("pack", DependsOn("test"), 
                () => RunAsync("dotnet", $"pack src/Xenial.Tasty/Xenial.Tasty.csproj --no-restore --no-build {logOptions("pack")}")
            );

            Target("default", DependsOn("test"));

            await RunTargetsAndExitAsync(args);
        }
    }
}
