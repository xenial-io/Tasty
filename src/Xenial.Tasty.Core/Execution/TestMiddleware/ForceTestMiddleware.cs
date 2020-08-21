using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class ForceTestMiddleware
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This grabs into user code")]
        public static TestExecutor UseForcedTestExecutor(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    if (context.CurrentCase.IsForced != null)
                    {
                        var result = context.CurrentCase.IsForced();
                        if (result)
                        {
                            await next().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await next().ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    context.CurrentCase.Exception = exception;
                    context.CurrentCase.TestOutcome = TestOutcome.Failed;
                }
            });
    }
}
