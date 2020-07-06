Description: Getting started
Title: Getting started
Order: 10
---

This is a guide to get started with Tasty and to show you how Tasty works.

# 1. Create a normal console application

Tasty doesn't have any strong opinion in which context you are running tests from. For this tutorial we are going to start with a simple console application:

```cmd
dotnet new console -o MyFirstTastyTests && cd MyFirstTastyTests
```

# 2. Install the nuget package

Tasty is a micro framework, so think of it more as a library than a whole framework. In order to use Tasty we need to add the nuget package:

```cmd
dotnet add package Xenial.Tasty
```

# 3. Add the using statement

Tasty uses the C#6 syntax to reduce clutter when writing tests, so we need to add a static using import to use it:

```cs
using System;

using static Xenial.Tasty; //This will import all the static methods into the current namespace

namespace MyFirstTastyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
        }
    }
}
```

# 4. Write your first test case

Tasty is a *functional* inspired testing *micro framework* that uses the latest C# features like Tuples and inline functions to increase test readability and provide richer error messages for better UX when troubleshooting failing tests. So let's add a simple calculation test:

```cs
using System;

using static Xenial.Tasty;

namespace MyFirstTastyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //This will tell Tasty that there is an test case named '1 + 2 should be 3'
            It("1 + 2 should be 3", () =>
            {
                var calculation = 1 + 2; // Act
                var isThree = calculation == 3; // Assert

                //Tell Tasty the result by returning a Tuple with additional information
                return (isThree, $"1 + 2 should be 3 but actually was {calculation}");
            });
        }
    }
}
```

# 5. Add the run method

Because Tasty is a *micro framework*. That means it does nothing until you tell it to do so. So let's add the `Run` method and tell it to execute the tests:

```cs
using System;

using static Xenial.Tasty;

namespace MyFirstTastyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            It("1 + 2 should be 3", () =>
            {
                var calculation = 1 + 2;
                var isThree = calculation == 3;
                return (isThree, $"1 + 2 should be 3 but actually was {calculation}");
            });

            Run(args); //Tell Tasty to execute the test cases
        }
    }
}
```

# 6. Run the project and see the results

Cause it's a normal console application, we just can run it with the known dotnet commands you already know, so let's execute the project:

```cmd
dotnet run
```

We should get an output similar to this:

```txt
👍 [00:00:00.0042]  1 + 2 should be 3

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S1 | T1
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0042] | [00:00:00.0042]
Outcome:         Success
=================================================================================================
```

# 7. Congratulations

You wrote your very first delicious test! Let's look into some more [features with async code](20-async-code.html).
