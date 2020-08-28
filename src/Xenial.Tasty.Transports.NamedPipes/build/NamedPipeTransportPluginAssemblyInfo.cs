[assembly: Xenial.Delicious.Plugins.TastyPlugin(
    typeof(Xenial.Delicious.Plugins.NamedPipesTransportPlugin),
    nameof(Xenial.Delicious.Plugins.NamedPipesTransportPlugin.UseRemoteReporter)
)]
[assembly: Xenial.Delicious.Plugins.CommanderPlugin(
    typeof(Xenial.Delicious.Plugins.NamedPipesTransportPlugin),
    nameof(Xenial.Delicious.Plugins.NamedPipesTransportPlugin.UseRemoteReporter)
)]
