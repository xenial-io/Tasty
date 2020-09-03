
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Transports;

using static System.IO.Path;
using static Xenial.Commander;
using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static partial class TestExecutorTests
    {
        private static (
            string configuration,
            string targetFramework,
            string testDirectory,
            bool isWatchMode
        ) GetAssemblyAttributes()
        {
            var attributes = typeof(TestExecutorTests)
                    .Assembly
                    .GetCustomAttributes<AssemblyMetadataAttribute>();

            return (
                attributes.First(m => m.Key == "Configuration").Value!,
                attributes.First(m => m.Key == "TargetFramework").Value!,
                attributes.First(m => m.Key == "MSBuildThisFileDirectory").Value!,
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"))
            );
        }

        public static void IntegrationTests() => Describe(nameof(IntegrationTests), () =>
        {
            var (configuration, targetFramework, testDirectory, isWatchMode) =
                GetAssemblyAttributes();

            var testsDirectory = Combine(testDirectory, "integration");
            var integrationTests = Directory.EnumerateDirectories(testsDirectory);

            foreach (var integrationTest in integrationTests)
            {
                It($"should run {integrationTest} with {targetFramework}/{configuration}", () =>
                {
                    var workingDirectory = Combine(testsDirectory, integrationTest);

                    var connectionString = NamedPipesConnectionStringBuilder.CreateNewConnection();

                    return TasteProcess(
                        connectionString,
                        "dotnet",
                        $"run --no-build --no-restore --framework {targetFramework} -c {configuration}",
                        workingDirectory,
                        configureCommander: commander => commander.UseNamedPipesTransport()
                    );
                });
            }
        });
    }
}
