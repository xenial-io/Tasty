using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Protocols;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class RunCommandMiddleware
    {
        public static TestExecutor UseRunCommands(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.IsInteractive && context.Remote != null)
                    {
                        var tcs = new TaskCompletionSource<bool>();

                        async void ExecuteCommand(object _, ExecuteCommandEventArgs e)
                        {
                            var command = context.Scope.Commands.Values.FirstOrDefault(p => p.Name == e.CommandName);
                            if (command != null)
                            {
                                try
                                {
                                    await command.Command(context);
                                    tcs.SetResult(true);
                                }
                                catch (Exception ex)
                                {
                                    tcs.SetException(ex);
                                }
                            }
                        }

                        void CancellationRequested(object _, EventArgs __)
                        {
                            tcs.SetResult(false);
                        }

                        void Exit(object _, EventArgs __)
                        {
                            tcs.SetCanceled();
                        }

                        context.Remote.ExecuteCommand -= ExecuteCommand;
                        context.Remote.ExecuteCommand += ExecuteCommand;
                        context.Remote.CancellationRequested -= CancellationRequested;
                        context.Remote.CancellationRequested += CancellationRequested;
                        context.Remote.Exit -= Exit;
                        context.Remote.Exit += Exit;

                        await tcs.Task;
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
}
