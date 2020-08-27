using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using StreamJsonRpc;

using Xenial.Delicious.Protocols;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Remote
{
    public delegate Task<bool> IsInteractiveRun();
    public delegate Task<Uri?> ParseConnectionString();

    public delegate Task<TransportStreamFactory> TransportStreamFactoryFunctor(Uri connectionString, CancellationToken token = default);

    public delegate Task<Stream> TransportStreamFactory();

    public delegate Task<ITastyRemote> ConnectToRemote(TastyScope scope, Stream remoteStream);

    public static class TastyRemoteDefaults
    {
        public static Task<bool> IsInteractiveRun()
        {
            var isInteractive = Environment.GetEnvironmentVariable(EnvironmentVariables.InteractiveMode);
            if (bool.TryParse(isInteractive, out var result))
            {
                return Task.FromResult(result);
            }
            return Task.FromResult(false);
        }

        public static Task<Uri?> ParseConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.TastyConnectionString);
            if (Uri.IsWellFormedUriString(connectionString, UriKind.Absolute))
            {
                return Task.FromResult<Uri?>(new Uri(connectionString));
            }
            return Task.FromResult<Uri?>(null);
        }

        public static Task<ITastyRemote> AttachToStream(TastyScope scope, Stream remoteStream)
        {
            _ = scope ?? throw new ArgumentNullException(nameof(scope));
            _ = remoteStream ?? throw new ArgumentNullException(nameof(remoteStream));

            var remote = JsonRpc.Attach<ITastyRemote>(remoteStream);

            return Task.FromResult(remote);
        }
    }
}
