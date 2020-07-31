using System;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace OrganizingTests
{
    class Program
    {
        static Program() => ConsoleReporter.Register();

        static void Main(string[] args)
        {
            MyFirstTests.TestCases();
            MySecondTests.TestCases();

            Run(args);
        }
    }

    public static class MyFirstTests
    {
        public static void TestCases()
        {
            Describe(nameof(MyFirstTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }

    public static class MySecondTests
    {
        public static void TestCases()
        {
            Describe(nameof(MySecondTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }
}
