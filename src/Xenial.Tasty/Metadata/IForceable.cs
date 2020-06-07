using System;

namespace Xenial.Delicious.Metadata
{
    public interface IForceAble
    {
        Func<bool> IsForced { get; }
    }
}
