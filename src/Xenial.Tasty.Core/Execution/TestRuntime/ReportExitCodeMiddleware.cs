using System.Linq;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ReportExitCodeMiddleware
    {
        public static TestExecutor UseExitCodeReporter(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next().ConfigureAwait(false);
                }
                finally
                {
                    var cases = context.Scope.Descendants().OfType<TestCase>().ToList();
                    var failedCase = cases
                        .FirstOrDefault(m => m.TestOutcome == TestOutcome.Failed);

                    context.ExitCode = failedCase != null
                        ? 1
                        : 0;
                }
            });
    }
}
