using System;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Transports;

using static Xenial.Delicious.Transports.InMemoryConnectionStringBuilder;

namespace Xenial.Delicious.Plugins
{
    public static class InMemoryTransportPlugin
    {
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
