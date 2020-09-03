using System;

namespace Xenial.Delicious.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class CommanderPluginAttribute : Attribute, IPluginAttribute
    {
        public Type PluginType { get; }
        public string PluginEntryPoint { get; }
        public CommanderPluginAttribute(Type tastyPluginType, string tastyPluginEntryPoint)
            => (PluginType, PluginEntryPoint)
            = (
                tastyPluginType ?? throw new ArgumentNullException(nameof(tastyPluginType)),
                tastyPluginEntryPoint ?? throw new ArgumentNullException(tastyPluginEntryPoint)
            );
    }
}
