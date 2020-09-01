using System;
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
                    .UseInMemoryTransport()
                    .UseRemoteReporter();

                scope.IsInteractiveRunHook = () => Task.FromResult(false);

                scope.It("A test case", () => true);
                scope.ParseConnectionString = () => TastyRemoteDefaults.ParseConnectionString(connectionString.ToString());

                var commander = new TastyCommander()
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
        });
    }
}
