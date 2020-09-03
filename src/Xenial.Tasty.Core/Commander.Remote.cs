using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;

namespace Xenial.Delicious.Commanders
{
    public class TastyRemoteCommander : TastyCommander
    {
        public Uri ConnectionString { get; }

        private readonly Func<CancellationToken, Task<int>> remoteTaskFactory;

        public TastyRemoteCommander(Uri connectionString, Func<CancellationToken, Task<int>> taskFactory)
            => (ConnectionString, remoteTaskFactory) = (connectionString, taskFactory);

        public async Task TryConnect(CancellationToken cancellationToken = default)
        {
            await TryLoadPlugins().ConfigureAwait(false);

            if (!IsConnected)
            {
                await ConnectToRemote(ConnectionString, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async IAsyncEnumerable<TestCaseResult> Run([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tests = await ConnectAsync(cancellationToken).ConfigureAwait(false);

            await foreach (var report in WaitForResults(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return report;
            }

            //TODO: handle exit code?
            var _ = await tests.ConfigureAwait(false);
        }

        public virtual async Task<Task<int>> ConnectAsync(CancellationToken cancellationToken = default)
        {
            await TryLoadPlugins().ConfigureAwait(false);

            var remote = TryConnect(cancellationToken).ConfigureAwait(false);

            var tests = remoteTaskFactory(cancellationToken);

            await remote;

            return tests;
        }
    }
}
