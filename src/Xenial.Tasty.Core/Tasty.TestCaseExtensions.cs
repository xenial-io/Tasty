using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;

namespace Xenial
{
    public static class TastyTestCaseExtensions
    {
        public static TestGroup Describe(this TestGroup group, string name, Action action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var nestedGroup = new TestGroup
            {
                Name = name,
                Executor = () =>
                {
                    action();
                    return Task.FromResult(true);
                },
                ParentGroup = group,
            };
            group.Executors.Add(nestedGroup);
            return nestedGroup;
        }

        public static TestGroup Describe(this TestGroup group, string name, Func<Task> action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var nestedGroup = new TestGroup
            {
                Name = name,
                Executor = async () =>
                {
                    await action().ConfigureAwait(false);
                    return true;
                },
                ParentGroup = group,
            };
            group.Executors.Add(nestedGroup);
            return group;
        }


        public static TestCase It(this TestGroup group, string name, Action action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var test = new TestCase
            {
                Name = name,
                Executor = () =>
                {
                    action.Invoke();
                    return Task.FromResult(true);
                },
            };
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<(bool success, string message)> action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
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
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<bool> action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var test = new TestCase
            {
                Name = name,
                Executor = () =>
                {
                    var result = action.Invoke();
                    return Task.FromResult(result);
                },
            };
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<Task> action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var test = new TestCase
            {
                Name = name,
                Executor = async () =>
                {
                    await action().ConfigureAwait(false);
                    return true;
                },
            };
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<Task<(bool success, string message)>> action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var test = new TestCase
            {
                Name = name,
            };
            test.Executor = async () =>
            {
                var (success, message) = await action().ConfigureAwait(false);
                test.AdditionalMessage = message;
                return success;
            };
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Executable action)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            var test = new TestCase
            {
                Name = name,
                Executor = action,
            };
            AddToGroup(group, test);
            return test;
        }

        private static void AddToGroup(TestGroup group, TestCase test)
        {
            test.Group = group;
            group.Executors.Add(test);
        }

        public static TestCase FIt(this TestGroup group, string name, Action action)
            => group.It(name, action)
              .Forced(() => true);

        public static TestCase FIt(this TestGroup group, string name, Func<Task> action)
            => group.It(name, action)
                .Forced(() => true);

        public static TestCase FIt(this TestGroup group, string name, Func<bool> action)
           => group.It(name, action)
               .Forced(() => true);

        public static TestCase FIt(this TestGroup group, string name, Func<Task<bool>> action)
            => group.It(name, action)
                .Forced(() => true);

        public static TestCase FIt(this TestGroup group, string name, Func<Task<(bool result, string message)>> action)
            => group.It(name, action)
                .Forced(() => true);

        public static TestCase FIt(this TestGroup group, string name, Func<(bool result, string message)> action)
            => group.It(name, action)
               .Forced(() => true);

        public static TestCase Ignored(this TestCase test)
        {
            _ = test ?? throw new ArgumentNullException(nameof(test));
            test.IsIgnored = () => true;
            return test;
        }

        public static TestCase Ignored(this TestCase test, Func<bool?> predicate)
        {
            _ = test ?? throw new ArgumentNullException(nameof(test));
            test.IsIgnored = predicate;
            return test;
        }

        public static TestCase Ignored(this TestCase test, bool ignored)
        {
            _ = test ?? throw new ArgumentNullException(nameof(test));
            test.IsIgnored = () => ignored;
            return test;
        }

        public static TestCase Ignored(this TestCase test, string reason)
        {
            _ = test ?? throw new ArgumentNullException(nameof(test));
            test.IsIgnored = () => true;
            test.IgnoredReason = reason;
            return test;
        }

        public static TestCase Forced(this TestCase test, Func<bool> predicate)
        {
            _ = test ?? throw new ArgumentNullException(nameof(test));
            test.IsForced = predicate;
            return test;
        }

        public static TestGroup Forced(this TestGroup group, Func<bool> predicate)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));
            group.IsForced = predicate;
            return group;
        }
    }
}
