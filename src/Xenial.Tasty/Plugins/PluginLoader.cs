using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    public class PluginLoader
    {
        public Task LoadPlugins(TastyScope scope)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var pluginAttributes = entryAssembly.CustomAttributes.OfType<TastyPluginAttribute>();

            foreach (var pluginAttribute in pluginAttributes)
            {
                Console.WriteLine(pluginAttribute.TastyPluginType);
            }

            Console.WriteLine("Plugins loaded");

            return Task.CompletedTask;
        }
    }
}
