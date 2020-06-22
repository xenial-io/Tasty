
using System;
using System.Linq;
using System.Reflection;

using static Xenial.Tasty;
using static SimpleExec.Command;
using static Shouldly.Should;
using static System.IO.Path;
using SimpleExec;

namespace Xenial.Delicious.Tests
{
    public static partial class TestExecutorTests
    {
        public static void IntegrationTests()
        {
            Describe(nameof(IntegrationTests), () =>
            {
                var configuration = typeof(TestExecutorTests)
                    .Assembly
                    .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
                    .First(m => m.Key == "Configuration").Value;

                var targetFramework = typeof(TestExecutorTests)
                    .Assembly
                    .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
                    .First(m => m.Key == "TargetFramework").Value;

                var testDirectory = typeof(TestExecutorTests)
                  .Assembly
                  .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
                  .First(m => m.Key == "MSBuildThisFileDirectory").Value;

                var isWatchMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"));

                var integrationTests = new[]
                {
                    "Xenial.Tasty.ForcedTests"
                };

                foreach (var integrationTest in integrationTests)
                {
                    It($"should run {integrationTest} with {targetFramework}/{configuration}", () =>
                    {
                        var workingDirectory = Combine(testDirectory!, integrationTest);

                        var args = !isWatchMode
                            ? "--no-build --no-restore"
                            : string.Empty;

                        NotThrow(async () => await ReadAsync("dotnet", $"run {args} --framework {targetFramework} -c {configuration}", workingDirectory, noEcho: true));
                    });
                }
            });
        }
    }
}