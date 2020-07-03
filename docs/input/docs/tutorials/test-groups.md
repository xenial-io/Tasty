Description: Test Groups / Describe
Order: 30
---

In Tasty there are no test categories like in other XUnit test frameworks. Tasty uses test groups / describes as an abstraction. You can nest multiple groups and mix and match multiple groups and tests. Test groups can have the same name and will result in the *same* group.

# 1. Write the test groups

```cs
using System;

using static Xenial.Tasty;

namespace TestGroupTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Describe("My Test Group", () =>
            {
                It("should succeed", () => true);
            });

            Describe("My Test Group #2", () =>
            {
                It("should succeed", () => true);
            });

            Describe("My Test Group", () => //You can use the same name multiple times
            {
                It("should succeed too", () => true);
            });

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
👍 [00:00:00.0047]  My Test Group should succeed
👍 [00:00:00.0001]  My Test Group #2 should succeed
👍 [00:00:00.0000]  My Test Group should succeed too

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S3 | T3
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0050] | [00:00:00.0050]
Outcome:         Success
=================================================================================================
```

As you can see you can name the test groups the same, that helps to structure larger tests or group them together with top level concepts like `Describe("Integration")` or `Describe("Unit")`.

# 2. Nested test groups

You can nest multiple groups and tests together whatever way you want:

```cs
using System;

using static Xenial.Tasty;

namespace TestGroupTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Describe("My Test Group", () =>
            {
                It("should succeed", () => true);
            });

            Describe("My Test Group #2", () =>
            {
                It("should succeed", () => true);
            });

            Describe("My Test Group", () =>
            {
                It("should succeed too", () => true);
            });

            //We can nest them as deep as we like,
            //In any order, mixed with tests all together
            Describe("I contain", () =>
            {
                Describe("not only one", () =>
                {
                    Describe("but two or more", () =>
                    {
                        It("nested groups", () => true);
                    });
                });

                It("test cases", () => true);

                Describe("multiple nested groups", () =>
                {
                    It("with tests", () => true);
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
👍 [00:00:00.0063]  My Test Group should succeed
👍 [00:00:00.0001]  My Test Group #2 should succeed
👍 [00:00:00.0000]  My Test Group should succeed too
👍 [00:00:00.0001]  I contain test cases
👍 [00:00:00.0000]  I contain multiple nested groups with tests
👍 [00:00:00.0001]  I contain not only one but two or more nested groups

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S6 | T6
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0068] | [00:00:00.0068]
Outcome:         Success
=================================================================================================
```

> As you probably have seen, the order in which nested test cases are executed is a bit confusing.
> We will cover that later in the lifecycle of the test pipeline.

# 3. Congratulations

You wrote your very first delicious tests with groups! As you can see it's very easy to group tests however you want. Next we are looking into the [setup and teardown feature](setup-teardown.html).
