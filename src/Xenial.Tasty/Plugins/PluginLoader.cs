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
                Console.WriteLine($"Loading Plugin: {pluginAttribute.TastyPluginType} from {pluginAttribute.TastyPluginType.Assembly.Location}");

                var @delegate = (Action<TastyScope>)Delegate.CreateDelegate(
                    typeof(Action<TastyScope>),
                    null,
                    pluginAttribute.TastyPluginType.GetMethod(pluginAttribute.TastyPluginEntryPoint, BindingFlags.Static | BindingFlags.Public)
                );
                @delegate.Invoke(scope);

                Console.WriteLine($"Loaded Plugin: {pluginAttribute.TastyPluginType} from {pluginAttribute.TastyPluginType.Assembly.Location}");
            }

            Console.WriteLine("Plugins loaded");

            return Task.CompletedTask;
        }
    }
}
