
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
using static Shouldly.Should;
using static SimpleExec.Command;
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
                It($"should run {integrationTest} with {targetFramework}/{configuration}", async () =>
                {
                    var workingDirectory = Combine(testsDirectory, integrationTest);

                    using var commander = new TastyCommander()
                    {
                        LoadPlugins = false
                    }
                    .UseNamedPipesTransport();

                    var connectionString = NamedPipesConnectionStringBuilder.CreateNewConnection();

                    var remote = commander.ConnectToRemote(connectionString);

                    var remoteProcess = ReadAsync("dotnet",
                        $"run --no-build --no-restore --framework {targetFramework} -c {configuration}",
                        workingDirectory,
                        noEcho: true,
                        configureEnvironment: (env) =>
                        {
                            env.Add(EnvironmentVariables.InteractiveMode, "false");
                            env.Add(EnvironmentVariables.TastyConnectionString, connectionString.ToString());
                        });

                    await Task.WhenAll(remote, remoteProcess);
                });
            }
        });
    }
}
