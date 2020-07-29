using System;
using System.Collections.Generic;
using System.Text;

namespace Xenial.Delicious.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class TastyPluginAttribute : Attribute
    {
        public string TastyPluginAssembly { get; }
        public string TastyPluginType { get; }
        public TastyPluginAttribute(string tastyPluginAssembly, string tastyPluginType) 
            => (TastyPluginAssembly, TastyPluginType) = (tastyPluginAssembly, tastyPluginType);
    }
}
