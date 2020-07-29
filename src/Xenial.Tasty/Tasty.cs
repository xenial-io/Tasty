using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial
{
    /// <summary>The global Tasty Scope.</summary>
    /// <example>
    ///   <code title="Usage">using static Xenial.Tasty;
    ///
    /// Describe("Basic math:", () =&gt; 
    /// {
    ///     It("1 + 1 = 2", () =&gt; 1 + 2 == 3);
    ///
    ///     It("1 - 1 = 0", () =&gt;
    ///     {
    ///         var sub = 1 - 1var result = sub == 0;
    ///         return result;
    ///     });
    /// });
    /// 
    /// Run();</code>
    /// </example>
    public static class Tasty
    {
        static Tasty()
        {
            DefaultScope = new TastyScope();
            DefaultScope.RegisterTransport(NamedPipeRemoteHook.CreateNamedPipeTransportStream);
        }

        private static readonly TastyScope DefaultScope;

        /// <summary>
        /// Registers an async test reporter.
        /// Gets called immediate after the test was executed
        /// <see cref="AsyncTestReporter"/>
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <returns>TastyScope.</returns>
        public static TastyScope RegisterReporter(AsyncTestReporter reporter)
            => DefaultScope.RegisterReporter(reporter);

        /// <summary>
        /// Registers an async test summary reporter.
        /// Gets called after all tests are executed.
        /// <see cref="AsyncTestSummaryReporter"/>
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <returns>TastyScope.</returns>
        public static TastyScope RegisterReporter(AsyncTestSummaryReporter summaryReporter)
            => DefaultScope.RegisterReporter(summaryReporter);

        /// <summary>
        /// Reports the specified test to the configured test reporters.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns>Task.</returns>
        public static Task Report(TestCase test)
            => DefaultScope.Report(test);

        /// <summary>
        /// Adds a describe block eg. <see cref="TestGroup"/> to the current scope
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestGroup.</returns>
        public static TestGroup Describe(string name, Action action)
            => DefaultScope.Describe(name, action);

        /// <summary>
        /// Adds a describe block eg. <see cref="TestGroup"/> to the current scope
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestGroup.</returns>
        public static TestGroup Describe(string name, Func<Task> action)
            => DefaultScope.Describe(name, action);

        /// <summary>
        /// Adds a describe block eg. <see cref="TestGroup"/> to the current scope
        /// and mark all nested TestGroups and TestCases as forced/focused
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestGroup.</returns>
        public static TestGroup FDescribe(string name, Action action)
            => DefaultScope.FDescribe(name, action);

        /// <summary>
        /// Adds a describe block eg. <see cref="TestGroup"/> to the current scope
        /// and mark all nested TestGroups and TestCases as forced/focused
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestGroup.</returns>
        public static TestGroup FDescribe(string name, Func<Task> action)
            => DefaultScope.FDescribe(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase It(string name, Action action)
            => DefaultScope.It(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase It(string name, Func<bool> action)
            => DefaultScope.It(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase It(string name, Func<Task> action)
            => DefaultScope.It(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// Return true if the test is succeeded otherwise falseS
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase It(string name, Func<Task<bool>> action)
            => DefaultScope.It(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// Return true if the test is succeeded, otherwise false
        /// Return an additional message that will be reported if the test is failed
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase It(string name, Func<(bool success, string message)> action)
            => DefaultScope.It(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// Return true if the test is succeeded, otherwise false
        /// Return an additional message that will be reported if the test is failed
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase It(string name, Func<Task<(bool success, string message)>> action)
            => DefaultScope.It(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// and mark it as forced/focused
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase FIt(string name, Action action)
            => DefaultScope.FIt(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// and mark it as forced/focused
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase FIt(string name, Func<Task> action)
            => DefaultScope.FIt(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// and mark it as forced/focused
        /// Return true if the test is succeeded otherwise false
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase FIt(string name, Func<bool> action)
            => DefaultScope.FIt(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// and mark it as forced/focused
        /// Return true if the test is succeeded otherwise false
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase FIt(string name, Func<Task<bool>> action)
            => DefaultScope.FIt(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// and mark it as forced/focused
        /// Return true if the test is succeeded, otherwise false
        /// Return an additional message that will be reported if the test is failed
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase FIt(string name, Func<Task<(bool result, string message)>> action)
            => DefaultScope.FIt(name, action);

        /// <summary>
        /// Adds a <see cref="TestCase"/> to the current scope
        /// and mark it as forced/focused
        /// Return true if the test is succeeded, otherwise false
        /// Return an additional message that will be reported if the test is failed
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <returns>TestCase.</returns>
        public static TestCase FIt(string name, Func<(bool result, string message)> action)
            => DefaultScope.FIt(name, action);

        /// <summary>
        /// Add a callback that runs before each <see cref="TestCase"/> and each <see cref="TestGroup"/> in this scope
        /// </summary>
        /// <param name="action">The action.</param>
        public static void BeforeEach(Func<Task> action)
            => DefaultScope.BeforeEach(action);

        /// <summary>
        /// Add a callback that runs before each <see cref="TestCase"/> and each <see cref="TestGroup"/> in this scope
        /// </summary>
        /// <param name="action">The action.</param>
        public static void BeforeEach(Action action)
            => DefaultScope.BeforeEach(action);

        /// <summary>
        /// Add a callback that runs after each <see cref="TestCase"/> and each <see cref="TestGroup"/> in this scope
        /// </summary>
        /// <param name="action">The action.</param>
        public static void AfterEach(Func<Task> action)
            => DefaultScope.AfterEach(action);

        /// <summary>
        /// Add a callback that runs after each <see cref="TestCase"/> and each <see cref="TestGroup"/> in this scope
        /// </summary>
        /// <param name="action">The action.</param>
        public static void AfterEach(Action action)
            => DefaultScope.AfterEach(action);

        /// <summary>
        /// Runs the tests in the global scope.
        /// Returns an non zero exit code if all tests succeed
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public static Task<int> Run(string[] args)
            => DefaultScope.Run(args);

        /// <summary>
        /// Runs the tests in the global scope.
        /// Returns an non zero exit code if all tests succeed
        /// </summary>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public static Task<int> Run()
            => DefaultScope.Run();
    }
}