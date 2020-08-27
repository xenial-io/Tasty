using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Remote;

namespace Xenial.Delicious.Transports
{
    public static class NamedPipesServerTranport
    {
        public static Task<TransportStreamFactory> CreateNamedPipeTransportStream(Uri connectionString, CancellationToken token = default)
        {
            _ = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            var connectionId = connectionString.Segments[1];
            TransportStreamFactory functor = () => CreateNamedPipeTransportStream(connectionId, token);
            return Task.FromResult(functor);
        }

        private static async Task<Stream> CreateNamedPipeTransportStream(string connectionId, CancellationToken token = default)
        {
            var stream = new NamedPipeServerStream(
                  connectionId,
                  PipeDirection.InOut,
                  NamedPipeServerStream.MaxAllowedServerInstances,
                  PipeTransmissionMode.Byte,
                  PipeOptions.Asynchronous
              );

            await stream.WaitForConnectionAsync(token).ConfigureAwait(false);

            return stream;
        }
    }
}
