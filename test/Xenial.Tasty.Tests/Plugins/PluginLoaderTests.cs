
using Shouldly;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Scopes;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests.Plugins
{
    public static class PluginLoaderTests
    {
        internal static class SimplePlugin
        {
            internal static bool WasCalled;
            public static TastyScope Use(TastyScope scope)
            {
                WasCalled = true;
                return scope;
            }
        }

        internal static class ComplexPlugin
        {
            internal static bool UseScopeWasCalled;
            internal static bool UseCommanderWasCalled;
            public static TastyScope Use(TastyScope scope)
            {
                UseScopeWasCalled = true;
                return scope;
            }

            public static TastyCommander Use(TastyCommander commander)
            {
                UseCommanderWasCalled = true;
                return commander;
            }
        }

        public static void PluginLoader() => Describe(nameof(TastyPluginLoader), () =>
        {
            TastyPluginLoader CreatePluginLoader(params TastyPluginAttribute[] plugins)
                => new TastyPluginLoader
                {
                    FindAttributes = () => plugins
                };

            BeforeEach(() => SimplePlugin.WasCalled = false);

            It("should load simple plugin", async () =>
            {
                var loader = CreatePluginLoader(new TastyPluginAttribute(typeof(SimplePlugin), nameof(SimplePlugin.Use)));
                await loader.LoadPlugins(new TastyScope());
                return SimplePlugin.WasCalled;
            });

            It("should throw with non existing method", async () =>
            {
                var loader = CreatePluginLoader(new TastyPluginAttribute(typeof(SimplePlugin), "NonExistingMethod"));

                await Should.ThrowAsync<InvalidPluginException>(async () => await loader.LoadPlugins(new TastyScope()));
            });

            It("should load complex plugin with right signiture", async () =>
            {
                var loader = CreatePluginLoader(new TastyPluginAttribute(typeof(ComplexPlugin), nameof(ComplexPlugin.Use)));
                await loader.LoadPlugins(new TastyScope());
                return ComplexPlugin.UseScopeWasCalled && !ComplexPlugin.UseCommanderWasCalled;
            });
        });
    }
}
