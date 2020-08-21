Description: Data driven tests
Title: Data driven tests
Order: 80
---
In order to do data-driven tests in `Tasty` you don't have to learn anything new!

# 1. Use a standard foreach loop with tuples

A test case is basically described by the name of the test in combination with a lambda expression, you just can use string interpolation and some loops (or `LINQ` statements if you are going the more functional way):

```cs
using System;

using static Xenial.Tasty;

namespace DataDrivenTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var cases  = new[] //We define a simple tuple array here for simplicity
            {
                (1, 2, 3),
                (99, 1, 100),
                (-1, 1, 0)
            };

            foreach(var (a, b, expected) in cases) //Deconstruct the tuple
            {
                It($"{a} + {b} = {expected}", () => // Use string interpolation for names
                {
                    var calculation = a + b;
                    return calculation == expected;
                });
            }

            Run(args);
        }
    }
}
```

Let's run it:

```cmd
dotnet run
```

```txt
👍 [00:00:00.0095]  1 + 2 = 3
👍 [00:00:00.0000]  99 + 1 = 100
👍 [00:00:00.0000]  -1 + 1 = 0

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S3 | T3
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0096] | [00:00:00.0096]
Outcome:         Success
=================================================================================================
```

# 2. Congratulations

You wrote your very first delicious data driven test! Next we look at [organizing tests](90-organize-tests.html).