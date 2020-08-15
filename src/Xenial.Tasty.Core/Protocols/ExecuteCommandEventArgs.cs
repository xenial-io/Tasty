using System;
using System.Collections.Generic;
using System.Text;

namespace Xenial.Delicious.Protocols
{
    [Serializable]
    public sealed class ExecuteCommandEventArgs : EventArgs
    {
        public string CommandName { get; set; } = string.Empty;
    }
}
