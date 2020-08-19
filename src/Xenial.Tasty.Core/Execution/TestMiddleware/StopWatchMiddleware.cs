using System;
using System.Diagnostics;
using System.Linq;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class StopWatchMiddleware
    {
        public static TestExecutor UseTestStopwatch(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await next();
                }
                finally
                {
                    sw.Stop();

                    context.CurrentCase.Duration = sw.Elapsed;
                }
            });
    }
}
