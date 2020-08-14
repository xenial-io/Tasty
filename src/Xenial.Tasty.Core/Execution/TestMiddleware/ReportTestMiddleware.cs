using System;
using System.Linq;

namespace Xenial.Delicious.Execution.TestMiddleware
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
                    try
                    {
                        await context.CurrentScope.Report(context.CurrentCase);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Reporter failed: {ex}");
                    }
                }
            });
    }
}
