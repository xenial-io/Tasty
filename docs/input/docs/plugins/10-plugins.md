Description: Tasty Plugins
Title: Tasty Plugins
Order: 10
---

`Tasty` is very open and flexible from the ground up, because almost everything is based on plugins.
You can use a lot of built in plugins or create your own.

# Anatomy of a plugin

A plugin is basically a nuget package that gets introduced into your executing test executable. Plugins are registered by being *compiled* into your executable by using a pre-compile step, that injects a [TastyPluginAttribute](/api/Xenial.Delicious.Plugins/TastyPluginAttribute/) into your assembly. This indicates the default scope of tasty to load plugins on startup. If you have a more complex setup, you can use several static and/or extension methods to register the plugins into a scope.

This helps avoid assembly scanning on the startup and it is very fast. It also puts you into control.

You can control which plugin you import via MSBuild properties with a simple scheme. Some of them have more options, but most follow a simple `<UseTasty*>true</UseTasty*>` pattern.

>In the future it will be possible to enable and disable plugins via environment variables as well.

## Available plugins

Here are the available plugins per categories:

### Reporters

* [Xenial.Tasty.Reporters.Console](reporters/10-console-reporter.html)
* [Xenial.Tasty.Reporters.Nyan](reporters/20-nyan-reporter.html)

# Congratulations

You learned about delicious plugins! Let's look into how to create your own [*tasty* plugins!](20-custom-plugins.html)
