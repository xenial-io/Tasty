using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class IgnoreTestMiddleware
    {
        public static TestExecutor UseIgnoreTestExecutor(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    if (context.CurrentCase.IsIgnored != null)
                    {
                        var result = context.CurrentCase.IsIgnored();
                        if (result.HasValue)
                        {
                            if (result.Value)
                            {
                                context.CurrentCase.TestOutcome = TestOutcome.Ignored;
                            }
                            else
                            {
                                await next();
                            }
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
