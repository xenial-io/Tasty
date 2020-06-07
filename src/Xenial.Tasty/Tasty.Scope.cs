using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Scopes
{
    public class TastyScope
    {
        public bool ClearBeforeRun { get; set; } = true;
        private readonly List<AsyncTestReporter> Reporters = new List<AsyncTestReporter>();
        private readonly List<AsyncTestSummaryReporter> SummaryReporters = new List<AsyncTestSummaryReporter>();
        internal readonly List<IExecutable> RootExecutors = new List<IExecutable>();
        internal readonly List<IExecutable> RootBeforeEachHooks = new List<IExecutable>();
        internal readonly List<IExecutable> RootAfterEachHooks = new List<IExecutable>();
        internal TestGroup CurrentGroup { get; set; }

        public void RegisterReporter(AsyncTestReporter reporter)
            => Reporters.Add(reporter);

        public void RegisterReporter(AsyncTestSummaryReporter summaryReporter)
            => SummaryReporters.Add(summaryReporter);

        public Task Report(TestCase test)
            => Task.WhenAll(Reporters.Select(async reporter => await reporter(test)).ToArray());

        public TestGroup Describe(string name, Action action)
        {
            var group = new TestGroup
            {
                Name = name,
                Executor = () =>
                {
                    action.Invoke();
                    return Task.FromResult(true);
                },
            };
            AddToGroup(group);
            return group;
        }
        public TestGroup FDescribe(string name, Action action)
            => Describe(name, action)
                .Forced(() => true);
                
        public TestGroup Describe(string name, Func<Task> action)
        {
            var group = new TestGroup
            {
                Name = name,
                Executor = async () =>
                {
                    await action();
                    return true;
                },
            };
            AddToGroup(group);
            return group;
        }

        void AddToGroup(TestGroup group)
        {
            if (CurrentGroup == null)
            {
                RootExecutors.Add(group);
            }
            else
            {
                group.ParentGroup = CurrentGroup;
                CurrentGroup.Executors.Add(group);
            }
        }

        public TestCase It(string name, Action action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = () =>
                {
                    action.Invoke();
                    return Task.FromResult(true);
                },
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<(bool success, string message)> action)
        {
            var test = new TestCase
            {
                Name = name,
            };
            test.Executor = () =>
            {
                var (success, message) = action.Invoke();
                test.AdditionalMessage = message;
                return Task.FromResult(success);
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<bool> action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = () =>
                {
                    var result = action.Invoke();
                    return Task.FromResult(result);
                },
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<Task> action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = async () =>
                {
                    await action();
                    return true;
                },
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<Task<(bool success, string message)>> action)
        {
            var test = new TestCase
            {
                Name = name,
            };
            test.Executor = async () =>
            {
                var (success, message) = await action();
                test.AdditionalMessage = message;
                return success;
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<Task<bool>> action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = action,
            };
            AddToGroup(test);
            return test;
        }

        public TestCase FIt(string name, Action action)
            => It(name, action)
                .Forced(() => true);

        void AddToGroup(TestCase test)
        {
            if (CurrentGroup == null)
            {
                var groups = RootExecutors.OfType<TestGroup>().ToList();
                if (groups.Count > 0)
                {
                    var group = groups.First();
                    test.Group = group;
                    group.Executors.Add(test);
                }
                else
                {
                    RootExecutors.Add(test);
                }
            }
            else
            {
                test.Group = CurrentGroup;
                CurrentGroup.Executors.Add(test);
            }
        }

        public void BeforeEach(Func<Task> action)
        {
            var hook = new TestBeforeEachHook
            {
                Executor = async () =>
                {
                    await action();
                    return true;
                }
            };
            AddToGroup(hook);
        }

        public void AfterEach(Func<Task> action)
        {
            var hook = new TestAfterEachHook
            {
                Executor = async () =>
                {
                    await action();
                    return true;
                }
            };
            AddToGroup(hook);
        }

        void AddToGroup(TestBeforeEachHook hook)
        {
            if (CurrentGroup == null)
            {
                var groups = RootExecutors.OfType<TestGroup>().ToList();
                if (groups.Count > 0)
                {
                    var group = groups.First();
                    hook.Group = group;
                    group.BeforeEachHooks.Add(hook);
                }
                else
                {
                    RootBeforeEachHooks.Add(hook);
                }
            }
            else
            {
                hook.Group = CurrentGroup;
                CurrentGroup.BeforeEachHooks.Add(hook);
            }
        }

        void AddToGroup(TestAfterEachHook hook)
        {
            if (CurrentGroup == null)
            {
                var groups = RootExecutors.OfType<TestGroup>().ToList();
                if (groups.Count > 0)
                {
                    var group = groups.First();
                    hook.Group = group;
                    group.AfterEachHooks.Add(hook);
                }
                else
                {
                    RootAfterEachHooks.Add(hook);
                }
            }
            else
            {
                hook.Group = CurrentGroup;
                CurrentGroup.AfterEachHooks.Add(hook);
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        private bool ClearConsole =>
            GetConsoleWindow() != IntPtr.Zero
            || Environment.GetEnvironmentVariable("CI") == null
            || Environment.GetEnvironmentVariable("TF_BUILD") == null
            || !System.Diagnostics.Debugger.IsAttached;

        public async Task<int> Run(string[] args)
        {
            if (ClearBeforeRun && ClearConsole)
            {
                Console.Clear();
            }

            await new TestExecutor(this).Execute();

            static IEnumerable<TestCase> Cases(TestGroup group)
            {
                foreach (var @case in group.Executors)
                {
                    if (@case is TestGroup nestedGroup)
                    {
                        foreach (var item in Cases(nestedGroup))
                            yield return item;
                    }
                    if (@case is TestCase test)
                    {
                        yield return test;
                    }
                }
            }

            await Task.WhenAll(SummaryReporters
                .Select(async r =>
                {
                    var testCases = RootExecutors.OfType<TestGroup>()
                                        .SelectMany(g => Cases(g))
                                        .ToList();

                    await r.Invoke(testCases);
                }).ToArray());

            return 0;
        }

        public Task<int> Run() => Run(Array.Empty<string>());
    }
}