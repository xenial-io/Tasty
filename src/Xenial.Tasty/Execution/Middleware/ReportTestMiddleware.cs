using System;
using System.Linq;

namespace Xenial.Delicious.Execution.Middleware
{
    public static class ReportTestMiddleware
    {
        public static TestExecutor UseTestReporters(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    await context.CurrentScope.Report(context.CurrentCase);
                }
            });
    }
}
