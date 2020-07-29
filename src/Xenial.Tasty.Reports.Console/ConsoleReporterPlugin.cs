using System;

using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    public static class ConsoleReporterPlugin
    {
        public static void UseConsoleReporter(this TastyScope scope)
        {
            scope.RegisterConsoleReporter();
        }
    }
}
