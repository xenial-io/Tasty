using System;
using System.Diagnostics;

namespace Xenial.Delicious.Execution.TestGroupMiddleware
{
    public static class ReportTestGroupMiddleware
    {
        public static TestExecutor UseTestGroupReporters(this TestExecutor executor)
            => executor.UseGroup(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    //TODO: TestGroupReporters
                    //context.CurrentScope.Report(context.CurrentGroup);
                }
            });
    }
}
