namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ParseConnectionStringMiddleware
    {
        public static TestExecutor UseParseConnectionString(this TestExecutor executor)
          => executor.UseRuntime(async (context, next) =>
          {
              try
              {
                  var connectionString = await context.Scope.ParseConnectionString().ConfigureAwait(false);
                  context.RemoteConnectionString = connectionString;
              }
              finally
              {
                  await next().ConfigureAwait(false);
              }
          });
    }
}
