using System;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Commands
{
    public class TastyCommand
    {
        public TastyCommand(string name, Func<RuntimeContext, Task> command, string? description, bool isDefault = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Description = description;
            IsDefault = isDefault;
        }

        public string Name { get; }
        public Func<RuntimeContext, Task> Command { get; }
        public string? Description { get; }
        public bool IsDefault { get; internal set; } = false;
    }
}
