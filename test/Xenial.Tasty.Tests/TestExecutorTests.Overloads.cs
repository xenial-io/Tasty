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
                        ClearBeforeRun = false
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
            });
        }
    }
}