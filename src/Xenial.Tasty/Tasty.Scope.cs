using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Visitors;
using static Xenial.Delicious.Visitors.TestIterator;

namespace Xenial.Delicious.Scopes
{
    public class TastyScope : TestGroup
    {
        public bool ClearBeforeRun { get; set; } = true;
        private readonly List<AsyncTestReporter> Reporters = new List<AsyncTestReporter>();
        private readonly List<AsyncTestSummaryReporter> SummaryReporters = new List<AsyncTestSummaryReporter>();

        internal TestGroup CurrentGroup { get; set; }

        public TastyScope()
        {
            Executor = () => Task.FromResult(true);
            CurrentGroup = this;
        }

        public TastyScope RegisterReporter(AsyncTestReporter reporter)
        {
            Reporters.Add(reporter);
            return this;
        }

        public TastyScope RegisterReporter(AsyncTestSummaryReporter summaryReporter)
        {
            SummaryReporters.Add(summaryReporter);
            return this;
        }

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

        public TestGroup FDescribe(string name, Action action)
            => Describe(name, action)
                .Forced(() => true);

        public TestGroup FDescribe(string name, Func<Task> action)
           => Describe(name, action)
               .Forced(() => true);

        void AddToGroup(TestGroup group)
        {
            group.ParentGroup = CurrentGroup;
            CurrentGroup.Executors.Add(group);
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

        public TestCase It(string name, Executable action)
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

        public TestCase FIt(string name, Func<Task> action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<bool> action)
           => It(name, action)
               .Forced(() => true);

        public TestCase FIt(string name, Func<Task<bool>> action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<Task<(bool result, string message)>> action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<(bool result, string message)> action)
           => It(name, action)
               .Forced(() => true);

        void AddToGroup(TestCase test)
        {
            test.Group = CurrentGroup;
            CurrentGroup.Executors.Add(test);
        }

        public void BeforeEach(Func<Task> action)
        {
            var hook = new TestBeforeEachHook(async () =>
            {
                await action();
                return true;
            }, null);

            AddToGroup(hook);
        }

        public void BeforeEach(Action action)
        {
            var hook = new TestBeforeEachHook(() =>
            {
                action();
                return Task.FromResult(true);
            }, null);

            AddToGroup(hook);
        }

        public void AfterEach(Func<Task> action)
        {
            var hook = new TestAfterEachHook(async () =>
            {
                await action();
                return true;
            }, null);

            AddToGroup(hook);
        }

        public void AfterEach(Action action)
        {
            var hook = new TestAfterEachHook(() =>
            {
                action();
                return Task.FromResult(true);
            }, null);

            AddToGroup(hook);
        }

        void AddToGroup(TestBeforeEachHook hook)
        {
            hook.Group = CurrentGroup;
            CurrentGroup.BeforeEachHooks.Add(hook);
        }

        void AddToGroup(TestAfterEachHook hook)
        {
            hook.Group = CurrentGroup;
            CurrentGroup.AfterEachHooks.Add(hook);
        }

        public async Task<int> Run(string[] args)
        {
            if (ClearBeforeRun)
            {
                try
                {
                    Console.Clear();
                }
                catch (IOException) { /* Handle is invalid */}
            }

            await new TestExecutor(this).Execute();

            var cases = this.Descendants().OfType<TestCase>().ToList();

            await Task.WhenAll(SummaryReporters
                .Select(async r =>
                {
                    await r.Invoke(cases);
                }).ToArray());

            var failedCase = cases
                .FirstOrDefault(m => m.TestOutcome == TestOutcome.Failed);

            if (failedCase != null)
            {
                return 1;
            }

            return 0;
        }

        public Task<int> Run() => Run(Array.Empty<string>());
    }
}