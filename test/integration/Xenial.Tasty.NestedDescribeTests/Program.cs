using System;
using System.Threading.Tasks;

using static Xenial.Tasty;

namespace Xenial.Delicious.NestedDescribeTests
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Describe("A group", () =>
            {
                It("can contain a test", () => true);

                Describe("with nesting", () =>
                {
                    It("should be allowed", () => true);
                });

                Describe("that has multiple groups", () =>
                {
                    Describe("with really deep nesting", () =>
                    {
                        It("should be allowed", () => true);
                    });
                });
            });

            return await Run(args);
        }
    }
}
