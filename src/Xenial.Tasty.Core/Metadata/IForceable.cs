using System;

namespace Xenial.Delicious.Metadata
{
    internal interface IForceable
    {
        Func<bool>? IsForced { get; set; }
    }
}
