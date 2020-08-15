using System;

namespace Xenial.Delicious.Metadata
{
    internal interface IForceAble
    {
        Func<bool>? IsForced { get; set; }
    }
}
