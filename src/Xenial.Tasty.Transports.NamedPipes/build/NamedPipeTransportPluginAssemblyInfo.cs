﻿[assembly: Xenial.Delicious.Plugins.TastyPlugin(
    typeof(Xenial.Delicious.Plugins.RemoteReporterPlugin),
    nameof(Xenial.Delicious.Plugins.RemoteReporterPlugin.UseRemoteReporter)
)]
[assembly: Xenial.Delicious.Plugins.CommanderPlugin(
    typeof(Xenial.Delicious.Plugins.RemoteReporterPlugin),
    nameof(Xenial.Delicious.Plugins.RemoteReporterPlugin.UseRemoteReporter)
)]
