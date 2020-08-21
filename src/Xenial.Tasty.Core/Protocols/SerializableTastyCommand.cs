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
}
