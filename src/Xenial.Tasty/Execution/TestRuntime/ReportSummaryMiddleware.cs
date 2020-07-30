using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xenial.Delicious.Reporters;

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
                    async Task<IEnumerable<TestSummary>> Summaries()
                    {
                        var result = new List<TestSummary>();

                        foreach (var provider in context.Scope.SummaryProviders)
                        {
                            result.Add(await provider.Invoke());
                        }

                        return result;
                    }

                    await Task.WhenAll(context.Scope.SummaryReporters
                        .Select(async r =>
                        {
                            foreach (var summary in await Summaries())
                            {
                                await r.Invoke(summary);
                            }
                        }).ToArray());
                }
            });
    }
}
