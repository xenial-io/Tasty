using System;
using System.Threading.Tasks;

using FakeItEasy;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Scopes;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static partial class TestExecutorTests
    {
        public static void OverloadRuntimeCases()
        {
            Describe(nameof(TestExecutor), () =>
            {
                const string TestName = "TestName";

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

                It("It should allow simple action", async () =>
                {
                    var (scope, action) = CreateScope<Action>();
                    scope.It(TestName, action);
                    await scope.Run();
                    A.CallTo(action).MustHaveHappenedOnceExactly();
                });

                It("It should allow Func<bool>", async () =>
                {
                    var (scope, action) = CreateScope<Func<bool>>();
                    scope.It(TestName, action);
                    await scope.Run();
                    A.CallTo(action).MustHaveHappenedOnceExactly();
                });

                It("It should allow Func<Task>", async () =>
                {
                    var (scope, action) = CreateScope<Func<Task>>();
                    scope.It(TestName, action);
                    await scope.Run();
                    A.CallTo(action).MustHaveHappenedOnceExactly();
                });

                It("It should allow Func<Task<bool>>", async () =>
                {
                    var (scope, action) = CreateScope<Func<Task<bool>>>();
                    scope.It(TestName, action);
                    await scope.Run();
                    A.CallTo(action).MustHaveHappenedOnceExactly();
                });

                It("It should allow Func<(bool success, string message)>", async () =>
                {
                    var (scope, action) = CreateScope<Func<(bool success, string message)>>();
                    scope.It(TestName, action);
                    await scope.Run();
                    A.CallTo(action).MustHaveHappenedOnceExactly();
                });

                It("It should allow Func<Task<(bool success, string message)>>", async () =>
                {
                    var (scope, action) = CreateScope<Func<Task<(bool success, string message)>>>();
                    scope.It(TestName, action);
                    await scope.Run();
                    A.CallTo(action).MustHaveHappenedOnceExactly();
                });

                It("It should allow Func<Task<false>>", async () =>
                {
                    var (scope, _) = CreateScope<Func<Task<bool>>>();

                    var testCase = scope.It(TestName, async () =>
                    {
                        await Task.CompletedTask;

                        return false;
                    });

                    await scope.Run();

                    return testCase.TestOutcome == Metadata.TestOutcome.Failed;
                });

                It("It should allow Func<Task<true>>", async () =>
                {
                    var (scope, _) = CreateScope<Func<Task<bool>>>();

                    var testCase = scope.It(TestName, async () =>
                    {
                        await Task.CompletedTask;

                        return true;
                    });

                    await scope.Run();
                    Console.WriteLine(testCase.TestOutcome);
                    return testCase.TestOutcome == Metadata.TestOutcome.Success;
                });
            });
        }
    }
}