using System;
using System.Collections.Generic;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Protocols
{
    public class SerializableTastyCommand
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

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
        private List<SerializableTestCase> testCases = new List<SerializableTestCase>();
        private List<SerializableTestGroup> testGroups = new List<SerializableTestGroup>();

        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for serialization")]
        public List<SerializableTestCase> TestCases { get => testCases; set => testCases = value ?? throw new ArgumentNullException(nameof(TestCases)); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for serialization")]
        public List<SerializableTestGroup> TestGroups { get => testGroups; set => testGroups = value ?? throw new ArgumentNullException(nameof(TestGroups)); }
        public TestOutcome TestOutcome { get; set; } = TestOutcome.NotRun;
        public TimeSpan Duration { get; set; }
        public bool? IsForced { get; set; }
        public Exception? Exception { get; set; }
    }
}
