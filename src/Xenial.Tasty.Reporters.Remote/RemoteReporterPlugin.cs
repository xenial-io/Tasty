using System;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    public static class RemoteReporterPlugin
    {
        public static TastyScope UseRemoteReporter(this TastyScope scope)
               => scope.RegisterRemoteReporter();
    }
}
