using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Utils;

namespace Xenial.Delicious.Reporters
{
    public static class SummaryReporter
    {
        private static readonly List<TestCase> cases = new List<TestCase>();

        public static void Register()
            => Tasty.RegisterReporter(Report)
                    .RegisterSummaryProvider(Summary);

        public static Task Report(TestCase test)
        {
            cases.Add(test);
            return Task.CompletedTask;
        }

        public static Task<TestSummary> Summary()
        {
            return Task.FromResult(new TestSummary(cases));
        }
    }

    public class TestSummary : IReadOnlyList<TestCase>
    {
        private readonly List<TestCase> tests;
        private readonly Dictionary<TestOutcome, List<TestCase>> testsPerOutCome;
        private readonly Dictionary<TestOutcome, TestResult> result;

        public TestSummary(IEnumerable<TestCase> tests)
        {
            this.tests = tests.ToList();

            testsPerOutCome = tests.GroupBy(t => t.TestOutcome).ToDictionary(t => t.Key, t => t.ToList());

            foreach (var outcome in Enum.GetValues(typeof(TestOutcome)).OfType<TestOutcome>())
            {
                if (!testsPerOutCome.ContainsKey(outcome))
                {
                    testsPerOutCome[outcome] = new List<TestCase>();
                }
            }

            result = new Dictionary<TestOutcome, TestResult>();
            foreach (var outcome in testsPerOutCome)
            {
                result[outcome.Key] = new TestResult(outcome.Value.Count, outcome.Value.Sum(t => t.Duration));
            }

            OutCome = tests.Where(t => t.TestOutcome > TestOutcome.Ignored).MinOrDefault(t => t.TestOutcome); ;
            Duration = tests.Sum(t => t.Duration);
        }

        public TestCase this[int index] => tests[index];

        public TestResult this[TestOutcome outcome] => result[outcome];

        public int Count => tests.Count;

        public TestOutcome OutCome { get; }

        public TimeSpan Duration { get; }

        public IEnumerator<TestCase> GetEnumerator() => tests.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => tests.GetEnumerator();

        public readonly struct TestResult
        {
            public TestResult(int count, TimeSpan duration)
            {
                Count = count;
                Duration = duration;
            }

            public int Count { get; }
            public TimeSpan Duration { get; }
        }
    }
}
