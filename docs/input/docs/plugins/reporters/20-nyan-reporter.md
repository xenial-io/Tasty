Description: Nyan Reporter
Title: Nyan Reporter
Order: 20
---

# Nyan Reporter

A rainbow full of joy reporter:

<img src="/assets/img/plugins/reporters/nyan-reporter.png" class="img-screenshot" alt="Nyan Reporter">

See it in action:
<img src="/assets/img/plugins/reporters/nyan-reporter.gif" class="img-screenshot" alt="Nyan Reporter Demo">

# 1. Installation

You can simply install it from the command-line

```cmd
dotnet add package Xenial.Tasty.Reporters.Nyan
```

or do it by hand via the `csproj`

```xml
<ItemGroup>
    <PackageReference Include="Xenial.Tasty.Reporters.Nyan" Version="x.x.x" />
</ItemGroup>
```

# 2. Configuration

You can disable and enable the reporter via a MSBuild property in the `csproj`

```xml
<PropertyGroup>
    <UseTastyNyanReporter>false</UseTastyNyanReporter>
</PropertyGroup>
```

# 3. Manual usage

If you are in a more advanced scenario, you can manually add a nyan reporter to a scope:

Import the namespace:

```cs
using Xenial.Delicious.Reporters;
```

Global:

```cs
NyanCatReporter.Register()
```

Local plugin:

```cs
TastyScope scope;
scope.UseNyanReporter();
```

Only reporter:

```cs
TastyScope scope;
scope.RegisterNyanReporter();
```

# 4. Api

You can look into the [Api](/api/Xenial.Delicious.Reporters/NyanReporter) and the [Sources](https://github.com/xenial-io/Tasty/blob/main/src/Xenial.Tasty.Reporters.Nyan/Reporters/Reporters.NyanCat.cs)

# 5. Congratulations

You learned about the *tasty* rainbow screaming nyan cat reporters!
