using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using StreamJsonRpc;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Remote
{
    public delegate Task<bool> IsInteractiveRun();

    public delegate Task<TransportStreamFactory?> TransportStreamFactoryFunctor(CancellationToken token = default);
    public delegate Task<Stream> TransportStreamFactory();

    public delegate Task<ITastyRemote> ConnectToRemote(TastyScope scope, Stream remoteStream);

    public static class TastyRemoteDefaults
    {
        public static Task<bool> IsInteractiveRun()
        {
            var isInteractive = Environment.GetEnvironmentVariable(EnvironmentVariables.InteractiveMode);
            if (!string.IsNullOrEmpty(isInteractive))
            {
                if (bool.TryParse(isInteractive, out var result))
                {
                    return Task.FromResult(result);
                }
            }
            return Task.FromResult(false);
        }

        public static Task<ITastyRemote> AttachToStream(TastyScope scope, Stream remoteStream)
        {
            _ = scope ?? throw new ArgumentNullException(nameof(scope));
            _ = remoteStream ?? throw new ArgumentNullException(nameof(remoteStream));

            var remote = JsonRpc.Attach<ITastyRemote>(remoteStream);

            Task TestReporter(TestCaseResult testCase)
               => remote.Report(testCase);

            Task SummaryReporter(IEnumerable<TestCaseResult> testCases)
                => remote.Report(testCases);

            scope.RegisterReporter(TestReporter);
            scope.RegisterReporter(SummaryReporter);

            return Task.FromResult(remote);
        }
    }

    public static class NamedPipeRemoteHook
    {
        public static Task<TransportStreamFactory?> CreateNamedPipeTransportStream(CancellationToken token = default)
        {
            var connectionType = Environment.GetEnvironmentVariable(EnvironmentVariables.InteractiveConnectionType);
            if (string.Equals(connectionType, "NamedPipes", StringComparison.InvariantCultureIgnoreCase))
            {
                var connectionId = Environment.GetEnvironmentVariable(EnvironmentVariables.InteractiveConnectionId);
                if (!string.IsNullOrEmpty(connectionId))
                {
                    TransportStreamFactory functor = () => CreateNamedPipeTransportStream(connectionId, token);
                    return Task.FromResult<TransportStreamFactory?>(functor);
                }
            }
            return Task.FromResult<TransportStreamFactory?>(null);
        }

        private static async Task<Stream> CreateNamedPipeTransportStream(string connectionId, CancellationToken token = default)
        {
            var stream = new NamedPipeClientStream(".", connectionId, PipeDirection.InOut, PipeOptions.Asynchronous);
            await stream.ConnectAsync(token).ConfigureAwait(false);
            return stream;
        }
    }
}
