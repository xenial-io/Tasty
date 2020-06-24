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
            var nestedGroup = new TestGroup
            {
                Name = name,
                Executor = async () =>
                {
                    await action();
                    return true;
                },
                ParentGroup = group,
            };
            group.Executors.Add(nestedGroup);
            return group;
        }


        public static TestCase It(this TestGroup group, string name, Action action)
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
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<(bool success, string message)> action)
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
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<bool> action)
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
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<Task> action)
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
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Func<Task<(bool success, string message)>> action)
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
            AddToGroup(group, test);
            return test;
        }

        public static TestCase It(this TestGroup group, string name, Executable action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = action,
            };
            AddToGroup(group, test);
            return test;
        }

        static void AddToGroup(TestGroup group, TestCase test)
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
            test.IsIgnored = () => true;
            return test;
        }

        public static TestCase Ignored(this TestCase test, Func<bool?> predicate)
        {
            test.IsIgnored = predicate;
            return test;
        }

        public static TestCase Ignored(this TestCase test, bool ignored)
        {
            test.IsIgnored = () => ignored;
            return test;
        }

        public static TestCase Ignored(this TestCase test, string reason)
        {
            test.IsIgnored = () => true;
            test.IgnoredReason = reason;
            return test;
        }

        public static TestCase Forced(this TestCase test, Func<bool> predicate)
        {
            test.IsForced = predicate;
            return test;
        }

        public static TestGroup Forced(this TestGroup group, Func<bool> predicate)
        {
            group.IsForced = predicate;
            return group;
        }
    }
}