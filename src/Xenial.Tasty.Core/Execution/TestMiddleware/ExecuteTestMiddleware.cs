using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class ExecuteTestMiddleware
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This grabs into user code")]
        public static TestExecutor UseTestExecutor(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    var result = await context.CurrentCase.Executor.Invoke().ConfigureAwait(false);
                    context.CurrentCase.TestOutcome = result
                        ? TestOutcome.Success
                        : TestOutcome.Failed;
                }
                catch (Exception exception)
                {
                    context.CurrentCase.Exception = exception;
                    context.CurrentCase.TestOutcome = TestOutcome.Failed;
                }
                await next().ConfigureAwait(false); ;
            });
    }
}
