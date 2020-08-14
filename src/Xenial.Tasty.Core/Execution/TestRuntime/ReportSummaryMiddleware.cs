using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ReportSummaryMiddleware
    {
        public static TestExecutor UseSummaryReporters(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    var cases = context.Scope.Descendants().OfType<TestCase>().ToList();
                    await Task.WhenAll(context.Scope.SummaryReporters
                        .Select(async r =>
                        {
                            await r.Invoke(cases);
                        }).ToArray());
                }
            });
    }
}
