using System;
using System.Linq;

using Xenial.Delicious.Commands;

using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class SelectCommandMiddleware
    {
        public static TestExecutor UseSelectCommand(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.IsInteractive && context.Remote != null)
                    {
                        var command = await Promise<TastyCommand?>((resolve, reject) =>
                        {
                            void ExecuteCommand(object _, Protocols.ExecuteCommandEventArgs e)
                            {
                                var cmd = context.Scope.Commands.Values.FirstOrDefault(p => p.Name == e.CommandName);
                                context.Remote.ExecuteCommand -= ExecuteCommand;
                                resolve(cmd);
                            }
                            context.Remote.ExecuteCommand -= ExecuteCommand;
                            context.Remote.ExecuteCommand += ExecuteCommand;
                            context.Remote.CancellationRequested += (s, _) => reject();
                        }).ConfigureAwait(false);

                        context.CurrentCommand = command;
                    }
                    else
                    {
                        var defaultCommand = context.Scope.Commands.Values.FirstOrDefault(p => p.IsDefault);

                        context.CurrentCommand = defaultCommand;
                    }
                }
                finally
                {
                    await next().ConfigureAwait(false);
                }
            });
    }
}
