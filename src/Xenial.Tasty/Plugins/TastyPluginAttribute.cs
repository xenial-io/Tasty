using System;

namespace Xenial.Delicious.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class TastyPluginAttribute : Attribute
    {
        public Type TastyPluginType { get; }
        public string TastyPluginEntryPoint { get; }
        public TastyPluginAttribute(Type tastyPluginType, string tastyPluginEntryPoint)
            => (TastyPluginType, TastyPluginEntryPoint)
            = (
                tastyPluginType ?? throw new ArgumentNullException(nameof(tastyPluginType)),
                tastyPluginEntryPoint ?? throw new ArgumentNullException(tastyPluginEntryPoint)
            );
    }
}
