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

            var pluginAttributes = entryAssembly.GetCustomAttributes(typeof(TastyPluginAttribute), true).OfType<TastyPluginAttribute>();

            foreach (var pluginAttribute in pluginAttributes)
            {
#if DEBUG
                Console.WriteLine($"Loading Plugin: {pluginAttribute.TastyPluginType} from {pluginAttribute.TastyPluginType.Assembly.Location}");
#endif
                try
                {
                    var @delegate = (TastyPlugin)Delegate.CreateDelegate(
                        typeof(TastyPlugin),
                        null,
                        pluginAttribute.TastyPluginType.GetMethod(pluginAttribute.TastyPluginEntryPoint, BindingFlags.Static | BindingFlags.Public)
                    );

                    @delegate.Invoke(scope);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidPluginException(@$"Unable to load plugin {pluginAttribute.TastyPluginType} from {pluginAttribute.TastyPluginType.Assembly.Location}.
The plugin needs to be compatible with delegate {nameof(TastyPlugin)}.
It must be a static extension method that accepts a {nameof(TastyScope)} and must return a {nameof(TastyScope)}",
                    pluginAttribute,
                    ex);
                }
#if DEBUG
                Console.WriteLine($"Loaded Plugin: {pluginAttribute.TastyPluginType} from {pluginAttribute.TastyPluginType.Assembly.Location}");
#endif         
            }
#if DEBUG
            Console.WriteLine("Plugins loaded");
#endif
            return Task.CompletedTask;
        }
    }
}
