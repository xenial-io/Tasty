using System;
using System.ComponentModel;

using Xenial.Delicious.Commanders;

namespace Xenial
{
    public static class Commander
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TastyCommander TastyDefaultCommander { get; } = new TastyCommander
        {
        };

    }
}
