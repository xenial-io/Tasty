Description: Setup and Teardown
Title: Setup and Teardown
Order: 40
---

Often while writing tests you have some setup work that needs to happen before tests run, and you have some finishing work that needs to happen after tests run. Tasty provides helper functions to handle this.

# 1. Using C# local functions to setup test cases

If you are writing unit tests that don't have complex or very expensive initialization code, you can just use local functions in combination with tuples to setup your test code. This is more a pattern than a feature in Tasty, but it's good to mention anyways, so let's look at an example:

```cs
using System;

using static Xenial.Tasty;

namespace NativeSetupTests
{
    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
            {
                return FirstName;
            }
            if(!string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(FirstName))
            {
                return LastName;
            }
            return $"{FirstName} {LastName}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Describe("Native setup", () =>
            {
                (Person p, Func<Person, bool>) CreatePerson(
                    string firstName = null,
                    string lastName = null,
                    Func<Person, bool> check = null
                )
                {
                    var p = new Person
                    {
                        FirstName = firstName,
                        LastName = lastName
                    };
                    return (p, check);
                };

                It("should allow only a first name", () =>
                {
                    var (person, check) = CreatePerson(
                        firstName: "John",
                        check: (p) => p.ToString() == "John"
                    );
                    return check(person);
                });

                It("should allow only a last name", () =>
                {
                    var (person, check) = CreatePerson(
                        lastName: "Doe",
                        check: (p) => p.ToString() == "Doe"
                    );
                    return check(person);
                });

                It("should allow last and first name", () =>
                {
                    var (person, check) = CreatePerson(
                        firstName: "John",
                        lastName: "Doe",
                        check: (p) => p.ToString() == "John Doe"
                    );
                    return check(person);
                });
            });

            Run(args);
        }
    }
}
```

Let's run the code:

```cmd
dotnet run
```

```txt
👍 [00:00:00.0080]  Native setup should allow only a first name
👍 [00:00:00.0003]  Native setup should allow only a last name
👍 [00:00:00.0003]  Native setup should allow last and first name

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S3 | T3
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0086] | [00:00:00.0086]
Outcome:         Success
=================================================================================================
```

> Those tests are a little bit constructed, because there is no need to pass the check to the local function,
> but if you deal with mocks or stubs this is really a nice pattern.
> And because there is nothing special from Tasty, every developer that is familiar with local functions and tuples
> will understand whats going on.

# 2. Repeating Setup For Many Tests

Like in XUnit frameworks, there is a functionality to run code before and after each test. You can use `BeforeEach` and `AfterEach` respectively.

For example, let's say we have a calculator, that does some addition upfront and we need to reset the result after each test:

```cs
using System;

using static Xenial.Tasty;

namespace SetupTests
{
    class Calculator
    {
        public int Result { get; private set; }

        public int Add(int a, int b) => Result += a + b;

        public int Reset() => Result = 0;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new Calculator();

            BeforeEach(() => calculator.Add(1, 1));
            AfterEach(() => calculator.Reset());

            It("2 + (2 + 4) = 8", () =>
            {
                calculator.Add(2, 4);
                return calculator.Result == 8;
            });

            It("2 + (10 + 10) = 22", () =>
            {
                calculator.Add(10, 10);
                return calculator.Result == 22;
            });

            Run(args);
        }
    }
}
```

Let's run the tests:

```cmd
dotnet run
```

```txt
👍 [00:00:00.0046]  2 + (2 + 4) = 8
👍 [00:00:00.0001]  2 + (10 + 10) = 22

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S2 | T2
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0048] | [00:00:00.0048]
Outcome:         Success
=================================================================================================
```

# 3. One-Time Setup

Currently there is no support for one-time setup, but it's on the [roadmap](https://github.com/xenial-io/Tasty/issues/12).

# 4. Scoping

By default, the before and after blocks apply to every test in a file. You can also group tests together using a `Describe` block. When they are inside a `Describe` block, the before and after blocks only apply to the tests within that `Describe` block:

```cs
using System;

using static Xenial.Tasty;

namespace SetupScopeTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Describe("Scopes", () =>
            {
                BeforeEach(() => Console.WriteLine("Before Scope"));
                AfterEach(() => Console.WriteLine("After Scope"));

                Describe("Nested #1", () =>
                {
                    BeforeEach(() => Console.WriteLine("Before Nested #1"));
                    AfterEach(() => Console.WriteLine("After Nested #1"));

                    It("test #1", () => Console.WriteLine("test #1"));
                });

                Describe("Nested #2", () =>
                {
                    BeforeEach(() => Console.WriteLine("Before Nested #2"));
                    AfterEach(() => Console.WriteLine("After Nested #2"));

                    It("test #1", () => Console.WriteLine("test #2"));
                });
            });

            Run();
        }
    }
}

```

Let's run:

```cmd
dotnet run
```

```txt
Before Nested #1
test #1
After Nested #1
👍 [00:00:00.0077]  Scopes Nested #1 test #1
Before Nested #2
test #2
After Nested #2
👍 [00:00:00.0008]  Scopes Nested #2 test #1

=================================================================================================
Summary:              F0 |              I0 |             NR0 |              S2 | T2
Time:    [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0000] | [00:00:00.0085] | [00:00:00.0085]
Outcome:         Success
=================================================================================================
```

> Note: This behavior is currently wrong and is [subject to change](https://github.com/xenial-io/Tasty/issues/13).

# 5. Congratulations

You wrote your very first delicious test lifecycle hooks! Next you will learn how to get into zen mode with [focused tests](50-focused-tests.html).
