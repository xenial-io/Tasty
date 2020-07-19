
using System;
using System.Linq;
using System.Threading.Tasks;

using FakeItEasy;

using Shouldly;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static partial class TestExecutorTests
    {
        public static void DefaultRuntimeCases()
        {
            Describe(nameof(TestExecutor), () =>
            {
                const string testCaseName = "TestCase";
                static (TastyScope scope, TestCase test, Action action) CreateCase(string name = testCaseName)
                {
                    var scope = new TastyScope
                    {
                        ClearBeforeRun = false,
                        IsInteractiveRunHook = () => Task.FromResult(false)
                    };
                    var executor = new TestExecutor(scope);
                    var action = A.Fake<Action>();
                    var testCase = scope.It(name, action);
                    return (scope, testCase, action);
                }

                It("should run simple test", async () =>
                {
                    var (scope, test, action) = CreateCase();

                    await scope.Run();

                    test.ShouldSatisfyAllConditions(
                        () => test.TestOutcome.ShouldBe(TestOutcome.Success),
                        () => test.Name.ShouldBe(testCaseName),
                        () => test.Duration.ShouldBeGreaterThan(TimeSpan.Zero),
                        () => A.CallTo(action).MustHaveHappenedOnceExactly()
                    );
                });

                It("should report failure", async () =>
                {
                    const string errorMessage = "ErrorMessage";
                    var exception = new Exception(errorMessage);
                    var (scope, test, action) = CreateCase();
                    A.CallTo(action).Throws(() => exception);

                    await scope.Run();

                    test.ShouldSatisfyAllConditions(
                       () => test.TestOutcome.ShouldBe(TestOutcome.Failed),
                       () => test.Name.ShouldBe(testCaseName),
                       () => test.Duration.ShouldBeGreaterThan(TimeSpan.Zero),
                       () => test.Exception.ShouldBe(exception),
                       () => A.CallTo(action).MustHaveHappenedOnceExactly()
                   );
                });

                foreach (var (ignored, outcome) in new[]
                {
                    (true, TestOutcome.Ignored),
                    (false, TestOutcome.Success)
                })
                {
                    It($"should ignore a test with {ignored}", async () =>
                    {
                        var (scope, test, action) = CreateCase();
                        test.Ignored(() => ignored);

                        await scope.Run();

                        test.ShouldSatisfyAllConditions(
                            () => test.TestOutcome.ShouldBe(outcome),
                            () =>
                            {
                                if (ignored) A.CallTo(action).MustNotHaveHappened();
                                else A.CallTo(action).MustHaveHappenedOnceExactly();
                            }
                        );
                    });
                }

                It("should call an summary reporter", async () =>
                {
                    var (scope, _, _) = CreateCase();
                    var summaryReporter = A.Fake<AsyncTestSummaryReporter>();
                    scope.RegisterReporter(summaryReporter);

                    await scope.Run();

                    A.CallTo(summaryReporter).MustHaveHappenedOnceExactly();
                });

                It("should call before each for 1 test", async () =>
                {
                    var (scope, _, action) = CreateCase();
                    var beforeAction = A.Fake<Action>();

                    scope.BeforeEach(() =>
                    {
                        beforeAction();
                        return Task.CompletedTask;
                    });

                    await scope.Run();

                    A.CallTo(beforeAction).MustHaveHappenedOnceExactly()
                        .Then(A.CallTo(action).MustHaveHappenedOnceExactly());
                });

                It("should call before each for 2 tests", async () =>
                {
                    var (scope, _, action) = CreateCase();
                    var beforeAction = A.Fake<Action>();
                    var secondAction = A.Fake<Action>();

                    scope.It("SecondTest", secondAction);

                    scope.BeforeEach(() =>
                    {
                        beforeAction();
                        return Task.CompletedTask;
                    });

                    await scope.Run();

                    A.CallTo(beforeAction).MustHaveHappened()
                        .Then(A.CallTo(action).MustHaveHappenedOnceExactly());

                    A.CallTo(beforeAction).MustHaveHappenedTwiceExactly()
                            .Then(A.CallTo(secondAction).MustHaveHappenedOnceExactly());
                });

                It("should call after each for 1 test", async () =>
                {
                    var (scope, _, action) = CreateCase();
                    var afterAction = A.Fake<Action>();

                    scope.AfterEach(() =>
                    {
                        afterAction();
                        return Task.CompletedTask;
                    });

                    await scope.Run();

                    A.CallTo(action).MustHaveHappenedOnceExactly()
                        .Then(A.CallTo(afterAction).MustHaveHappenedOnceExactly());
                });

                It("should call after each for 2 tests", async () =>
                {
                    var (scope, _, action) = CreateCase();
                    var afterAction = A.Fake<Action>();
                    var secondAction = A.Fake<Action>();

                    scope.It("SecondTest", () => secondAction());

                    scope.AfterEach(() =>
                    {
                        afterAction();
                        return Task.CompletedTask;
                    });

                    await scope.Run();

                    A.CallTo(action).MustHaveHappenedOnceExactly()
                        .Then(A.CallTo(afterAction).MustHaveHappened())
                        .Then(A.CallTo(secondAction).MustHaveHappenedOnceExactly())
                        .Then(A.CallTo(afterAction).MustHaveHappened())
                        ;
                });

                It("should call after each even if test throws", async () =>
                {
                    var (scope, _, action) = CreateCase();
                    var afterAction = A.Fake<Action>();
                    A.CallTo(action).Throws(() => new Exception());
                    scope.AfterEach(() =>
                    {
                        afterAction();
                        return Task.CompletedTask;
                    });

                    await scope.Run();

                    A.CallTo(afterAction).MustHaveHappenedOnceExactly();
                });

                It("should call tests in order of declaration", async () =>
                {
                    var (scope, _, action) = CreateCase();
                    var secondAction = A.Fake<Action>();
                    scope.It("Second Test", secondAction);

                    await scope.Run();

                    A.CallTo(action).MustHaveHappenedOnceExactly()
                        .Then(A.CallTo(secondAction).MustHaveHappenedOnceExactly());
                });
            });
        }
    }
}