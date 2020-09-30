using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Reporters
{
    public static class RemoteReporter
    {
        public static TastyScope RegisterRemoteReporter(this TastyScope scope)
        {
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            var hook = scope.ConnectToRemoteRunHook;

            scope.ConnectToRemoteRunHook = async (scope, remoteStream) =>
            {
                var remote = await hook(scope, remoteStream).ConfigureAwait(false);

                Task TestReporter(TestCaseResult testCase)
                    => remote.Report(testCase);

                Task SummaryReporter(IEnumerable<TestCaseResult> testCases)
                    => remote.Report(testCases);

                scope.RegisterReporter(TestReporter);
                scope.RegisterReporter(SummaryReporter);

                return remote;
            };

            return scope;
        }
    }
}
