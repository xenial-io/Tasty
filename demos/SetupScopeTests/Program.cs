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
