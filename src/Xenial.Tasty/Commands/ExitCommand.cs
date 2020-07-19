using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Commands
{
    public static class ExitCommand
    {
        public static (string name, Func<RuntimeContext, Task> command, string? description, bool? isDefault) Register()
            => ("x", Execute, "Exit interactive run", false);

        static Task Execute(RuntimeContext context)
        {
            context.EndPipeLine = true;
            return Task.CompletedTask;
        }
    }
}
