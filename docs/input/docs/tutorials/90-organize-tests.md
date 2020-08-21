Description: Organize Tests
Title: Organize Tests
Order: 90
---

We previously looked at very simple tests. This of course does not scale in larger test suites. So we need to organize our tests into multiple files and methods.
`Tasty` does not care how those tests are added to the scopes, so basically you are free to do whatever you want. Here are some useful patterns for organizing your tests:

# 1. Use static methods

Because everything is just function based, you only need to make sure to get methods invoked that contain test cases. We can do that with simple static (or non-static, you decide) methods:

```cs
using System;

using static Xenial.Tasty;

namespace OrganizingTests
{
    class Program
    {
        static void Main(string[] args)
        {
            MyFirstTests.TestCases();
            MySecondTests.TestCases();

            Run(args);
        }
    }

    public static class MyFirstTests
    {
        public static void TestCases()
        {
            Describe(nameof(MyFirstTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }

    public static class MySecondTests
    {
        public static void TestCases()
        {
            Describe(nameof(MySecondTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }
}

```

Let's run:

```cmd
dotnet run
```cmd

```txt
👍 [00:00:00.0091]  MyFirstTests #1
👍 [00:00:00.0001]  MyFirstTests #2
👍 [00:00:00.0001]  MySecondTests #1
👍 [00:00:00.0001]  MySecondTests #2

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S4 | T4
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0096] | [00:00:00.0096]
Outcome:         Success
=================================================================================================
```

As you can see, there is nothing special, just calling a couple of static methods!

# 2. Use static constructors and reflection

In order to reduce the overhead and boiler plate code you need to write, you can use just reflection and static constructors to do some kind of test discovery. This is pretty simple, let's look at some code:

```cs
using System;
using System.Runtime.CompilerServices;

using static Xenial.Tasty;

namespace OrganizingWithReflectionTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var types = typeof(Program).Assembly.GetTypes();

            foreach(var type in types)
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }

            Run(args);
        }
    }

    public static class MyFirstTests
    {
        static MyFirstTests()
        {
            Describe(nameof(MyFirstTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }

    public static class MySecondTests
    {
        static MySecondTests()
        {
            Describe(nameof(MySecondTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }
}

```

Let's run the code:

```cmd
dotnet run
```

```txt
👍 [00:00:00.0048]  MyFirstTests #1
👍 [00:00:00.0001]  MyFirstTests #2
👍 [00:00:00.0000]  MySecondTests #1
👍 [00:00:00.0001]  MySecondTests #2

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S4 | T4
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0052] | [00:00:00.0052]
Outcome:         Success
=================================================================================================
```

# 3. Congratulations

You've organized your delicious tests into separate classes! That's the end of the road for now. You've learned everything you need to know about `Tasty`! You are now a **Delicious testing** *warrior*!
