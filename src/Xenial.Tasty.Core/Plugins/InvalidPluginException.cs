using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Xenial.Delicious.Plugins
{
    [Serializable]
    public class InvalidPluginException : Exception
    {
        public string TastyPluginType { get; }
        public string TastyPluginEntryPoint { get; }

        internal InvalidPluginException(string message, TastyPluginAttribute pluginAttribute, Exception innerException)
            : base(message, innerException)
        {
            if (pluginAttribute == null)
            {
                throw new ArgumentNullException(nameof(pluginAttribute));
            }

            TastyPluginType = pluginAttribute.TastyPluginType.FullName;
            TastyPluginEntryPoint = pluginAttribute.TastyPluginEntryPoint;
        }

        protected InvalidPluginException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => (TastyPluginType, TastyPluginEntryPoint)
            = (
                info.GetString(nameof(TastyPluginType)),
                info.GetString(nameof(TastyPluginEntryPoint))
            );

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(TastyPluginType), TastyPluginType);
            info.AddValue(nameof(TastyPluginEntryPoint), TastyPluginEntryPoint);

            base.GetObjectData(info, context);
        }
    }
}