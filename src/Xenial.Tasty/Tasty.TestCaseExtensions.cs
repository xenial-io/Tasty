using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;

namespace Xenial
{
    public static class TastyTestCaseExtensions
    {
        public static TestCase It(this TestGroup group, string name, Action action)
        {
            var test = new TestCase
            {
                Name = name,
                Group = group,
                Executor = () =>
                {
                    action();
                    return Task.FromResult(true);
                }
            };
            group.Executors.Add(test);
            return test;
        }

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

        public static TestCase Inconclusive(this TestCase test)
        {
            test.IsInconclusive = () => true;
            return test;
        }

        public static TestCase Inconclusive(this TestCase test, string reason)
        {
            test.IsInconclusive = () => true;
            test.InconclusiveReason = reason;
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