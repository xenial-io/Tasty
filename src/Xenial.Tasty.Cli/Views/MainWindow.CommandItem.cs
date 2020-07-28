using System;

using Xenial.Delicious.Protocols;

namespace Xenial.Delicious.Cli.Views
{
    internal class CommandItem
    {
        internal SerializableTastyCommand Command { get; }
        internal CommandItem(SerializableTastyCommand command)
            => Command = command;

        public override string ToString()
            => $"{(Command.IsDefault ? "* " : string.Empty)}{Command.Name}{(string.IsNullOrEmpty(Command.Description) ? string.Empty : ($" - {Command.Description}"))}";
    }
}
