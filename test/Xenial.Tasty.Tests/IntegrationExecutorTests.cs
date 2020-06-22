
using System;
using System.Linq;
using System.Reflection;

using static Xenial.Tasty;
using static SimpleExec.Command;
using static Shouldly.Should;
using static System.IO.Path;

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

                It($"Should run ForcedTests in {configuration}/{targetFramework}", () =>
                {
                    var workingDirectory = Combine(testDirectory!, "Xenial.Tasty.ForcedTests");

                    NotThrow(async () => await ReadAsync("dotnet", $"run --no-build --no-restore --framework {targetFramework} -c {configuration}", workingDirectory, noEcho: true));
                });
            });
        }
    }
}