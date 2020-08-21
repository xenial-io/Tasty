using System;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Reporters
{
    public class TestCaseResult
    {
        public TestCaseResult(string name, string fullName, TestOutcome testOutcome, TimeSpan duration)
        {
            Name = name;
            FullName = fullName;
            TestOutcome = testOutcome;
            Duration = duration;
        }

        public string Name { get; }
        public string FullName { get; }
        public TestOutcome TestOutcome { get; }
        public TimeSpan Duration { get; }

        public bool IsIgnored { get; init; }
        public string IgnoredReason { get; init; } = string.Empty;
        public string AdditionalMessage { get; init; } = string.Empty;
        public Exception? Exception { get; init; }
        public string? StackTrace { get; init; }
    }

    public static class TestCaseExtentions
    {
        public static TestCaseResult ToResult(this TestCase testCase)
            => (testCase ?? throw new ArgumentNullException(nameof(testCase))) switch
            {
                _ => new TestCaseResult(testCase.Name, testCase.FullName, testCase.TestOutcome, testCase.Duration)
                {
                    Exception = testCase.Exception,
                    StackTrace = testCase.Exception?.StackTrace,
                    IsIgnored = testCase.IsIgnored?.Invoke() ?? false,
                    IgnoredReason = testCase.IgnoredReason,
                    AdditionalMessage = testCase.AdditionalMessage
                }
            };
    }
}
