using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Xenial.Delicious.Plugins
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "By design")]
    public class InvalidPluginException : Exception
    {
        public InvalidPluginException(string? message, object pluginAttribute, Exception? ex) : base(message)
        {
            PluginAttribute = pluginAttribute;
            Ex = ex;
        }

        public string TastyPluginType { get; } = "";
        public string TastyPluginEntryPoint { get; } = "";
        public object PluginAttribute { get; }
        public Exception? Ex { get; }
    }
}
