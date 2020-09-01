using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FakeItEasy;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Transports;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static class TastyCommanderTests
    {
        public static void Commander() => Describe(nameof(TastyCommander), () =>
        {
            (Uri connectionString, TastyScope scope, TastyCommander commander, AsyncTestReporter reporter) CreateRemote()
            {
                var connectionString = InMemoryConnectionStringBuilder.CreateNewConnection();

                var scope = new TastyScope()
                {
                    LoadPlugins = false,
                    ClearBeforeRun = false,
                    //This would block the interactive run
                    IsInteractiveRunHook = () => Task.FromResult(false)
                }
                .UseInMemoryTransport()
                .UseRemoteReporter();

                scope.It("A test case", () => true);
                scope.ParseConnectionString = () => TastyRemoteDefaults.ParseConnectionString(connectionString.ToString());

                var commander = new TastyCommander()
                {
                    LoadPlugins = false
                }
                .UseInMemoryTransport();

                var reporter = A.Fake<AsyncTestReporter>();
                commander.RegisterReporter(reporter);

                return (connectionString, scope, commander, reporter);
            }

            It("Remote scope will be executed", async () =>
            {
                var (connectionString, scope, commander, reporter) = CreateRemote();
                var remote = commander.ConnectToRemote(connectionString);

                var runner = scope.Run();

                await Task.WhenAll(remote, runner);

                A.CallTo(reporter).MustHaveHappened();
            });

            It("Remote scope succeeds with non zero exit code", async () =>
                {
                    var (connectionString, scope, commander, _) = CreateRemote();
                    var remote = commander.ConnectToRemote(connectionString);

                    var runner = scope.Run();

                    await Task.WhenAll(remote, runner);

                    var exitCode = await runner;

                    return (exitCode == 0, "Remote scope should return non zero exit code");
                });

            async IAsyncEnumerable<TestCaseResult> RemoteReportsTestCase()
            {
                var (connectionString, scope, commander, reporter) = CreateRemote();
                var remote = commander.ConnectToRemote(connectionString);

                var tests = scope.Run();

                await foreach (var report in commander.WaitForResults())
                {
                    yield return report;
                }

                await tests;
            }

            It("Remote reports test case", () => RemoteReportsTestCase());
        });
    }
}
