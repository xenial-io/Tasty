using System;
using System.Collections.Generic;
using System.Text;

namespace Xenial.Delicious.Plugins
{
    public delegate TastyScope TastyPlugin(TastyScope scope);

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class TastyPluginAttribute : Attribute
    {
        public Type TastyPluginType { get; }
        public string TastyPluginEntryPoint { get; }
        public TastyPluginAttribute(Type tastyPluginType, string tastyPluginEntryPoint)
            => (TastyPluginType, TastyPluginEntryPoint) = (tastyPluginType, tastyPluginEntryPoint);
    }
}
