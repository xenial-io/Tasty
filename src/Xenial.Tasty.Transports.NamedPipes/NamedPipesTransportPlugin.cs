using System;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Transports;

namespace Xenial.Delicious.Plugins
{
    public static class NamedPipesTransportPlugin
    {
        public static TastyScope UseNamedPipesTransport(this TastyScope scope)
               => (scope ?? throw new ArgumentNullException(nameof(scope)))
                .RegisterTransport(
                   Uri.UriSchemeNetPipe,
                   NamedPipesClientTranport.CreateNamedPipeTransportStream
                );

        public static TastyCommander UseNamedPipesTransport(this TastyCommander commander)
               => (commander ?? throw new ArgumentNullException(nameof(commander)))
                .RegisterTransport(
                   Uri.UriSchemeNetPipe,
                   NamedPipesServerTranport.CreateNamedPipeTransportStream
                );
    }
}
