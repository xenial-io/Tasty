[assembly: Xenial.Delicious.Plugins.TastyPlugin(
    typeof(Xenial.Delicious.Plugins.InMemoryTransportPlugin),
    nameof(Xenial.Delicious.Plugins.InMemoryTransportPlugin.UseInMemoryTransport)
)]
[assembly: Xenial.Delicious.Plugins.CommanderPlugin(
    typeof(Xenial.Delicious.Plugins.InMemoryTransportPlugin),
    nameof(Xenial.Delicious.Plugins.InMemoryTransportPlugin.UseInMemoryTransport)
)]
