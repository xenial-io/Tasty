using System.Collections.Generic;
using System.IO;

using Nerdbank.Streams;

namespace Xenial.Delicious.Transports
{
    public static class InMemoryTransport
    {
        //TODO: find a way so safe dispose the streams or cleanup the dictionary on disposal
        private static readonly Dictionary<string, (Stream clientStream, Stream serverStream)> inMemoryStreams = new Dictionary<string, (Stream clientStream, Stream serverStream)>();

        public static (Stream clientStream, Stream serverStream) GetStream(string connectionId)
        {
            if (inMemoryStreams.TryGetValue(connectionId, out var streams))
            {
                return streams;
            }

            var newStreams = FullDuplexStream.CreatePair();
            inMemoryStreams[connectionId] = newStreams;
            return newStreams;
        }
    }
}
