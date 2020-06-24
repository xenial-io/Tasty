using System;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;
using static Xenial.Tasty;

namespace Xenial.Delicious.Tests
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // const string withoutGroupTestName = "Should allow test without group";
            // It(withoutGroupTestName, () =>
            // {
            //     WriteLine(withoutGroupTestName);
            // });
            Describe(nameof(Tasty), () =>
            {
                TastyScopeTests.DependencyTree();
                TestExecutorTests.DefaultRuntimeCases();
                TestExecutorTests.OverloadRuntimeCases();
                TestExecutorTests.ForcingTestCases();
                TestExecutorTests.IntegrationTests();
                // const string asyncTestName = "Should allow async await";
                // It(asyncTestName, async () =>
                // {
                //     await Task.CompletedTask;
                //     WriteLine(asyncTestName);
                // });

                // const string actionTestName = "Should allow simple action";
                // It(actionTestName, () =>
                // {
                //     WriteLine(actionTestName);
                // });

                // Describe("With boolean return type", () =>
                // {
                //     const string booleanTaskSuccessTestName = "Should allow task to succeed";
                //     It(booleanTaskSuccessTestName, () =>
                //     {
                //         WriteLine(booleanTaskSuccessTestName);
                //         return Task.FromResult(true);
                //     });

                //     const string booleanTaskFailureTestName = "Should allow task to fail";
                //     It(booleanTaskFailureTestName, () =>
                //     {
                //         WriteLine(booleanTaskFailureTestName);
                //         return Task.FromResult(false);
                //     });

                //     const string booleanSuccessTestName = "Should allow native to succeed";
                //     It(booleanSuccessTestName, () =>
                //     {
                //         WriteLine(booleanSuccessTestName);
                //         return true;
                //     });

                //     const string booleanFailureTestName = "Should allow native to fail";
                //     It(booleanFailureTestName, () =>
                //     {
                //         WriteLine(booleanFailureTestName);
                //         return false;
                //     });

                //     const string tupleSuccessTestName = "Should allow tuple to succeed";
                //     It(tupleSuccessTestName, () =>
                //     {
                //         WriteLine(tupleSuccessTestName);
                //         return (true, "This does succeed");
                //     });

                //     const string tupleFailureTestName = "Should allow tuple to fail";
                //     It(tupleFailureTestName, () =>
                //     {
                //         WriteLine(tupleFailureTestName);
                //         return (false, "This does fail");
                //     });

                //     const string taskTupleSuccessTestName = "Should allow tuple with task to succeed";
                //     It(taskTupleSuccessTestName, async () =>
                //     {
                //         WriteLine(taskTupleSuccessTestName);
                //         await Task.CompletedTask;
                //         return (true, "This does succeed");
                //     });

                //     const string taskTupleFailureTestName = "Should allow tuple with task to fail";
                //     It(taskTupleFailureTestName, async () =>
                //     {
                //         WriteLine(taskTupleFailureTestName);
                //         await Task.CompletedTask;
                //         return (false, "This does fail");
                //     });
                // });

                // const string exceptionTaskFailureTestName = "Should collect thrown exception";
                // It(exceptionTaskFailureTestName, () =>
                // {
                //     void Throw() => throw new Exception(exceptionTaskFailureTestName);

                //     Throw();
                // });

                // Enumerable.Range(1, 3).Select(number => $"Should allow parameterized test #{number}")
                //     .Select(testName => It(testName, () =>
                //     {
                //         WriteLine(testName);
                //     // }))
                //     .ToArray();

                // const string ignoredTestName = "Should allow to ignore test";

                // It(ignoredTestName, () =>
                // {
                //     WriteLine(ignoredTestName);
                // }).Ignored();

                // const string ignoredWithReasonTestName = "Should allow to ignore test with reason";

                // It(ignoredWithReasonTestName, () =>
                // {
                //     WriteLine(ignoredWithReasonTestName);
                // }).Ignored("The reason is the reason");

                // new[] { true, false }
                //     .Select(ignored => (ignoredTestName: $"Should allow to ignore test with boolean {ignored}", ignored))
                //     .Select((test) => It(test.ignoredTestName, () => WriteLine(ignoredTestName)).Ignored(test.ignored))
                //     .ToArray();

                // new[] { true, false }
                //     .Select(ignored => (ignoredTestName: $"Should allow to ignore test with predicate {ignored}", ignored))
                //     .Select((test) => It(test.ignoredTestName, () => WriteLine(ignoredTestName)).Ignored(() => test.ignored))
                //     .ToArray();

                // const string inconclusiveTestName = "Should allow a inconclusive test";
                // It(inconclusiveTestName, () =>
                // {
                //     WriteLine(inconclusiveTestName);
                // }).Inconclusive();

                // const string inconclusiveWithReasonTestName = "Should allow a inconclusive test with reason";

                // It(inconclusiveWithReasonTestName, () =>
                // {
                //     WriteLine(inconclusiveWithReasonTestName);
                // }).Inconclusive("The reason is the reason");

                // const string inconclusiveTestThatIsIgnoredTestName = "Should allow a inconclusive test that is ignored";
                // It(inconclusiveTestThatIsIgnoredTestName, () =>
                // {
                //     WriteLine(inconclusiveTestThatIsIgnoredTestName);
                // })
                // .Ignored()
                // .Inconclusive("It is ignored and should not run");

                // Describe("Allow nested describe blocks", () =>
                // {
                //     BeforeEach(() =>
                //     {
                //         WriteLine("Before each test");
                //         return Task.CompletedTask;
                //     });

                //     It("and execute the tests inside #1", () => (true, "This was called"));
                //     It("and execute the tests inside #2", () => (true, "This was called"));
                // });
            });

            return await Run(args);
        }
    }
}