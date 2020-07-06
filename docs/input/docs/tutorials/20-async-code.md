Description: Async code
Title: Async code
Order: 20
---
Cause Tasty is async all the way through testing async code is pretty easy:

# 1. Write the async test code 

There is nothing special about executing async code in Tasty. Just use `async` and `await` if you would expect with any C# application:

```cs
using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace AsyncTastyTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Mark the execution callback async
            It("I'm async and happy about it", async () =>
            {
                await Task.Delay(100); // Do async computation and await it
                return true; //We could omit that, but that's for the next lesson
            });
        }
    }
}

```

# 2. Use top level async await to run

We use the top level await feature of C# to run the tests asynchronous:

```cs
using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace AsyncTastyTests
{
    class Program
    {
        //Make sure we have an async main
        static async Task Main(string[] args)
        {
            It("I'm async and happy about it", async () =>
            {
                await Task.Delay(100);
                return true;
            });

            await Run(args); //await the results
        }
    }
}

```

# 3. Run the project and see the results 

Let's run and look:

```cmd
dotnet run
```

```txt
👍 [00:00:00.1213]  I'm async and happy about it

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S1 | T1
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.1213] | [00:00:00.1213]
Outcome:         Success
=================================================================================================
```

# 4. Congratulations

You wrote your very first async delicious test! Let's look into some more [features with test groups](30-test-groups.html).
