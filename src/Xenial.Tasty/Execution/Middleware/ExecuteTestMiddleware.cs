using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.Middleware
{
    public static class ExecuteTestMiddleware
    {
        public static TestExecutor UseTestExecutor(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    var result = await context.CurrentCase.Executor.Invoke();
                    context.CurrentCase.TestOutcome = result ? TestOutcome.Success : TestOutcome.Failed;
                }
                catch (Exception exception)
                {
                    context.CurrentCase.Exception = exception;
                    context.CurrentCase.TestOutcome = TestOutcome.Failed;
                }
                await next();
            });
    }
}
