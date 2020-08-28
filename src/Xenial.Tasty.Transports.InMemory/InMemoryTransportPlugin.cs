using System;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Transports;

namespace Xenial.Delicious.Plugins
{
    public static class InMemoryTransportPlugin
    {
        public static string Scheme => "inmem";

        public static TastyScope UseInMemoryTransport(this TastyScope scope)
               => (scope ?? throw new ArgumentNullException(nameof(scope)))
                .RegisterTransport(
                   Scheme,
                   InMemoryClientTranport.CreateInMemoryTransportStream
                );

        public static TastyCommander UseInMemoryTransport(this TastyCommander commander)
               => (commander ?? throw new ArgumentNullException(nameof(commander)))
                .RegisterTransport(
                   Scheme,
                   InMemoryServerTranport.CreateInMemoryTransportStream
                );
    }
}
