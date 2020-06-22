namespace Xenial.Delicious.Execution.Middleware
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
                        ?? context.CurrentScope.RootAfterEachHooks
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