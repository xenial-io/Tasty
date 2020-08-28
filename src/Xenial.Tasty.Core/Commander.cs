using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Remote;

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
