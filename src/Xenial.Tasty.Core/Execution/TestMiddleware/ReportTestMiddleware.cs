using System;
using System.Linq;

using Xenial.Delicious.Reporters;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class ReportTestMiddleware
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This grabs into user code")]
        public static TestExecutor UseTestReporters(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    await next().ConfigureAwait(false);
                }
                finally
                {
                    try
                    {
                        await context.CurrentScope.Report(context.CurrentCase.ToResult()).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Reporter failed: {ex}");
                    }
                }
            });
    }
}
