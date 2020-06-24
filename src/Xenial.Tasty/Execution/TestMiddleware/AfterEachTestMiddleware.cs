namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class AfterEachTestMiddleware
    {
        public static TestExecutor UseAfterEachTest(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    foreach (var hook in
                        context.CurrentCase.Group?.AfterEachHooks
                        ?? context.CurrentScope.AfterEachHooks
                        )
                    {
                        var hookResult = await hook.Executor.Invoke();
                        if (!hookResult)
                        {
                            break;
                        }
                    }
                }
            });
    }
}