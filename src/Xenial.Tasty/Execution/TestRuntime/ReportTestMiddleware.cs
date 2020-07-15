using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ReportSummaryMiddleware
    {
        public static TestExecutor UseSummaryReporters(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    var cases = context.Scope.Descendants().OfType<TestCase>().ToList();
                    await Task.WhenAll(context.Scope.SummaryReporters
                        .Select(async r =>
                        {
                            await r.Invoke(cases);
                        }).ToArray());
                }
            });
    }

    public static class ReportExitCodeMiddleware
    {
        public static TestExecutor UseExitCodeReporter(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    var cases = context.Scope.Descendants().OfType<TestCase>().ToList();
                    var failedCase = cases
                        .FirstOrDefault(m => m.TestOutcome == TestOutcome.Failed);

                    context.ExitCode = failedCase != null
                        ? 1
                        : 0;
                }
            });
    }

    public static class DetectInteractiveRunMiddleware
    {
        public static TestExecutor UseInteractiveRunDetection(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    context.IsInteractive = await context.Scope.IsInteractiveRunHook();
                }
                finally
                {
                    await next();
                }
            });
    }

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

    public static class FinishPipelineMiddleware
    {
        public static TestExecutor UseFinishPipeline(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    if (!context.IsInteractive)
                    {
                        context.IsFinished = true;
                    }
                }
            });
    }

    public static class RemoteStreamDisposalMiddleware
    {
        public static TestExecutor UseRemoteDisposal(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    if (!context.IsInteractive)
                    {
                        context.RemoteStream?.Dispose();
                        context.Remote?.Dispose();
                    }
                }
            });
    }

    public static class ClearConsoleMiddleware
    {
        public static TestExecutor UseClearConsole(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.Scope.ClearBeforeRun)
                    {
                        try
                        {
                            Console.Clear();
                        }
                        catch (IOException) { /* Handle is invalid */}
                    }
                }
                finally
                {
                    await next();
                }
            });
    }

    public static class ResetConsoleColorMiddleware
    {
        public static TestExecutor UseResetConsoleColor(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    Console.ResetColor();
                }
            });
    }

    public static class RunCommandMiddleware
    {
        public static TestExecutor UseRunCommands(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.IsInteractive)
                    {

                    }
                    else
                    {
                        var defaultCommand = context.Scope.Commands.Values.FirstOrDefault(p => p.IsDefault);
                        if (defaultCommand != null)
                        {
                            await defaultCommand.Command(context);
                        }
                    }
                }
                finally
                {
                    await next();
                }
            });

    }

    public static class ClearRemoteConsoleMiddleware
    {
        public static TestExecutor UseRemoteClearConsole(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.Scope.ClearBeforeRun && context.Remote != null)
                    {
                        await context.Remote.ClearConsole();
                    }
                }
                finally
                {
                    await next();
                }
            });

    }
}
