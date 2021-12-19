using System;
using System.Collections.Generic;
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
        private readonly Assembly? entryAssembly;

        public PluginLoader(Assembly? entryAssembly = null)
        {
            this.entryAssembly = entryAssembly;
            FindAttributes = FindAssemblyAttributes;
        }

        internal Func<IEnumerable<TPluginAttribute>> FindAttributes { get; set; }
        private IEnumerable<TPluginAttribute> FindAssemblyAttributes()
        {
            var entryAssembly = (this.entryAssembly ?? Assembly.GetEntryAssembly());

            _ = entryAssembly ?? throw new ArgumentNullException(nameof(entryAssembly));

            var pluginAttributes = entryAssembly.GetCustomAttributes(typeof(TPluginAttribute), true).OfType<TPluginAttribute>();

            return pluginAttributes;
        }

        internal virtual Task LoadPlugins(TPluginArgument pluginArgument)
        {
            InvalidPluginException CreatePluginException(TPluginAttribute pluginAttribute, Exception? ex = null)
                => new InvalidPluginException(@$"Unable to load plugin {pluginAttribute.PluginType} from {pluginAttribute.PluginType.Assembly.Location}.
The plugin needs to be compatible with delegate {typeof(TastyPlugin).FullName}.
It must be a static extension method that accepts and returns a {typeof(TastyScope).FullName}",
                    pluginAttribute,
                    ex);


            var pluginAttributes = FindAttributes();

            foreach (var pluginAttribute in pluginAttributes)
            {
#if DEBUG
                Console.WriteLine($"Loading Plugin: {pluginAttribute.PluginType} from {pluginAttribute.PluginType.Assembly.Location}");
#endif
                try
                {
                    var methods = pluginAttribute.PluginType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == pluginAttribute.PluginEntryPoint);
                    var method = methods
                        .FirstOrDefault(m => typeof(TPluginArgument)
                        .IsAssignableFrom(m.GetParameters().FirstOrDefault()?.ParameterType))
                        ?? throw CreatePluginException(pluginAttribute);

                    var @delegate = (TPluginDelegate)Delegate.CreateDelegate(
                        typeof(TPluginDelegate),
                        null,
                        method
                    );

                    @delegate.DynamicInvoke(pluginArgument);
                }
                catch (ArgumentException ex)
                {
                    throw CreatePluginException(pluginAttribute, ex);
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
