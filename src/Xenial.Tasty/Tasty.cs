using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

namespace Xenial
{
    public static class Tasty
    {
        static Tasty()
        {
            DefaultScope = new TastyScope();
            ConsoleReporter.Register();
        }

        private static readonly TastyScope DefaultScope;

        public static void RegisterReporter(AsyncTestReporter reporter)
            => DefaultScope.RegisterReporter(reporter);

        public static void RegisterReporter(AsyncTestSummaryReporter summaryReporter)
            => DefaultScope.RegisterReporter(summaryReporter);

        public static Task Report(TestCase test)
            => DefaultScope.Report(test);

        public static TestGroup Describe(string name, Action action)
            => DefaultScope.Describe(name, action);

        public static TestGroup Describe(string name, Func<Task> action)
            => DefaultScope.Describe(name, action);

        public static TestCase It(string name, Action action)
            => DefaultScope.It(name, action);

        public static TestCase It(string name, Func<bool> action)
            => DefaultScope.It(name, action);

        public static TestCase It(string name, Func<Task> action)
            => DefaultScope.It(name, action);

        public static TestCase It(string name, Func<Task<bool>> action)
            => DefaultScope.It(name, action);

        public static TestCase It(string name, Func<(bool success, string message)> action)
           => DefaultScope.It(name, action);

        public static TestCase It(string name, Func<Task<(bool success, string message)>> action)
          => DefaultScope.It(name, action);

        public static void BeforeEach(Func<Task> action)
            => DefaultScope.BeforeEach(action);

        public static Task<int> Run(string[] args)
            => DefaultScope.Run(args);

        public static Task<int> Run()
            => DefaultScope.Run();
    }
}