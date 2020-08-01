Description: Console Reporter
Title: Console Reporter
Order: 10
---

# Console Reporter

A very basic console reporter:

![Console Reporter](/assets/img/plugins/reporters/console-reporter.png)

# 1. Installation

You can simply install it from the command-line

```cmd
dotnet add package Xenial.Tasty.Reporters.Console
```

or do it by hand via the `csproj`

```csproj
<ItemGroup>
    <PackageReference Include="Xenial.Tasty.Reporters.Console" Version="x.x.x" />
</ItemGroup>
```

# 2. Configuration

You can disable and enable the reporter via a MSBuild property in the `csproj`

```csproj
<PropertyGroup>
    <UseTastyConsoleReporter>false</UseTastyConsoleReporter>
</PropertyGroup>
```

# 3. Color schemes

You can intercept and change color schemes:

```cs
ConsoleReporter.Scheme = new ColorScheme
{
    ErrorIcon = "🤬",
    ErrorColor = ConsoleColor.Magenta,
    SuccessIcon = "🥰",
    SuccessColor = ConsoleColor.White
};
```

# 4. Manual usage

If you are in a more advanced scenario, you can manually add a console reporter to a scope:

Import the namespace:

```cs
using Xenial.Delicious.Reporters;
```

Global:

```cs
ConsoleReporter.Register()
```

Local plugin:

```cs
TastyScope scope;
scope.UseConsoleReporter();
```

Only reporter:

```cs
TastyScope scope;
scope.RegisterConsoleReporter();
```

# 5. Api

You can look into the [Api](/api/Xenial.Delicious.Reporters/ConsoleReporter) and the [Sources](https://github.com/xenial-io/Tasty/blob/master/src/Xenial.Tasty.Reporters.Console/Reporters/Reporters.Console.cs)

# 6. Congratulations

You learned about delicious console reporters! Let's look into how to use the [power of nyan cat if you want more *rainbows*](20-nyan-reporter.html).
