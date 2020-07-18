using System;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Commands
{
    internal class TastyCommand
    {
        internal TastyCommand(string name, Func<RuntimeContext, Task> command, string? description, bool isDefault = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Description = description;
            IsDefault = isDefault;
        }

        internal string Name { get; }
        internal Func<RuntimeContext, Task> Command { get; }
        internal string? Description { get; }
        internal bool IsDefault { get; internal set; } = false;
    }
}
