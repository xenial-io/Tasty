using System;
using System.Collections.Generic;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Protocols
{
    public class SerializableTestCase
    {
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public TestOutcome TestOutcome { get; set; } = TestOutcome.NotRun;
        public Exception? Exception { get; set; }
        public bool? IsIgnored { get; set; }
        public string IgnoredReason { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string AdditionalMessage { get; set; } = string.Empty;
        public bool? IsForced { get; set; }
    }

    public class SerializableTestGroup
    {
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<SerializableTestCase> TestCases { get; set; } = new List<SerializableTestCase>();
        public List<SerializableTestGroup> TestGroups { get; set; } = new List<SerializableTestGroup>();
        public TestOutcome TestOutcome { get; set; } = TestOutcome.NotRun;
        public TimeSpan Duration { get; set; }
        public bool? IsForced { get; set; }
        public Exception? Exception { get; set; }
    }
}
