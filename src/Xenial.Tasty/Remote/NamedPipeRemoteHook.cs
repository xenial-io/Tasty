using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
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
            var isInteractive = Environment.GetEnvironmentVariable("TASTY_INTERACTIVE");
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
            var remote = JsonRpc.Attach<TastyRemote>(remoteStream);

            scope.RegisterReporter(test => remote.Report(new SerializableTestCase
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
            }));

            return Task.FromResult(remote);
        }
    }

    public static class NamedPipeRemoteHook
    {
        public static Task<TransportStreamFactory?> CreateNamedPipeTransportStream(CancellationToken token = default)
        {
            var connectionType = Environment.GetEnvironmentVariable("TASTY_INTERACTIVE_CON_TYPE");
            if (connectionType != null && connectionType.ToLowerInvariant() == "NamedPipes".ToLowerInvariant())
            {
                var connectionId = Environment.GetEnvironmentVariable("TASTY_INTERACTIVE_CON_ID");
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
