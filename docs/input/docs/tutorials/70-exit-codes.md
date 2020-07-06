Description: Exit codes
Title: Exit codes
Order: 70
---

In order to communicate test failures to other tools like for example `Bullseye` and `Simplexec` and other CI/CD tools as `Azure DevOps` or `Github Actions`, Tasty returns exit-codes after executing tests. At the moment it will report `0` if all tests are successful and `1` if any test case failed.

1. Use async await at top level

By using the top level `async` `await` feature from the C# compiler, it's really easy to use that:

```cs
using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace ReturnCodes
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            It("1 + 2 should be 3", () =>
            {
                var calculation = 1 + 2;
                var isThree = calculation == 3;
                return (isThree, $"1 + 2 should be 3 but actually was {calculation}");
            });

            return await Run(args); //Tell Tasty to execute the test cases and return an exit-code
        }
    }
}

```

So let's run

```cmd
dotnet run
```

```txt
👍 [00:00:00.0047]  1 + 2 should be 3
        1 + 2 should be 3 but actually was 3

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S1 | T1
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0047] | [00:00:00.0047]
Outcome:         Success
=================================================================================================
```

So in this demo we have no failing tests, so let's look at the last exit-code:

```cmd
echo %errorlevel%
0
```

> The exit-codes are not settled yet from an API perspective and are subject to change.

# 2. Congratulations

You wrote your very first delicious test with exit-codes! Next we look at [data driven tests](80-data-driven-tests.html).
