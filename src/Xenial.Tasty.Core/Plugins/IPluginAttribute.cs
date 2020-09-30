using System;

namespace Xenial.Delicious.Plugins
{
    public interface IPluginAttribute
    {
        Type PluginType { get; }
        string PluginEntryPoint { get; }
    }
}
