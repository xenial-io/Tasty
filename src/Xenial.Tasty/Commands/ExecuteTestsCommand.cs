using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Commands
{
    public static class ExecuteTestsCommand
    {
        public static (string name, Func<TastyScope, Task> command, string? description, bool? isDefault) Register()
            => ("e", Execute, "Execute all tests in default order", true);

        public static async Task Execute(TastyScope scope)
        {
            await Task.FromResult(true);
        }

        public static async Task<Queue<TestCase>> Execute(TestExecutor executor)
        {
            var groupQueue = new Queue<TestGroup>(executor.Scope.Descendants().OfType<TestGroup>());
            var testQueue = new Queue<TestCase>();

            while (groupQueue.Count > 0)
            {
                var currentGroup = groupQueue.Dequeue();
                await Execute(executor, groupQueue, testQueue, currentGroup);
            }

            return testQueue;
        }

        static async Task Execute(TestExecutor executor, Queue<TestGroup> groupQueue, Queue<TestCase> testQueue, TestGroup testGroup)
        {
            var app = executor.BuildTestGroupMiddleware();
            var context = new TestGroupContext(testGroup, executor.Scope, groupQueue, testQueue);
            await app(context);
        }

        public static Task<Queue<TestCase>> VisitForcedTestCases(Queue<TestCase> testQueue)
        {
            if (testQueue.Count(t => t.IsForced != null) > 0)
            {
                var forcedTestQueue = new Queue<TestCase>();
                while (testQueue.Count > 0)
                {
                    var currentTest = testQueue.Dequeue();
                    if (currentTest.IsForced != null)
                    {
                        try
                        {
                            var result = currentTest.IsForced();
                            if (result)
                            {
                                forcedTestQueue.Enqueue(currentTest);
                            }
                        }
                        catch (Exception ex)
                        {
                            currentTest.Exception = ex;
                            currentTest.TestOutcome = TestOutcome.Failed;
                        }
                    }
                }
                return Task.FromResult(forcedTestQueue);
            }

            return Task.FromResult(testQueue);
        }

        public static async Task Execute(TestExecutor executor, Queue<TestCase> testQueue)
        {
            while (testQueue.Count > 0)
            {
                var currentTest = testQueue.Dequeue();
                await Execute(executor, currentTest);
            }
        }

        private static async Task Execute(TestExecutor executor, TestCase testCase)
        {
            var app = executor.BuildTestMiddleware();
            var context = new TestExecutionContext(testCase, executor.Scope, testCase.Group);
            await app(context);
        }
    }
}
