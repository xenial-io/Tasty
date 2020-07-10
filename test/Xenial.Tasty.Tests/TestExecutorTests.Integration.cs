
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleExec;
using static System.IO.Path;
using static Shouldly.Should;
using static SimpleExec.Command;
using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static partial class TestExecutorTests
    {
        static (
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

                    //We don't mind restore, cause adding dependencies 
                    //in watch mode isn't happening that often and
                    //cuts test time almost in half
                    var args = !isWatchMode
                        ? "--no-build --no-restore"
                        : "--no-restore";

                    NotThrow(async () => await ReadAsync("dotnet",
                        $"run {args} --framework {targetFramework} -c {configuration}",
                        workingDirectory,
                        noEcho: true,
                        configureEnvironment: (env) =>
                        {
                            env.Add("TASTY_INTERACTIVE", "false");
                        })
                    );
                });
            }
        });
    }
}