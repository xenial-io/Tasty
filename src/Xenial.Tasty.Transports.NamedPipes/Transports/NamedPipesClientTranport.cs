using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Remote;

namespace Xenial.Delicious.Transports
{
    public static class NamedPipesClientTranport
    {
        public static Task<TransportStreamFactory> CreateNamedPipeTransportStream(Uri connectionString, CancellationToken token = default)
        {
            TransportStreamFactory functor = () => CreateStream(connectionString, token);
            return Task.FromResult(functor);
        }

        private static async Task<Stream> CreateStream(Uri connectionString, CancellationToken token = default)
        {
            _ = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            var serverName = connectionString.Host;

            // TODO: write a connection string parser once we introduce the next transport
            var connectionId = connectionString.Segments[1];
            var stream = new NamedPipeClientStream(serverName, connectionId, PipeDirection.InOut, PipeOptions.Asynchronous);
            await stream.ConnectAsync(token).ConfigureAwait(false);
            return stream;
        }
    }
}
