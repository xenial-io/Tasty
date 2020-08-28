using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    internal class PluginLoader<TPluginAttribute, TPluginDelegate, TPluginArgument>
        where TPluginAttribute : Attribute, IPluginAttribute
        where TPluginDelegate : Delegate
    {
        internal virtual Task LoadPlugins(TPluginArgument pluginArgument)
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            var pluginAttributes = entryAssembly.GetCustomAttributes(typeof(TPluginAttribute), true).OfType<TPluginAttribute>();

            foreach (var pluginAttribute in pluginAttributes)
            {
#if DEBUG
                Console.WriteLine($"Loading Plugin: {pluginAttribute.PluginType} from {pluginAttribute.PluginType.Assembly.Location}");
#endif
                try
                {
                    var @delegate = (TPluginDelegate)Delegate.CreateDelegate(
                        typeof(TPluginDelegate),
                        null,
                        pluginAttribute.PluginType.GetMethod(pluginAttribute.PluginEntryPoint, BindingFlags.Static | BindingFlags.Public)
                    );
                    @delegate.DynamicInvoke(pluginArgument);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidPluginException(@$"Unable to load plugin {pluginAttribute.PluginType} from {pluginAttribute.PluginType.Assembly.Location}.
The plugin needs to be compatible with delegate {typeof(TastyPlugin).FullName}.
It must be a static extension method that accepts and returns a {typeof(TastyScope).FullName}",
                    pluginAttribute,
                    ex);
                }
#if DEBUG
                Console.WriteLine($"Loaded Plugin: {pluginAttribute.PluginType} from {pluginAttribute.PluginType.Assembly.Location}");
#endif
            }
#if DEBUG
            Console.WriteLine("Plugins loaded");
#endif
            return Task.CompletedTask;
        }
    }
}
