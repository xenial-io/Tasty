using System;
using System.Threading.Tasks;

using FakeItEasy;

using Shouldly;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static partial class TestExecutorTests
    {
        public static void ForcingTestCases()
        {
            Describe(nameof(TestExecutor), () =>
            {
                static (TastyScope scope, T action) CreateScope<T>()
                    where T : class
                {
                    var scope = new TastyScope
                    {
                        ClearBeforeRun = false,
                        IsInteractiveRunHook = () => Task.FromResult(false)
                    };
                    var action = A.Fake<T>();
                    return (scope, action);
                }

                It("should only run forced test case", async () =>
                {
                    const string testName = "A testcase";
                    var (scope, action) = CreateScope<Action>();
                    var test1 = scope.It(testName, action);
                    var test2 = scope.FIt($"{testName}#2", action);
                    var test3 = scope.It($"{testName}#3", action);

                    await scope.Run();

                    A.CallTo(action).MustHaveHappenedOnceExactly();
                    scope.ShouldSatisfyAllConditions(
                        () => test1.TestOutcome.ShouldBe(TestOutcome.NotRun),
                        () => test2.TestOutcome.ShouldBe(TestOutcome.Success),
                        () => test3.TestOutcome.ShouldBe(TestOutcome.NotRun)
                    );
                });

                It("should run all tests in a forced describe", async () =>
                {
                    var (scope, action) = CreateScope<Action>();
                    const string description = "Test Description";
                    var nestedActionThatShouldRun = A.Fake<Action>();
                    var nestedActionThatShouldNotRun = A.Fake<Action>();

                    var describe1 = scope.FDescribe($"{description}#1", action);
                    describe1.It("NestedCase1", nestedActionThatShouldRun);
                    describe1.It("NestedCase2", nestedActionThatShouldRun);
                    describe1.It("NestedCase3", nestedActionThatShouldRun);

                    var describe2 = scope.Describe($"{description}#2", action);
                    describe2.It("NestedCase1", nestedActionThatShouldNotRun);
                    describe2.It("NestedCase2", nestedActionThatShouldNotRun);
                    describe2.It("NestedCase3", nestedActionThatShouldNotRun);

                    await scope.Run();
                    scope.ShouldSatisfyAllConditions(
                        () => A.CallTo(nestedActionThatShouldRun).MustHaveHappened(3, Times.Exactly),
                        () => A.CallTo(nestedActionThatShouldNotRun).MustNotHaveHappened()
                    );
                });

                It("should run only 1 forced test in a deep tree", async () =>
                {
                    var (scope, action) = CreateScope<Action>();
                    var actionThatShouldRun1 = A.Fake<Action>();
                    var actionThatShouldRun2 = A.Fake<Action>();
                    var actionThatShouldNotRun = A.Fake<Action>();

                    var describe = scope.Describe("Root Describe", () => { });

                    var nestedGroup1 = describe.Describe("Child Describe #1", () => { });
                    nestedGroup1.It("Should NotRun #1", actionThatShouldNotRun);
                    nestedGroup1.It("Should NotRun #2", actionThatShouldNotRun);
                    nestedGroup1.It("Should NotRun #3", actionThatShouldNotRun);
                    nestedGroup1.FIt("Should Run", actionThatShouldRun1);

                    var nestedGroup2 = describe.Describe("Child Describe #2", () => { });
                    nestedGroup2.It("Should NotRun #1", actionThatShouldNotRun);
                    nestedGroup2.It("Should NotRun #2", actionThatShouldNotRun);
                    nestedGroup2.It("Should NotRun #3", actionThatShouldNotRun);

                    TestGroup? deepNestedGroup = null;
                    deepNestedGroup = nestedGroup2.Describe("Deep Describe #1", () =>
                    {
                        deepNestedGroup?.FIt("Deep nested describe", actionThatShouldRun2);
                    });

                    await scope.Run();

                    scope.ShouldSatisfyAllConditions(
                        () => A.CallTo(actionThatShouldRun1).MustHaveHappenedOnceExactly(),
                        () => A.CallTo(actionThatShouldRun2).MustHaveHappenedOnceExactly(),
                        () => A.CallTo(actionThatShouldNotRun).MustNotHaveHappened()
                    );
                });
            });
        }
    }
}