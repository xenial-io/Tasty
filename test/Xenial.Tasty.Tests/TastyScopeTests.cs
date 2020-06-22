using System.Linq;

using Shouldly;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Utils;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static class TastyScopeTests
    {
        public static void DependencyTree()
        {
            Describe(nameof(TastyScope), () =>
            {
                const string testDescription = "TestDescription";
                static (TastyScope scope, TestGroup group) CreateScope(string description = testDescription)
                {
                    var scope = new TastyScope
                    {
                        ClearBeforeRun = false
                    };
                    var group = scope.Describe(description, () => { });
                    return (scope, group);
                }

                It("should allow single describe block", () =>
                {
                    var (_, group) = CreateScope();

                    group.Name.ShouldBe(testDescription);
                });

                It("should allow multiple describe blocks with the same name", () =>
                {
                    var (scope, group) = CreateScope();
                    Should.NotThrow(() => scope.Describe(testDescription, () => { }));
                });

                It("should allow nested groups", async () =>
                {
                    var (scope, _) = CreateScope();
                    var groupA = scope.Describe("Parent", () =>
                    {
                        var groupB = scope.Describe("Child", () => { });
                        groupB.ParentGroup.ShouldSatisfyAllConditions(
                            () => groupB.ParentGroup.ShouldNotBeNull(),
                            () => groupB.ParentGroup?.Name.ShouldBe("Parent"),
                            () => groupB.Name.ShouldBe("Child")
                        );
                    });
                    await scope.Run();
                    groupA.Executors.ShouldSatisfyAllConditions(
                        () => groupA.Executors.Count.ShouldBe(1),
                        () => groupA.Executors.FirstOrDefault().ShouldBeOfType<TestGroup>(),
                        () => groupA.Executors.First().As<TestGroup>()!.Name.ShouldBe("Child")
                    );
                });

                foreach (var exitCode in new[] { 0, 1 })
                {
                    It($"{nameof(Run)} should exit with zero-code {exitCode}", async () =>
                    {
                        var (scope, group) = CreateScope();

                        group.It("TestCase", () =>
                        {
                            var foo = exitCode == 0;
                            return foo;
                        });

                        var result = await scope.Run();
                        result.ShouldBe(exitCode);
                    });
                }
            });
        }
    }
}
