using System;
using System.Linq;

namespace Xenial.Delicious.Execution.Middleware
{
    public static class ReportTestMiddleware
    {
        public static TestExecutor UseTestReporters(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                await context.CurrentScope.Report(context.CurrentCase);
                await next();
            });
    }
}
