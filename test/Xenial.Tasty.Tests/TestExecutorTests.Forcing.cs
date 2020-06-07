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
                        ClearBeforeRun = false
                    };
                    var action = A.Fake<T>();
                    return (scope, action);
                }

                // It("should only run forced test case", async () =>
                // {
                //     var (scope, action) = CreateScope<Action>();
                //     var test1 = scope.It(TestName, action);
                //     var test2 = scope.FIt($"{TestName}#2", action);
                //     var test3 = scope.It($"{TestName}#3", action);

                //     await scope.Run();

                //     A.CallTo(action).MustHaveHappenedOnceExactly();
                //     scope.ShouldSatisfyAllConditions(
                //         () => test1.TestOutcome.ShouldBe(TestOutcome.NotRun),
                //         () => test2.TestOutcome.ShouldBe(TestOutcome.Success),
                //         () => test3.TestOutcome.ShouldBe(TestOutcome.NotRun)
                //     );
                // });

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
            });
        }
    }
}