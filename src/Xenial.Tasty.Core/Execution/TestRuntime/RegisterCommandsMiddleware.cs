using System.Linq;

using Xenial.Delicious.Protocols;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class RegisterCommandsMiddleware
    {
        public static TestExecutor UseRegisterCommands(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.Remote != null)
                    {
                        var commands = context.Scope.Commands.Values.Select(c => new SerializableTastyCommand
                        {
                            Name = c.Name,
                            Description = c.Description,
                            IsDefault = c.IsDefault
                        }).ToList();

                        await context.Remote.RegisterCommands(commands).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await next().ConfigureAwait(false);
                }
            });
    }
}
