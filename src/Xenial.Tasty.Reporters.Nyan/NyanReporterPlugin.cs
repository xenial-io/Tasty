using System;

using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    public static class NyanReporterPlugin
    {
        public static TastyScope UseNyanReporter(this TastyScope scope)
            => scope.RegisterNyanReporter();
    }
}
