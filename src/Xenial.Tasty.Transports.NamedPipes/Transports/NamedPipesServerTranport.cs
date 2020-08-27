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
        //public static Task<TransportStreamFactory?> CreateNamedPipeTransportStream(CancellationToken token = default)
        //{
        //    //var connectionType = Environment.GetEnvironmentVariable(EnvironmentVariables.InteractiveConnectionType);
        //    //if (string.Equals(connectionType, "NamedPipes", StringComparison.InvariantCultureIgnoreCase))
        //    //{
        //    //    var connectionId = Environment.GetEnvironmentVariable(EnvironmentVariables.InteractiveConnectionId);
        //    //    if (!string.IsNullOrEmpty(connectionId))
        //    //    {
        //    //        TransportStreamFactory functor = () => CreateNamedPipeTransportStream(connectionId, token);
        //    //        return Task.FromResult<TransportStreamFactory?>(functor);
        //    //    }
        //    //}
        //    return Task.FromResult<TransportStreamFactory?>(null);
        //}

        //private static async Task<Stream> CreateNamedPipeTransportStream(string connectionId, CancellationToken token = default)
        //{
        //    var stream = new NamedPipeServerStream(
        //          connectionId,
        //          PipeDirection.InOut,
        //          NamedPipeServerStream.MaxAllowedServerInstances,
        //          PipeTransmissionMode.Byte,
        //          PipeOptions.Asynchronous
        //      );

        //    await stream.WaitForConnectionAsync(token).ConfigureAwait(false);

        //    return stream;
        //}
    }
}
