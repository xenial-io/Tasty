using System;
using System.Runtime.CompilerServices;

using static Xenial.Tasty;

namespace OrganizingWithReflectionTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var types = typeof(Program).Assembly.GetTypes();

            foreach (var type in types)
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }

            Run(args);
        }
    }

    public static class MyFirstTests
    {
        static MyFirstTests()
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
        static MySecondTests()
        {
            Describe(nameof(MySecondTests), () =>
            {
                It("#1", () => true);
                It("#2", () => true);
            });
        }
    }
}
