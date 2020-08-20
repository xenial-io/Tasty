Description: Custom Tasty Plugins
Title: Custom Tasty Plugins
Order: 20

---

To create your own plugin you have only to make sure to follow 3 simple principles:

1. Make sure you support `netstandard2.0` or if absolutely needed `netstandard2.1`
1. Provide extension methods for custom configuration
1. Provide an auto install mode that is configurable

# 1. Create a project

Because there is nothing special about a custom plugin, we start with a simple `classlib` project:

```cmd
dotnet new classlib -o MyTastyPlugin
cd MyTastyPlugin
```

Add a reference to `Xenial.Tasty.Core`.

```cmd
dotnet add package Xenial.Tasty.Core
```

> At the time of writing there is no `Xenial.Tasty.Core` yet. Just use `Xenial.Tasty` for now

Tasty uses semver and tries to be binary compatible. So in your `csproj` file you should specify the version as floating:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Because Tasty is not stable yet, we support 0.x.x -->
    <!-- Later on this should be 1.* for 1.x.x or 2.* for 2.x.x -->
    <PackageReference Include="Xenial.Tasty.Core" Version="0.*" />
  </ItemGroup>

</Project>

```

# 2. Write an extension method

We should place all plugin entry points into the same namespace: `Xenial.Delicious.Plugins`

```cs
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Plugins
{
    public static class MyTastyPlugin
    {
        public static TastyScope UseMyTastyPlugin(this TastyScope scope)
        {
            //Hook into several extension points
            return scope;
        }
    }
}
```

# 3. Provide auto install mode

Because there is no plugin discovery, `Tasty` does only a lookup in the entry assembly for a `TastyPluginAttribute` that communicates the entry point.
To make the lives for our plugin users easier, we should provide some **MSBuild** _magic_.

We need to create a `build` folder:

```cmd
mkdir build
```

Inside we need 3 files `MyTastyPluginAssemblyInfo.cs`, `MyTastyPlugin.props` and `MyTastyPlugin.targets`:

`MyTastyPluginAssemblyInfo.cs`:

```cs
[assembly: Xenial.Delicious.Plugins.TastyPlugin(
    typeof(Xenial.Delicious.Plugins.MyTastyPlugin),
    nameof(Xenial.Delicious.Plugins.MyTastyPlugin.UseMyTastyPlugin)
)]
```

`MyTastyPlugin.props`:

```xml
<Project>

  <PropertyGroup>
    <!-- We should follow the convention starting with UseTastyXXX here -->
    <UseTastyMyTastyPlugin>true</UseTastyMyTastyPlugin>
  </PropertyGroup>

</Project>
```

`MyTastyPlugin.targets`:

```xml
<Project>

  <Target Name="AddUseTastyMyTastyPlugin" BeforeTargets="CoreGenerateAssemblyInfo" Condition="$(UseTastyMyTastyPlugin)">
    <ItemGroup>
      <Compile Include="$(MSBuildThisFileDirectory)MyTastyPluginAssemblyInfo.cs" Visible="false" />
    </ItemGroup>
  </Target>

</Project>
```

This will automatically register your plugin in a project if your nuget package is used:

```xml
  <ItemGroup>
    <PackageReference Include="MyTastyPlugin" Version="x.x.x" />
  </ItemGroup>
```

Endusers will be able to prevent your plugin from be automatically registered by using the `MSBuild` property.

```xml
  <PropertyGroup>
    <!-- Disable auto registration -->
    <UseTastyMyTastyPlugin>false</UseTastyMyTastyPlugin>
  </PropertyGroup>
```

> There is currently no easy way to automatically register project references. So you should use either the extension methods or create a nuget package.

# 4. Pack the plugin

There is nothing special about the nuget itself, so use the normal way of packing and distributing your package:

```cmd
dotnet pack
```

# Congratulations

You learned about creating your own delicious plugin!
