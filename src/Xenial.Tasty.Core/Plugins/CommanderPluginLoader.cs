using System;
using System.Linq;

using Xenial.Delicious.Commanders;

namespace Xenial.Delicious.Plugins
{
    internal class CommanderPluginLoader : PluginLoader<CommanderPluginAttribute, CommanderPlugin, TastyCommander>
    {
    }
}
