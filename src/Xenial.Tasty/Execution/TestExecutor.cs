using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Execution
{
    public class TestExecutor
    {
        TastyScope Scope { get; }

        public TestExecutor(TastyScope scope)
            => Scope = scope;

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

        internal async Task Execute(Func<bool> execute, TestCase testCase)
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
