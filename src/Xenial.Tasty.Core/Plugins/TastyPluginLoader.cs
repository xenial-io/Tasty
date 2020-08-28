using System;
using System.Linq;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    internal class TastyPluginLoader : PluginLoader<TastyPluginAttribute, TastyPlugin, TastyScope>
    {
    }
}
