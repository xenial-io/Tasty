Description: Tasty Plugins
Title: Tasty Plugins
Order: 10
---

Cause `Tasty` is very open and flexible from the ground up almost everything is based on plugins.  
You can use a lot of built in plugins or create your own.

## Anatomy of a plugin

A plugin is basically a nuget package that get's introduced into your executing test executable. Plugins are registered by being *compiled* into your executable by using an pre-compile step, that inject an [TastyPluginAttribute](./api/Xenial.Delicious.Plugins/TastyPluginAttribute/) into your assembly. This indicates the default scope of tasty to load plugins on startup. If you have a more complex setup, you can use several static and/or extension methods to register the plugins into a scope.

This helps avoid assembly scanning on the startup and is very fast.

You can control plugin you import via MSBuild properties with a simple scheme. Some of them have more options, but most follow a simple `<UseTasty*>true</UseTasty*>` pattern.

>In the future it will be possible to enable and disable plugins via environment variables as well.

 