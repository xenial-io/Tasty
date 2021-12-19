using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace TestGroupTests
{
    internal class Program
    {
        static Program() => ConsoleReporter.Register();

        private static void Main(string[] args)
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
