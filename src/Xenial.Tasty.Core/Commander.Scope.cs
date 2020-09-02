using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Commanders
{
    public class TastyScopeCommander : TastyCommander
    {
        private readonly Lazy<TastyScope> scopeFactory;
        public Uri ConnectionString { get; }

        public TastyScopeCommander(Uri connectionString, Func<TastyScope> createScope)
            => (ConnectionString, scopeFactory) = (connectionString, new Lazy<TastyScope>(createScope));

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
            await TryLoadPlugins().ConfigureAwait(false);

            var remote = TryConnect(cancellationToken).ConfigureAwait(false);

            var tests = scopeFactory.Value.Run().ConfigureAwait(false);

            await remote;

            await foreach (var report in WaitForResults(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return report;
            }

            await tests;
        }
    }
}
