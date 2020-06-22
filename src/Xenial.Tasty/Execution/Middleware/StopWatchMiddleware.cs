using System;
using System.Diagnostics;
using System.Linq;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Execution.Middleware
{
    public static class StopWatchMiddleware
    {
        public static TestExecutor UseStopwatch(this TestExecutor executor)
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