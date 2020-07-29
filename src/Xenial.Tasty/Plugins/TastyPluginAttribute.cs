using System;
using System.Collections.Generic;
using System.Text;

namespace Xenial.Delicious.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class TastyPluginAttribute : Attribute
    {
        public Type TastyPluginType { get; }
        public TastyPluginAttribute(Type tastyPluginType)
            => TastyPluginType = tastyPluginType;
    }
}
