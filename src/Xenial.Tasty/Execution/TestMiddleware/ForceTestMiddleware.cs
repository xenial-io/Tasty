using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class ForceTestMiddleware
    {
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
                            await next();
                        }
                    }
                    else
                    {
                        await next();
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
