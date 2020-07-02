Description: Setup and Teardown
Title: Setup and Teardown
Order: 40
---

Often while writing tests you have some setup work that needs to happen before tests run, and you have some finishing work that needs to happen after tests run. Tasty provides helper functions to handle this.

# 1. Using C# local functions to setup test cases

If you are writing unit tests that don't have complex or very expensive initialization code you can just use local functions in combination with tuples to setup your test code. This is more a pattern than a feature in Tasty, but anyways.let's look at an example:

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

> Those tests are a little bit constructed, cause there is no need to pass the check to the local function,
> but if you deal with mocks or stubs this is really a nice pattern.
> And cause there is nothing special from Tasty, every developer that is familiar with local functions and tuples
> will understand whats going on.

# 2. Repeating Setup For Many Tests

Like in XUnit frameworks there is a functionality to run code before each test