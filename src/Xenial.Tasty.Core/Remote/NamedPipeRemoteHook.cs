using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using StreamJsonRpc;

using Xenial.Delicious.Protocols;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Remote
{
    public delegate Task<bool> IsInteractiveRun();

    public delegate Task<TransportStreamFactory?> TransportStreamFactoryFunctor(CancellationToken token = default);
    public delegate Task<Stream> TransportStreamFactory();

    public delegate Task<TastyRemote> ConnectToRemote(TastyScope scope, Stream remoteStream);

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

        public static Task<TastyRemote> AttachToStream(TastyScope scope, Stream remoteStream)
        {
            static SerializableTestCase MapToSerializableTestCase(Metadata.TestCase test)
            {
                return new SerializableTestCase
                {
                    FullName = test.FullName,
                    Name = test.Name,
                    AdditionalMessage = test.AdditionalMessage,
                    Duration = test.Duration,
                    Exception = test.Exception,
                    IgnoredReason = test.IgnoredReason,
                    IsForced = test.IsForced?.Invoke(),
                    IsIgnored = test.IsIgnored?.Invoke(),
                    TestOutcome = test.TestOutcome
                };
            }

            var remote = JsonRpc.Attach<TastyRemote>(remoteStream);

            scope.RegisterReporter(test => remote.Report(MapToSerializableTestCase(test)));

            Task SummaryReporter(IEnumerable<Metadata.TestCase> @cases)
                => remote.Report(@cases.Select(test => MapToSerializableTestCase(test)));

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

        static async Task<Stream> CreateNamedPipeTransportStream(string connectionId, CancellationToken token = default)
        {
            var stream = new NamedPipeClientStream(".", connectionId, PipeDirection.InOut, PipeOptions.Asynchronous);
            await stream.ConnectAsync(token);
            return stream;
        }
    }
}
