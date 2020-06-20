using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Visitors;
using System.Collections;
using System.Collections.Generic;

namespace Xenial.Delicious.Execution
{
    public delegate System.Threading.Tasks.Task TestDelegate(TestContext context);

    public class TestContext
    {
        public TestContext(TestCase currentCase, TastyScope currentScope)
        {
            CurrentCase = currentCase ?? throw new ArgumentNullException(nameof(currentCase));
            CurrentScope = currentScope ?? throw new ArgumentNullException(nameof(currentScope));
        }

        public TestCase CurrentCase { get; }
        public TastyScope CurrentScope { get; }
    }

    public static class StopWatchMiddleware
    {
        public static TestExecutor UseStopwatch(this TestExecutor executor)
            => executor.Use(async (conext, next) =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await next();
                }
                finally
                {
                    sw.Stop();

                    conext.CurrentCase.Duration = sw.Elapsed;
                }
            });
    }

    public static class ExecuteTestMiddleware
    {
        public static TestExecutor UseTestExecutor(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    var result = await context.CurrentCase.Executor.Invoke();
                    context.CurrentCase.TestOutcome = result ? TestOutcome.Success : TestOutcome.Failed;
                }
                catch (Exception exception)
                {
                    context.CurrentCase.Exception = exception;
                    context.CurrentCase.TestOutcome = TestOutcome.Failed;
                }
                await next();
            });
    }

    public static class ReportTestMiddleware
    {
        public static TestExecutor UseTestReporters(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                await context.CurrentScope.Report(context.CurrentCase);
                await next();
            });
    }

    public static class TestExecutorExtentions
    {
        public static TestExecutor Use(this TestExecutor app, Func<TestContext, Func<Task>, Task> middleware)
        {
            return app.Use(next =>
            {
                return context =>
                {
                    Func<Task> simpleNext = () => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }
    }

    public class TestExecutor
    {
        private List<Func<TestDelegate, TestDelegate>> Middlewares = new List<Func<TestDelegate, TestDelegate>>();
        public TestExecutor Use(Func<TestDelegate, TestDelegate> middleware)
        {
            Middlewares.Add(middleware);
            return this;
        }

        TastyScope Scope { get; }

        public TestExecutor(TastyScope scope)
        {
            Scope = scope;
            this.UseStopwatch()
                .UseTestExecutor()
                .UseTestReporters();
        }

        public async Task Execute()
        {
            async Task ExecuteTests(IExecutable[] executableItems)
            {
                for (var i = 0; i < executableItems.Length; i++)
                {
                    var executable = executableItems[i];
                    if (executable is TestGroup group)
                    {
                        Scope.CurrentGroup = group;
                        var sw = Stopwatch.StartNew();
                        var groupResult = await executable.Executor.Invoke();
                        if (groupResult)
                        {
                            foreach (var action in group.Executors)
                            {
                                if (action is TestCase testCase)
                                {
                                    await Execute(testCase.IsForced, testCase);
                                }
                                if (action is TestGroup testGroup)
                                {
                                    await ExecuteTests(new[] { testGroup });
                                }
                            }
                        }
                        sw.Stop();
                        group.Duration = sw.Elapsed;
                    }
                    if (executable is TestCase test)
                    {
                        await Execute(test.IsForced, test);
                    }
                }
            }

            ForceTestVisitor.MarkTestsAsForced(Scope);

            await ExecuteTests(Scope.RootExecutors.ToArray());
        }

        internal async Task Execute(Func<bool>? execute, TestCase testCase)
        {
            if (execute != null && !execute())
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                var inconclusive = testCase.IsInconclusive != null ? testCase.IsInconclusive() : false;
                if (inconclusive.HasValue && inconclusive.Value)
                {
                    testCase.TestOutcome = TestOutcome.NotRun;
                }
                else
                {
                    var ignored = testCase.IsIgnored != null ? testCase.IsIgnored() : false;
                    if (ignored.HasValue && ignored.Value)
                    {
                        testCase.TestOutcome = TestOutcome.Ignored;
                    }
                    else
                    {
                        foreach (var hook in testCase.Group?.BeforeEachHooks ?? Scope.RootBeforeEachHooks)
                        {
                            var hookResult = await hook.Executor.Invoke();
                            if (!hookResult)
                            {
                                return;
                            }
                        }

                        try
                        {
                            var result = await testCase.Executor.Invoke();
                            testCase.TestOutcome = result ? TestOutcome.Success : TestOutcome.Failed;
                        }
                        catch (Exception exception)
                        {
                            testCase.Exception = exception;
                            testCase.TestOutcome = TestOutcome.Failed;
                        }

                        foreach (var hook in testCase.Group?.AfterEachHooks ?? Scope.RootAfterEachHooks)
                        {
                            var hookResult = await hook.Executor.Invoke();
                            if (!hookResult)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                testCase.Exception = ex;
                testCase.TestOutcome = TestOutcome.Failed;
            }
            sw.Stop();
            testCase.Duration = sw.Elapsed;

            await Scope.Report(testCase);
        }
    }
}
