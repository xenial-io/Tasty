using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Commands
{
    public class TastyCommand
    {
        public TastyCommand(string name, Func<TastyScope, Task> command, string? description, bool isDefault = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Description = description;
            IsDefault = isDefault;
        }

        public string Name { get; }
        public Func<TastyScope, Task> Command { get; }
        public string? Description { get; }
        public bool IsDefault { get; } = false;
    }
}
