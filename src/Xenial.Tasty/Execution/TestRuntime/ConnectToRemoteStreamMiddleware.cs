using System.Diagnostics;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ConnectToRemoteStreamMiddleware
    {
        public static TestExecutor UseRemote(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.IsInteractive)
                    {
                        foreach (var remoteStreamFactoryFunctor in context.Scope.TransportStreamFactories)
                        {
                            var remoteStreamFactory = await remoteStreamFactoryFunctor();
                            if (remoteStreamFactory != null)
                            {
                                context.RemoteStream = await remoteStreamFactory.Invoke();
                                context.Remote = await context.Scope.ConnectToRemoteRunHook(context.Scope, context.RemoteStream);

                                break;
                            }
                        }
                    }
                }
                finally
                {
                    await next();
                }
            });
    }
}
