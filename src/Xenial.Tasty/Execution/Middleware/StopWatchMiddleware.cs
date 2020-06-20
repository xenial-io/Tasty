using System;
using System.Diagnostics;
using System.Linq;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Execution.Middleware
{
    public static class StopWatchMiddleware
    {
        public static TestExecutor UseStopwatch(this TestExecutor executor)
            => executor.Use(async (conext, next) =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await next();
                }
                finally
                {
                    sw.Stop();

                    conext.CurrentCase.Duration = sw.Elapsed;
                }
            });
    }
}