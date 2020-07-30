using System;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    public static class NyanReporterPlugin
    {
        public static TastyScope UseNyanReporter(this TastyScope scope)
            => scope.UseNyanReporter();
    }
}
