using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Reporters
{
    public static class RemoteReporter
    {
        public static TastyScope RegisterRemoteReporter(this TastyScope scope)
         => (scope ?? throw new ArgumentNullException(nameof(scope)))
                .RegisterReporter(Report)
                .RegisterReporter(ReportSummary);

        public static TastyScope Register()
            => Tasty.TastyDefaultScope.RegisterRemoteReporter();

        private static Task ReportSummary(IEnumerable<TestCaseResult> tests)
            => Task.CompletedTask;

        public static Task Report(TestCaseResult test)
            => Task.CompletedTask;
    }
}
