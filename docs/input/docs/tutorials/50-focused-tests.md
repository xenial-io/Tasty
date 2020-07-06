Description: Focused tests
Title: Focused tests
Order: 50
---

In order to troubleshoot tests, or stay focused when working on only a portion of the code base, you can simply mark a test or group as **focused** eg. **forced**.

# 1. Mark single test cases as focused 

You can mark test cases with the `FIt` syntax. That will ensure only those tests will run:

```cs
using System;

using static Xenial.Tasty;

namespace FocusedTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //If there are tests marked in the tree as focused/forced, other ones will not run
            Describe(nameof(FocusedTests), () =>
            {
                It("Should not run #1", () => false);
                FIt("Should run #1", () => true); //Mark the test as focused
                FIt("Should run #2", () => true); //You can mark multiples in focus/force mode
                It("Should not run #2", () => false); //This test will also not run
            });

            Run(args);
        }
    }
}

```

Let's run:

```cmd
dotnet run
```

```txt
👍 [00:00:00.0042]  FocusedTests Should run #1
👍 [00:00:00.0000]  FocusedTests Should run #2

=================================================================================================
Summary:              F0 |              I0 |             NR2 |              S2 | T4
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0043] | [00:00:00.0043]
Outcome:         Success
=================================================================================================
```

As you can see, only the tests that are in focused mode where run. It didn't even output anything from the other tests, to reduce visual clutter, cause you are in *focus*mode and don't care about the other tests at the moment.

# 2. Mark test groups as focused

If you have a collection of tests that you want to focus on, you also can mark test groups as focused with `FDescribe`. That will mark all child groups and tests in this group to be in focus mode:

```cs
using System;

using static Xenial.Tasty;

namespace FocusedTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Describe(nameof(FocusedTests), () =>
            {
                It("Should not run #1", () => false);
                FIt("Should run #1", () => true);
                FIt("Should run #2", () => true);
                It("Should not run #2", () => false);

                FDescribe("all those tests and groups are in focus mode", () =>
                {
                    It("Focused #1", () => true);

                    Describe("even if nested", () =>
                    {
                        It("Focused #2", () => true);
                        It("Focused #3", () => true);
                    });
                });
            });

            Run(args);
        }
    }
}
```

Let's run:

```cmd
dotnet run
```

```txt
👍 [00:00:00.0042]  FocusedTests Should run #1
👍 [00:00:00.0001]  FocusedTests Should run #2
👍 [00:00:00.0001]  FocusedTests all those tests and groups are in focus mode Focused #1
👍 [00:00:00.0000]  FocusedTests all those tests and groups are in focus mode even if nested Focused #2
👍 [00:00:00.0000]  FocusedTests all those tests and groups are in focus mode even if nested Focused #3

=================================================================================================
Summary:              F0 |              I0 |             NR2 |              S5 | T7
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0046] | [00:00:00.0046]
Outcome:         Success
=================================================================================================
```

# 3. Congratulations

You wrote your very first delicious focused tests! Next you will learn how to get rapid feedback with [watch mode](60-watch-mode.html).
