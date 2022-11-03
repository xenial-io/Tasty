using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace NativeSetupTests
{
    internal class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
            {
                return FirstName;
            }
            if (!string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(FirstName))
            {
                return LastName;
            }
            return $"{FirstName} {LastName}";
        }
    }

    internal class Program
    {
        static Program() => ConsoleReporter.Register();

        private static void Main(string[] args)
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
