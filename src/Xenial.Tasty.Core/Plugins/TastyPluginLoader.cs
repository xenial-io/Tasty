using System;
using System.Linq;
using System.Reflection;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    internal class TastyPluginLoader : PluginLoader<TastyPluginAttribute, TastyPlugin, TastyScope>
    {
        public TastyPluginLoader(Assembly? entryAssembly = null) : base(entryAssembly) { }
    }
}
