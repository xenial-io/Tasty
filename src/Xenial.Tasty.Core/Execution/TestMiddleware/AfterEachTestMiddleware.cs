namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class AfterEachTestMiddleware
    {
        public static TestExecutor UseAfterEachTest(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    await next().ConfigureAwait(false);
                }
                finally
                {
                    var hooks = context.CurrentCase.Group?.AfterEachHooks
                        ?? context.CurrentScope.AfterEachHooks;

                    foreach (var hook in hooks)
                    {
                        var hookResult = await hook.Executor.Invoke().ConfigureAwait(false);
                        if (!hookResult)
                        {
                            break;
                        }
                    }
                }
            });
    }
}
