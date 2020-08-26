using System;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Transports;

namespace Xenial.Delicious.Plugins
{
    public static class RemoteReporterPlugin
    {
        public static TastyScope UseNamedPipesTransport(this TastyScope scope)
               => (scope ?? throw new ArgumentNullException(nameof(scope)))
                .RegisterTransport(NamedPipesTranport.CreateNamedPipeTransportStream);
    }
}
